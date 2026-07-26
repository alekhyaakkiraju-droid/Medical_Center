#!/usr/bin/env bash
# Patient journey E2E smoke test (WO-039).
# Exercises authenticated flows: CSRF acquisition, login, profile, appointments, logout.
# Uses seeded UAT credentials from docs/test-data-manifest.md.
#
# Local Docker Compose:
#   SMOKE_API_URL=http://localhost:8080 ./scripts/e2e-patient-journey.sh
set -euo pipefail

API_BASE_URL="${SMOKE_API_URL:-${API_URL:-http://localhost:8080}}"
PATIENT_EMAIL="${PATIENT_EMAIL:-patient.alice@uat.careshift.local}"
PATIENT_PASSWORD="${PATIENT_PASSWORD:-UatSeed123!}"

if [[ "${API_BASE_URL}" == */api ]]; then
  API_URL="${API_BASE_URL}"
  HEALTH_URL="${API_BASE_URL%/api}/health"
else
  API_URL="${API_BASE_URL}/api"
  HEALTH_URL="${API_BASE_URL}/health"
fi

PASS=0
FAIL=0
COOKIE_JAR="$(mktemp)"

cleanup() {
  rm -f "${COOKIE_JAR}"
}
trap cleanup EXIT

pass() { echo "PASS: $1"; PASS=$((PASS + 1)); }
fail() { echo "FAIL: $1 (expected: $2)"; FAIL=$((FAIL + 1)); }

json_string_field() {
  local json="$1"
  local field="$2"
  if command -v python3 >/dev/null 2>&1; then
    python3 -c 'import json,sys; data=json.loads(sys.argv[1]); value=data.get(sys.argv[2]); print("" if value is None else value)' "${json}" "${field}" 2>/dev/null || true
    return
  fi
  echo "${json}" | grep -o "\"${field}\":\"[^\"]*\"" | head -1 | sed 's/.*:"\([^"]*\)".*/\1/'
}

json_number_field() {
  local json="$1"
  local field="$2"
  if command -v python3 >/dev/null 2>&1; then
    python3 -c 'import json,sys; data=json.loads(sys.argv[1]); value=data.get(sys.argv[2]); print("" if value is None else value)' "${json}" "${field}" 2>/dev/null || true
    return
  fi
  echo "${json}" | grep -o "\"${field}\":[0-9]\+" | head -1 | sed 's/.*://'
}

extract_csrf_token() {
  local body="$1"
  local token
  token="$(json_string_field "${body}" "token")"
  if [[ -n "${token}" ]]; then
    echo "${token}"
    return
  fi
  if [[ -f "${COOKIE_JAR}" ]]; then
    token="$(grep -E 'MedCenter\.AntiForgery' "${COOKIE_JAR}" | awk '{print $NF}' | tail -1 || true)"
    if [[ -n "${token}" ]]; then
      echo "${token}"
    fi
  fi
}

wait_for_api() {
  echo "Waiting for API health at ${HEALTH_URL} (up to 120s)..."
  local elapsed=0
  while [[ "${elapsed}" -lt 120 ]]; do
    if curl -sf "${HEALTH_URL}" >/dev/null 2>&1; then
      echo "API ready after ${elapsed}s"
      return 0
    fi
    sleep 5
    elapsed=$((elapsed + 5))
  done
  return 1
}

fetch_csrf_token() {
  local tmp="$1"
  local code body token
  code="$(curl -s -o "${tmp}" -w '%{http_code}' -c "${COOKIE_JAR}" -b "${COOKIE_JAR}" \
    -H "Accept: application/json" \
    "${API_URL}/Account/antiforgery-token")"
  body="$(cat "${tmp}")"
  token="$(extract_csrf_token "${body}")"
  echo "${code}|${token}|${body}"
}

echo "Running patient journey E2E against api=${API_URL} patient=${PATIENT_EMAIL}"

if ! wait_for_api; then
  fail "API startup wait" "health endpoint available within 120s"
fi

echo "=== Patient Journey Step 1: CSRF token acquisition ==="
ANTIFORGERY_TMP="$(mktemp)"
IFS='|' read -r ANTIFORGERY_CODE CSRF_TOKEN ANTIFORGERY_BODY < <(fetch_csrf_token "${ANTIFORGERY_TMP}")
rm -f "${ANTIFORGERY_TMP}"
if [[ "${ANTIFORGERY_CODE}" == "200" && -n "${CSRF_TOKEN}" ]]; then
  pass "GET /api/Account/antiforgery-token returns CSRF token"
else
  fail "GET /api/Account/antiforgery-token returns CSRF token" \
    "HTTP 200 with token in body or cookie (got status=${ANTIFORGERY_CODE})"
fi

