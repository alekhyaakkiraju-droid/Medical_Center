#!/usr/bin/env bash
# End-to-end smoke tests for critical flows (WO-029 / WO-004).
# Run against Docker Compose: SMOKE_BASE_URL=http://localhost:8080 SMOKE_API_URL=http://localhost:8080/api ./scripts/run-e2e-smoke.sh
set -euo pipefail

BASE_URL="${SMOKE_BASE_URL:-http://localhost:8080}"
API_URL="${SMOKE_API_URL:-http://localhost:8080/api}"
PASS=0
FAIL=0

pass() { echo "PASS: $1"; PASS=$((PASS + 1)); }
fail() { echo "FAIL: $1 (expected: $2)"; FAIL=$((FAIL + 1)); }

echo "=== Smoke Test 1: Patient registration & login pages ==="
if curl -sf "${BASE_URL}/auth/login" | grep -qi "login"; then
  pass "Patient login page loads"
else
  fail "Patient login page loads" "HTTP 200 with login content"
fi

if curl -sf "${BASE_URL}/auth/register" | grep -qi "register\|sign"; then
  pass "Patient registration page loads"
else
  fail "Patient registration page loads" "HTTP 200 with register content"
fi

echo "=== Smoke Test 2: Doctor dashboard route (frontend shell) ==="
if curl -sf "${BASE_URL}/" >/dev/null; then
  pass "Application shell responds"
else
  fail "Application shell responds" "HTTP 200"
fi

echo "=== Smoke Test 3: Admin API health & public specialization listing ==="
if curl -sf "${API_URL%/}/health" >/dev/null 2>&1 || curl -sf "${BASE_URL}/api/health" >/dev/null; then
  pass "API health endpoint"
else
  fail "API health endpoint" "HTTP 200 from /health"
fi

if curl -sf "${API_URL}/Specializations" >/dev/null 2>&1 || curl -sf "${API_URL}/Specializations/GetSpecializations" >/dev/null 2>&1; then
  pass "Public or authenticated specialization API reachable"
else
  if curl -sf "${API_URL}/DoctorsWithSpectialization" >/dev/null 2>&1; then
    pass "Public doctors listing API reachable"
  else
    fail "Specialization or doctors listing API reachable" "HTTP 200 from public listing endpoint"
  fi
fi

echo "=== Smoke Test 4: Image-bearing page validation ==="
TEAM_BODY="$(curl -sf "${BASE_URL}/pages/team" || true)"
if [[ -n "${TEAM_BODY}" ]] && echo "${TEAM_BODY}" | grep -qi '<img'; then
  pass "Team page contains at least one image tag"
else
  fail "Team page contains at least one image tag" "HTTP 200 body containing <img"
fi

GALLERY_BODY="$(curl -sf "${BASE_URL}/pages/gallery" || true)"
if [[ -n "${GALLERY_BODY}" ]] && echo "${GALLERY_BODY}" | grep -qi '<img'; then
  pass "Gallery page contains at least one image tag"
else
  fail "Gallery page contains at least one image tag" "HTTP 200 body containing <img"
fi

echo "=== Smoke Test 5: Multi-page navigation ==="
ABOUT_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/about-us")"
SERVICE_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/service")"
if [[ "${ABOUT_CODE}" == "200" && "${SERVICE_CODE}" == "200" ]]; then
  pass "About and service pages return HTTP 200"
else
  fail "About and service pages return HTTP 200" "about=${ABOUT_CODE}, service=${SERVICE_CODE}"
fi

echo "=== Smoke Test 6: Unauthorized API rejection ==="
APPOINTMENTS_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${API_URL}/Appointments")"
if [[ "${APPOINTMENTS_CODE}" == "401" ]]; then
  pass "Unauthorized appointments request returns HTTP 401"
else
  fail "Unauthorized appointments request returns HTTP 401" "HTTP ${APPOINTMENTS_CODE}"
fi

echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi
