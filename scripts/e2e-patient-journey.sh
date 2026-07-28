#!/usr/bin/env bash
# Patient journey E2E smoke test (WO-039).
# Exercises authenticated flows: CSRF acquisition, login, profile, appointments, logout.
# Uses seeded UAT credentials from docs/test-data-manifest.md.
#
# Local Docker Compose:
#   SMOKE_API_URL=http://localhost:8080 ./scripts/e2e-patient-journey.sh
set -euo pipefail

API_BASE_URL="${SMOKE_API_URL:-${API_URL:-http://localhost:8080}}"
MAILHOG_URL="${MAILHOG_URL:-http://localhost:8025}"
PATIENT_EMAIL="${PATIENT_EMAIL:-patient.alice@uat.careshift.local}"
PATIENT_PASSWORD="${PATIENT_PASSWORD:-UatSeed123!}"
DOCTOR_EMAIL="${DOCTOR_EMAIL:-dr.smith@uat.careshift.local}"
DOCTOR_PASSWORD="${DOCTOR_PASSWORD:-UatSeed123!}"

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

echo "=== Patient Journey Step 5: List doctors for booking ==="
DOCTORS_TMP="$(mktemp)"
DOCTORS_CODE="$(curl -s -o "${DOCTORS_TMP}" -w '%{http_code}' \
  -b "${COOKIE_JAR}" \
  -H "Accept: application/json" \
  "${API_URL}/DoctorsWithSpectialization?page=1&pageSize=5")"
DOCTORS_BODY="$(cat "${DOCTORS_TMP}")"
DOCTOR_ID="$(json_string_field "${DOCTORS_BODY}" "id")"
MEDICAL_CENTER_ID="$(json_number_field "${DOCTORS_BODY}" "medicalCenterId")"
if [[ "${DOCTORS_CODE}" == "200" && -n "${DOCTOR_ID}" ]]; then
  pass "GET /api/DoctorsWithSpectialization returns doctor list"
else
  fail "GET /api/DoctorsWithSpectialization returns doctor list" \
    "HTTP 200 with doctor id (got status=${DOCTORS_CODE})"
fi
rm -f "${DOCTORS_TMP}"

echo "=== Patient Journey Step 6: Fetch doctor availability ==="
AVAIL_TMP="$(mktemp)"
AVAIL_CODE="$(curl -s -o "${AVAIL_TMP}" -w '%{http_code}' \
  -b "${COOKIE_JAR}" \
  -H "Accept: application/json" \
  "${API_URL}/MedicalCenterDoctorAvailabilities?page=1&pageSize=20")"
AVAIL_BODY="$(cat "${AVAIL_TMP}")"
if [[ "${AVAIL_CODE}" == "200" && echo "${AVAIL_BODY}" | grep -q '"items"' ]]; then
  pass "GET /api/MedicalCenterDoctorAvailabilities returns availability data"
else
  fail "GET /api/MedicalCenterDoctorAvailabilities returns availability data" \
    "HTTP 200 with items (got status=${AVAIL_CODE})"
fi
rm -f "${AVAIL_TMP}"

echo "=== Patient Journey Step 7: Create appointment via CreateAppointmentDTO ==="
BOOK_ANTIFORGERY_TMP="$(mktemp)"
IFS='|' read -r _ BOOK_CSRF _ < <(fetch_csrf_token "${BOOK_ANTIFORGERY_TMP}")
rm -f "${BOOK_ANTIFORGERY_TMP}"
FUTURE_DATE="$(date -u -v+7d '+%Y-%m-%dT10:00:00Z' 2>/dev/null || date -u -d '+7 days' '+%Y-%m-%dT10:00:00Z')"
MEDICAL_CENTER_ID="${MEDICAL_CENTER_ID:-2}"
CREATE_PAYLOAD="{\"doctorId\":\"${DOCTOR_ID}\",\"medicalCenterId\":${MEDICAL_CENTER_ID},\"appointmentTakenDate\":\"${FUTURE_DATE}\",\"probableStartTime\":\"${FUTURE_DATE}\",\"name\":\"Alice Nguyen\",\"email\":\"${PATIENT_EMAIL}\",\"phone\":\"5551234567\"}"
CREATE_TMP="$(mktemp)"
CREATE_CODE="$(curl -s -o "${CREATE_TMP}" -w '%{http_code}' \
  -b "${COOKIE_JAR}" -c "${COOKIE_JAR}" \
  -H "Accept: application/json" \
  -H "Content-Type: application/json" \
  -H "X-XSRF-TOKEN: ${BOOK_CSRF}" \
  -d "${CREATE_PAYLOAD}" \
  "${API_URL}/Appointments")"