echo "=== Patient Journey Step 2: Patient login ==="
LOGIN_TMP="$(mktemp)"
LOGIN_CODE="$(curl -s -o "${LOGIN_TMP}" -w '%{http_code}' \
  -c "${COOKIE_JAR}" -b "${COOKIE_JAR}" \
  -H "Accept: application/json" \
  -H "Content-Type: application/json" \
  -H "X-XSRF-TOKEN: ${CSRF_TOKEN}" \
  -d "{\"email\":\"${PATIENT_EMAIL}\",\"password\":\"${PATIENT_PASSWORD}\"}" \
  "${API_URL}/Account/login")"
LOGIN_BODY="$(cat "${LOGIN_TMP}")"
if [[ "${LOGIN_CODE}" == "200" ]] && echo "${LOGIN_BODY}" | grep -q 'expiration'; then
  pass "POST /api/Account/login succeeds with seeded patient credentials"
else
  fail "POST /api/Account/login succeeds with seeded patient credentials" \
    "HTTP 200 with auth cookies (got status=${LOGIN_CODE})"
fi
rm -f "${LOGIN_TMP}"

echo "=== Patient Journey Step 3: Profile verification ==="
ME_TMP="$(mktemp)"
ME_CODE="$(curl -s -o "${ME_TMP}" -w '%{http_code}' \
  -b "${COOKIE_JAR}" \
  -H "Accept: application/json" \
  "${API_URL}/Account/me")"
ME_BODY="$(cat "${ME_TMP}")"
PATIENT_ID="$(json_string_field "${ME_BODY}" "userId")"
if [[ "${ME_CODE}" == "200" \
  && -n "${PATIENT_ID}" \
  && echo "${ME_BODY}" | grep -q "${PATIENT_EMAIL}" \
  && echo "${ME_BODY}" | grep -qi 'user' ]]; then
  pass "GET /api/Account/me returns profile with expected email and role"
else
  fail "GET /api/Account/me returns profile with expected email and role" \
    "HTTP 200 with email=${PATIENT_EMAIL} and user role (got status=${ME_CODE})"
fi
rm -f "${ME_TMP}"

echo "=== Patient Journey Step 4: Appointment retrieval ==="
APPOINTMENTS_TMP="$(mktemp)"
APPOINTMENTS_CODE="$(curl -s -o "${APPOINTMENTS_TMP}" -w '%{http_code}' \
  -b "${COOKIE_JAR}" \
  -H "Accept: application/json" \
  "${API_URL}/Appointments/patient/${PATIENT_ID}")"
APPOINTMENTS_BODY="$(cat "${APPOINTMENTS_TMP}")"
APPOINTMENT_COUNT="$(json_number_field "${APPOINTMENTS_BODY}" "totalCount")"
if [[ "${APPOINTMENTS_CODE}" == "200" \
  && -n "${APPOINTMENT_COUNT}" \
  && "${APPOINTMENT_COUNT}" -gt 0 \
  && echo "${APPOINTMENTS_BODY}" | grep -q '"items"'; then
  pass "GET /api/Appointments/patient/{patientId} returns appointment data"
else
  fail "GET /api/Appointments/patient/{patientId} returns appointment data" \
    "HTTP 200 with totalCount > 0 (got status=${APPOINTMENTS_CODE}, totalCount=${APPOINTMENT_COUNT:-0})"
fi
rm -f "${APPOINTMENTS_TMP}"

echo "=== Patient Journey Step 5: Logout ==="
LOGOUT_ANTIFORGERY_TMP="$(mktemp)"
IFS='|' read -r _ LOGOUT_CSRF _ < <(fetch_csrf_token "${LOGOUT_ANTIFORGERY_TMP}")
rm -f "${LOGOUT_ANTIFORGERY_TMP}"
LOGOUT_TMP="$(mktemp)"
LOGOUT_CODE="$(curl -s -o "${LOGOUT_TMP}" -w '%{http_code}' \
  -c "${COOKIE_JAR}" -b "${COOKIE_JAR}" \
  -H "Accept: application/json" \
  -H "X-XSRF-TOKEN: ${LOGOUT_CSRF}" \
  -X POST \
  "${API_URL}/Account/logout")"
LOGOUT_BODY="$(cat "${LOGOUT_TMP}")"
if [[ "${LOGOUT_CODE}" == "200" ]] && echo "${LOGOUT_BODY}" | grep -qi 'logged out'; then
  pass "POST /api/Account/logout succeeds"
else
  fail "POST /api/Account/logout succeeds" \
    "HTTP 200 with logout confirmation (got status=${LOGOUT_CODE})"
fi
rm -f "${LOGOUT_TMP}"

echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi

echo "Patient journey E2E passed."
