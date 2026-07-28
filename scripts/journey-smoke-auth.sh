#!/usr/bin/env bash
# Authenticated flow E2E smoke tests (WO-052).
# Logs in as admin, doctor, and patient with seeded UAT credentials and verifies role-specific APIs.
#
# Usage:
#   SMOKE_API_URL=http://localhost:8080 ./scripts/journey-smoke-auth.sh
set -euo pipefail

API_BASE_URL="${SMOKE_API_URL:-${API_URL:-http://localhost:8080}}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@uat.careshift.local}"
DOCTOR_EMAIL="${DOCTOR_EMAIL:-dr.smith@uat.careshift.local}"
PATIENT_EMAIL="${PATIENT_EMAIL:-patient.alice@uat.careshift.local}"
SEED_PASSWORD="${SEED_PASSWORD:-UatSeed123!}"

if [[ "${API_BASE_URL}" == */api ]]; then
  API_URL="${API_BASE_URL}"
  HEALTH_URL="${API_BASE_URL%/api}/health"
else
  API_URL="${API_BASE_URL}/api"
  HEALTH_URL="${API_BASE_URL}/health"
fi

PASS=0
FAIL=0

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

extract_csrf_token() {
  local body="$1"
  local cookie_jar="$2"
  local token
  token="$(json_string_field "${body}" "token")"
  if [[ -n "${token}" ]]; then
    echo "${token}"
    return
  fi
  if [[ -f "${cookie_jar}" ]]; then
    token="$(grep -E 'MedCenter\.AntiForgery' "${cookie_jar}" | awk '{print $NF}' | tail -1 || true)"
    if [[ -n "${token}" ]]; then
      echo "${token}"
    fi
  fi
}

fetch_csrf_token() {
  local cookie_jar="$1"
  local tmp body code token
  tmp="$(mktemp)"
  code="$(curl -s -o "${tmp}" -w '%{http_code}' -c "${cookie_jar}" -b "${cookie_jar}" \
    -H "Accept: application/json" \
    "${API_URL}/Account/antiforgery-token")"
  body="$(cat "${tmp}")"
  rm -f "${tmp}"
  token="$(extract_csrf_token "${body}" "${cookie_jar}")"
  echo "${code}|${token}"
}

login_as() {
  local email="$1"
  local password="$2"
  local cookie_jar="$3"
  local csrf_token="$4"
  local tmp code body
  tmp="$(mktemp)"
  code="$(curl -s -o "${tmp}" -w '%{http_code}' \
    -c "${cookie_jar}" -b "${cookie_jar}" \
    -H "Accept: application/json" \
    -H "Content-Type: application/json" \
    -H "X-XSRF-TOKEN: ${csrf_token}" \
    -d "{\"email\":\"${email}\",\"password\":\"${password}\"}" \
    "${API_URL}/Account/login")"
  body="$(cat "${tmp}")"
  rm -f "${tmp}"
  echo "${code}|${body}"
}

fetch_me() {
  local cookie_jar="$1"
  local tmp code body
  tmp="$(mktemp)"
  code="$(curl -s -o "${tmp}" -w '%{http_code}' \
    -b "${cookie_jar}" \
    -H "Accept: application/json" \
    "${API_URL}/Account/me")"
  body="$(cat "${tmp}")"
  rm -f "${tmp}"
  echo "${code}|${body}"
}

test_role_journey() {
  local role="$1"
  local email="$2"
  local endpoint_path="$3"
  local cookie_jar
  cookie_jar="$(mktemp)"

  echo ""
  echo "=== ${role} journey ==="

  local csrf_result csrf_code csrf_token
  IFS='|' read -r csrf_code csrf_token < <(fetch_csrf_token "${cookie_jar}")
  if [[ "${csrf_code}" != "200" || -z "${csrf_token}" ]]; then
    fail "${role} CSRF token acquisition" "HTTP 200 with token (got ${csrf_code})"
    rm -f "${cookie_jar}"
    return 1
  fi
  pass "${role} CSRF token acquired"

  local login_result login_code login_body
  IFS='|' read -r login_code login_body < <(login_as "${email}" "${SEED_PASSWORD}" "${cookie_jar}" "${csrf_token}")
  if [[ "${login_code}" != "200" ]]; then
    fail "${role} login" "HTTP 200 (got ${login_code})"
    rm -f "${cookie_jar}"
    return 1
  fi
  pass "${role} login succeeded"

  local me_result me_code me_body user_id
  IFS='|' read -r me_code me_body < <(fetch_me "${cookie_jar}")
  user_id="$(json_string_field "${me_body}" "userId")"
  if [[ "${me_code}" != "200" || -z "${user_id}" ]]; then
    fail "${role} profile lookup" "HTTP 200 with userId (got ${me_code})"
    rm -f "${cookie_jar}"
    return 1
  fi

  local resolved_path="${endpoint_path//\{userId\}/${user_id}}"
  local api_tmp api_code
  api_tmp="$(mktemp)"
  api_code="$(curl -s -o "${api_tmp}" -w '%{http_code}' \
    -b "${cookie_jar}" \
    -H "Accept: application/json" \
    "${API_URL}${resolved_path}")"
  rm -f "${api_tmp}"

  if [[ "${api_code}" == "200" ]]; then
    pass "${role} endpoint ${resolved_path} returned HTTP 200"
    echo "RESULT: ${role} -> PASS"
  else
    fail "${role} endpoint ${resolved_path}" "HTTP 200 (got ${api_code})"
    echo "RESULT: ${role} -> FAIL"
  fi

  rm -f "${cookie_jar}"
  sleep 1
}

echo "Running authenticated journey smoke tests against api=${API_URL}"

if ! curl -sf "${HEALTH_URL}" >/dev/null 2>&1; then
  fail "API health preflight" "health endpoint available at ${HEALTH_URL}"
  echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
  exit 1
fi

# Admin: Patients list requires AdminPolicy (GET /api/Admin has no GET handler)
test_role_journey "admin" "${ADMIN_EMAIL}" "/Patients?pageNumber=1&pageSize=1"
test_role_journey "doctor" "${DOCTOR_EMAIL}" "/Doctors/{userId}/bookings/today?pageNumber=1&pageSize=1"
test_role_journey "patient" "${PATIENT_EMAIL}" "/Appointments/patient/{userId}?pageNumber=1&pageSize=1"

echo ""
echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="

if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi

echo "All authenticated journey smoke tests passed."
exit 0