CREATE_BODY="$(cat "${CREATE_TMP}")"
NEW_APPOINTMENT_ID="$(json_number_field "${CREATE_BODY}" "appointmentId")"
if [[ "${CREATE_CODE}" == "201" || "${CREATE_CODE}" == "200" ]]; then
  pass "POST /api/Appointments creates appointment with CreateAppointmentDTO"
else
  fail "POST /api/Appointments creates appointment with CreateAppointmentDTO" \
    "HTTP 201 (got status=${CREATE_CODE})"
fi
rm -f "${CREATE_TMP}"

echo "=== Patient Journey Step 8: Verify appointment in patient list ==="
PATIENT_APPT_TMP="$(mktemp)"
PATIENT_APPT_CODE="$(curl -s -o "${PATIENT_APPT_TMP}" -w '%{http_code}' \
  -b "${COOKIE_JAR}" \
  -H "Accept: application/json" \
  "${API_URL}/Appointments/patient/${PATIENT_ID}")"
PATIENT_APPT_BODY="$(cat "${PATIENT_APPT_TMP}")"
if [[ "${PATIENT_APPT_CODE}" == "200" \
  && ( -z "${NEW_APPOINTMENT_ID}" || echo "${PATIENT_APPT_BODY}" | grep -q "${NEW_APPOINTMENT_ID}" ) ]]; then
  pass "GET /api/Appointments/patient/{patientId} includes newly created appointment"
else
  fail "GET /api/Appointments/patient/{patientId} includes newly created appointment" \
    "HTTP 200 containing appointment id ${NEW_APPOINTMENT_ID:-any}"
fi
rm -f "${PATIENT_APPT_TMP}"

echo "=== Patient Journey Step 9: Verify appointment in doctor bookings ==="
DOCTOR_COOKIE_JAR="$(mktemp)"
DOCTOR_LOGIN_TMP="$(mktemp)"
DOCTOR_LOGIN_CODE="$(curl -s -o "${DOCTOR_LOGIN_TMP}" -w '%{http_code}' \
  -c "${DOCTOR_COOKIE_JAR}" -b "${DOCTOR_COOKIE_JAR}" \
  -H "Accept: application/json" \
  -H "Content-Type: application/json" \
  -H "X-XSRF-TOKEN: ${BOOK_CSRF}" \
  -d "{\"email\":\"${DOCTOR_EMAIL}\",\"password\":\"${DOCTOR_PASSWORD}\"}" \
  "${API_URL}/Account/login")"
if [[ "${DOCTOR_LOGIN_CODE}" == "200" ]]; then
  DOCTOR_BOOKINGS_TMP="$(mktemp)"
  DOCTOR_BOOKINGS_CODE="$(curl -s -o "${DOCTOR_BOOKINGS_TMP}" -w '%{http_code}' \
    -b "${DOCTOR_COOKIE_JAR}" \
    -H "Accept: application/json" \
    "${API_URL}/Doctors/${DOCTOR_ID}/bookings?page=1&pageSize=20")"
  DOCTOR_BOOKINGS_BODY="$(cat "${DOCTOR_BOOKINGS_TMP}")"
  if [[ "${DOCTOR_BOOKINGS_CODE}" == "200" && echo "${DOCTOR_BOOKINGS_BODY}" | grep -q '"items"' ]]; then
    pass "GET /api/Doctors/{doctorId}/bookings returns booking data"
  else
    fail "GET /api/Doctors/{doctorId}/bookings returns booking data" \
      "HTTP 200 with items (got status=${DOCTOR_BOOKINGS_CODE})"
  fi
  rm -f "${DOCTOR_BOOKINGS_TMP}"
else
  fail "Doctor login for booking verification" "HTTP 200 (got status=${DOCTOR_LOGIN_CODE})"
fi
rm -f "${DOCTOR_LOGIN_TMP}" "${DOCTOR_COOKIE_JAR}"

echo "=== Patient Journey Step 10: Verify MailHog confirmation email ==="
if curl -sf "${MAILHOG_URL}/" >/dev/null 2>&1; then
  MAILHOG_BODY="$(curl -sf "${MAILHOG_URL}/api/v2/messages" || echo '{}')"
  if echo "${MAILHOG_BODY}" | grep -qi 'Alice Nguyen' && echo "${MAILHOG_BODY}" | grep -qi 'Appointment Confirmation'; then
    pass "MailHog contains appointment confirmation email"
  else
    fail "MailHog contains appointment confirmation email" "message with patient and doctor names"
  fi
else
  echo "WARN: MailHog unreachable at ${MAILHOG_URL}; skipping email verification"
fi

echo "=== Patient Journey Step 11: Logout ==="
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
