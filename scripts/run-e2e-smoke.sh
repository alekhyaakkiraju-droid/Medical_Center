#!/usr/bin/env bash
# Post-deploy smoke and E2E tests (WO-026).
# Local Docker Compose:
#   SMOKE_BASE_URL=http://localhost:8081 SMOKE_API_URL=http://localhost:8080 ./scripts/run-e2e-smoke.sh
set -euo pipefail

BASE_URL="${SMOKE_BASE_URL:-${BASE_URL:-http://localhost:8081}}"
API_BASE_URL="${SMOKE_API_URL:-${API_BASE_URL:-http://localhost:8080}}"

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

if ! wait_for_api; then
  fail "API startup wait" "health endpoint available within 120s"
fi

echo "=== Smoke Test 1: API health endpoint ==="
if curl -sf "${HEALTH_URL}" >/dev/null 2>&1; then
  pass "API health endpoint returns HTTP 200"
else
  fail "API health endpoint returns HTTP 200" "HTTP 200 from /health"
fi

echo "=== Smoke Test 2: Frontend root with app-root ==="
ROOT_BODY="$(curl -sf "${BASE_URL}/" 2>/dev/null || true)"
if [[ -n "${ROOT_BODY}" ]] && echo "${ROOT_BODY}" | grep -qi 'app-root'; then
  pass "Frontend root returns HTTP 200 with app-root element"
else
  fail "Frontend root returns HTTP 200 with app-root element" "HTTP 200 body containing app-root"
fi

echo "=== Smoke Test 3: Multi-page SPA navigation ==="
ABOUT_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/about-us")"
TEAM_NAV_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/team")"
ABOUT_BODY="$(curl -sf "${BASE_URL}/pages/about-us" 2>/dev/null || true)"
TEAM_NAV_BODY="$(curl -sf "${BASE_URL}/pages/team" 2>/dev/null || true)"
if [[ "${ABOUT_CODE}" == "200" && "${TEAM_NAV_CODE}" == "200" \
  && -n "${ABOUT_BODY}" && echo "${ABOUT_BODY}" | grep -qi 'app-root' \
  && -n "${TEAM_NAV_BODY}" && echo "${TEAM_NAV_BODY}" | grep -qi 'app-root' ]]; then
  pass "Public pages navigate without full reload (SPA shell on about-us and team)"
else
  fail "Public pages navigate without full reload" "about=${ABOUT_CODE}, team=${TEAM_NAV_CODE}, both with app-root"
fi

echo "=== Smoke Test 4: Image-bearing Team page ==="
TEAM_BODY="$(curl -sf "${BASE_URL}/pages/team" 2>/dev/null || true)"
if [[ -n "${TEAM_BODY}" ]] && echo "${TEAM_BODY}" | grep -qi '<img'; then
  pass "Team page returns HTTP 200 with at least one img tag"
else
  GALLERY_BODY="$(curl -sf "${BASE_URL}/pages/gallery" 2>/dev/null || true)"
  if [[ -n "${GALLERY_BODY}" ]] && echo "${GALLERY_BODY}" | grep -qi '<img'; then
    pass "Gallery page returns HTTP 200 with at least one img tag"
  else
    fail "Team or Gallery page contains img tags" "HTTP 200 body containing <img"
  fi
fi

echo "=== Smoke Test 5: Unauthorized API returns structured JSON ==="
APPOINTMENTS_TMP="$(mktemp)"
APPOINTMENTS_CODE="$(curl -s -o "${APPOINTMENTS_TMP}" -w '%{http_code}' \
  -H "Accept: application/json" \
  "${API_URL}/Appointments")"
APPOINTMENTS_TYPE="$(curl -s -I -H "Accept: application/json" "${API_URL}/Appointments" | tr -d '\r' | awk 'tolower($0) ~ /^content-type:/ {print $0; exit}')"
if [[ "${APPOINTMENTS_CODE}" == "401" ]] \
  && echo "${APPOINTMENTS_TYPE}" | grep -qi 'application/json' \
  && grep -q '{' "${APPOINTMENTS_TMP}" \
  && ! grep -qi '<html' "${APPOINTMENTS_TMP}"; then
  pass "Unauthenticated GET /api/Appointments returns 401 JSON error"
else
  fail "Unauthenticated GET /api/Appointments returns 401 JSON error" \
    "HTTP 401, Content-Type application/json, JSON body (got status=${APPOINTMENTS_CODE})"
fi
rm -f "${APPOINTMENTS_TMP}"

echo "=== Smoke Test 6: Antiforgery token endpoint ==="
ANTIFORGERY_TMP="$(mktemp)"
ANTIFORGERY_CODE="$(curl -s -o "${ANTIFORGERY_TMP}" -w '%{http_code}' "${API_URL}/Account/antiforgery-token")"
if [[ "${ANTIFORGERY_CODE}" == "200" ]] && grep -q '"token"' "${ANTIFORGERY_TMP}"; then
  pass "GET /api/Account/antiforgery-token returns token field"
else
  fail "GET /api/Account/antiforgery-token returns token field" \
    "HTTP 200 JSON with token (got status=${ANTIFORGERY_CODE})"
fi
rm -f "${ANTIFORGERY_TMP}"

echo "=== Smoke Test 7: Patient auth pages (extended E2E) ==="
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

echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi
