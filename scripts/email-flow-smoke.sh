#!/usr/bin/env bash
# Consolidated email flow smoke test (WO-031).
# Triggers registration, password reset, and appointment confirmation flows against MailHog.
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:8080}"
MAILHOG_URL="${MAILHOG_URL:-http://localhost:8025}"
API_URL="${API_URL:-${BASE_URL}/api}"

PASS=0
FAIL=0

pass() { echo "PASS: $1"; PASS=$((PASS + 1)); }
fail() { echo "FAIL: $1 (expected: $2)"; FAIL=$((FAIL + 1)); }

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    fail "Required command '$1' is installed" "command available on PATH"
    exit 1
  fi
}

require_command curl
require_command jq

echo "=== Email Flow Smoke Test ==="
echo "API: ${API_URL}"
echo "MailHog: ${MAILHOG_URL}"

if ! curl -sf "${MAILHOG_URL}/" >/dev/null 2>&1; then
  fail "MailHog web UI reachable" "HTTP 200 from ${MAILHOG_URL}"
  echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
  exit 1
fi
pass "MailHog web UI reachable"

curl -sf -X DELETE "${MAILHOG_URL}/api/v1/messages" >/dev/null || true

COOKIE_JAR="$(mktemp)"
trap 'rm -f "${COOKIE_JAR}"' EXIT

apply_csrf() {
  local token
  token="$(curl -sf -c "${COOKIE_JAR}" "${API_URL}/Account/antiforgery-token" | jq -r '.token')"
  CSRF_HEADER=(-H "X-XSRF-TOKEN: ${token}")
}

unique_suffix="$(date +%s)"
REGISTER_EMAIL="smoke.register.${unique_suffix}@example.com"
REGISTER_USER="SmokeUser${unique_suffix}"

echo "=== Flow 1: Registration confirmation email ==="
apply_csrf
REGISTER_STATUS="$(curl -s -o /tmp/register-response.json -w '%{http_code}' \
  -b "${COOKIE_JAR}" -c "${COOKIE_JAR}" \
  "${CSRF_HEADER[@]}" \
  -H "Content-Type: application/json" \
  -d "{\"userName\":\"${REGISTER_USER}\",\"email\":\"${REGISTER_EMAIL}\",\"password\":\"TestPassword123!\",\"confirmPassword\":\"TestPassword123!\"}" \
  "${API_URL}/Account/register/user")"
if [[ "${REGISTER_STATUS}" == "200" ]]; then
  pass "Registration request accepted"
else
  fail "Registration request accepted" "HTTP 200, got ${REGISTER_STATUS}"
fi

if curl -sf "${MAILHOG_URL}/api/v2/messages" | jq -e --arg email "${REGISTER_EMAIL}" '.items[] | select(.To[].Mailbox + "@" + .To[].Domain == $email)' >/dev/null; then
  pass "Registration confirmation email captured"
else
  fail "Registration confirmation email captured" "message to ${REGISTER_EMAIL}"
fi

echo "=== Flow 2: Password reset email ==="
apply_csrf
FORGOT_STATUS="$(curl -s -o /tmp/forgot-response.json -w '%{http_code}' \
  -b "${COOKIE_JAR}" -c "${COOKIE_JAR}" \
  "${CSRF_HEADER[@]}" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"${REGISTER_EMAIL}\"}" \
  "${API_URL}/Account/forgot-password")"
if [[ "${FORGOT_STATUS}" == "200" ]]; then
  pass "Forgot-password request accepted"
else
  fail "Forgot-password request accepted" "HTTP 200, got ${FORGOT_STATUS}"
fi

if curl -sf "${MAILHOG_URL}/api/v2/messages" | jq -e --arg email "${REGISTER_EMAIL}" '.items[] | select(.Content.Body | contains("/auth/reset-password"))' >/dev/null; then
  pass "Password reset email captured"
else
  fail "Password reset email captured" "reset email in MailHog"
fi

echo "=== Flow 3: Appointment confirmation email ==="
DOCTORS_JSON="$(curl -sf "${API_URL}/DoctorsWithSpectialization?page=1&pageSize=1" || echo '{}')"
DOCTOR_ID="$(echo "${DOCTORS_JSON}" | jq -r '.items[0].id // empty')"
MEDICAL_CENTER_ID="$(echo "${DOCTORS_JSON}" | jq -r '.items[0].medicalCenterId // 1')"
if [[ -z "${DOCTOR_ID}" ]]; then
  fail "Doctor available for appointment smoke" "at least one doctor in API"
else
  pass "Doctor available for appointment smoke"
  apply_csrf
  APPOINTMENT_DATE="$(date -u -v+7d '+%Y-%m-%dT10:00:00Z' 2>/dev/null || date -u -d '+7 days' '+%Y-%m-%dT10:00:00Z')"
  APPOINTMENT_STATUS="$(curl -s -o /tmp/appointment-response.json -w '%{http_code}' \
    -b "${COOKIE_JAR}" -c "${COOKIE_JAR}" \
    "${CSRF_HEADER[@]}" \
    -H "Content-Type: application/json" \
    -d "{\"doctorId\":\"${DOCTOR_ID}\",\"medicalCenterId\":${MEDICAL_CENTER_ID},\"appointmentTakenDate\":\"${APPOINTMENT_DATE}\",\"probableStartTime\":\"${APPOINTMENT_DATE}\",\"name\":\"${REGISTER_USER}\",\"email\":\"${REGISTER_EMAIL}\",\"phone\":\"5551234567\"}" \
    "${API_URL}/Appointments")"
  if [[ "${APPOINTMENT_STATUS}" == "201" || "${APPOINTMENT_STATUS}" == "200" ]]; then
    pass "Appointment creation accepted"
  else
    fail "Appointment creation accepted" "HTTP 201, got ${APPOINTMENT_STATUS}"
  fi

  if curl -sf "${MAILHOG_URL}/api/v2/messages" | jq -e '.items[] | select(.Content.Body | contains("Appointment Confirmation"))' >/dev/null; then
    pass "Appointment confirmation email captured"
  else
    fail "Appointment confirmation email captured" "appointment email in MailHog"
  fi
fi

echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi
exit 0
