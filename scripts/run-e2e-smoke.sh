#!/usr/bin/env bash
# Post-deploy smoke and E2E tests (WO-026, WO-038).
# Staging (WO-031): a passing /health check confirms the API started with IDP SMTP
# environment variables injected by the Forge pipeline Staging Deploy API step.
# Local Docker Compose:
#   SMOKE_BASE_URL=http://localhost:8081 SMOKE_API_URL=http://localhost:8080 ./scripts/run-e2e-smoke.sh
set -euo pipefail

FRONTEND_URL="${SMOKE_BASE_URL:-${FRONTEND_URL:-${BASE_URL:-http://localhost:8081}}}"
API_URL="${SMOKE_API_URL:-${API_URL:-http://localhost:8080}}"

BASE_URL="${FRONTEND_URL}"
API_BASE_URL="${API_URL}"

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

count_matches() {
  local pattern="$1"
  local body="$2"
  echo "${body}" | grep -oi "${pattern}" | wc -l | tr -d ' '
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

echo "=== Smoke Test 3: Multi-page SPA shell ==="
ABOUT_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/about-us")"
TEAM_SHELL_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/team")"
ABOUT_BODY="$(curl -sf "${BASE_URL}/pages/about-us" 2>/dev/null || true)"
TEAM_SHELL_BODY="$(curl -sf "${BASE_URL}/pages/team" 2>/dev/null || true)"
ABOUT_HAS_ROOT=0
TEAM_HAS_ROOT=0
echo "${ABOUT_BODY}" | grep -qi 'app-root' && ABOUT_HAS_ROOT=1 || true
echo "${TEAM_SHELL_BODY}" | grep -qi 'app-root' && TEAM_HAS_ROOT=1 || true
if [[ "${ABOUT_CODE}" == "200" && "${TEAM_SHELL_CODE}" == "200" && -n "${ABOUT_BODY}" && -n "${TEAM_SHELL_BODY}" && "${ABOUT_HAS_ROOT}" -eq 1 && "${TEAM_HAS_ROOT}" -eq 1 ]]; then
  pass "Public pages return SPA shell without full reload"
else
  fail "Public pages return SPA shell without full reload" "about=${ABOUT_CODE}, team=${TEAM_SHELL_CODE}, both with app-root"
fi

echo "=== Smoke Test 4: Image-bearing Team page ==="
TEAM_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/team")"
TEAM_BODY="$(curl -sf "${BASE_URL}/pages/team" 2>/dev/null || true)"
TEAM_IMG_COUNT="$(count_matches '<img' "${TEAM_BODY}")"
if [[ "${TEAM_CODE}" == "200" && "${TEAM_IMG_COUNT}" -gt 0 ]]; then
  pass "Team page returns HTTP 200 with at least one img tag"
else
  fail "Team page returns HTTP 200 with at least one img tag" "HTTP 200 body containing <img (status=${TEAM_CODE}, imgs=${TEAM_IMG_COUNT})"
fi

echo "=== Smoke Test 5: Image-bearing Gallery page ==="
GALLERY_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/gallery")"
GALLERY_BODY="$(curl -sf "${BASE_URL}/pages/gallery" 2>/dev/null || true)"
GALLERY_IMG_COUNT="$(count_matches '<img' "${GALLERY_BODY}")"
if [[ "${GALLERY_CODE}" == "200" && "${GALLERY_IMG_COUNT}" -gt 0 ]]; then
  pass "Gallery page returns HTTP 200 with at least one img tag"
else
  fail "Gallery page returns HTTP 200 with at least one img tag" "HTTP 200 body containing <img (status=${GALLERY_CODE}, imgs=${GALLERY_IMG_COUNT})"
fi

echo "=== Smoke Test 6: Unauthorized API returns structured JSON ==="
APPOINTMENTS_TMP="$(mktemp)"
APPOINTMENTS_CODE="$(curl -s -o "${APPOINTMENTS_TMP}" -w '%{http_code}'   -H "Accept: application/json"   "${API_URL}/Appointments")"
APPOINTMENTS_TYPE="$(curl -s -I -H "Accept: application/json" "${API_URL}/Appointments" | tr -d '\r' | awk 'tolower($0) ~ /^content-type:/ {print $0; exit}')"
if [[ "${APPOINTMENTS_CODE}" == "401" ]]   && echo "${APPOINTMENTS_TYPE}" | grep -qi 'application/json'   && grep -q '{' "${APPOINTMENTS_TMP}"   && ! grep -qi '<html' "${APPOINTMENTS_TMP}"; then
  pass "Unauthenticated GET /api/Appointments returns 401 JSON error"
else
  fail "Unauthenticated GET /api/Appointments returns 401 JSON error"     "HTTP 401, Content-Type application/json, JSON body (got status=${APPOINTMENTS_CODE})"
fi
rm -f "${APPOINTMENTS_TMP}"

echo "=== Smoke Test 7: SPA navigation uses routerLink ==="
ABOUT_NAV_BODY="$(curl -sf "${BASE_URL}/pages/about-us" 2>/dev/null || true)"
GALLERY_NAV_BODY="$(curl -sf "${BASE_URL}/pages/gallery" 2>/dev/null || true)"
ABOUT_ROUTER_COUNT="$(count_matches 'routerlink' "${ABOUT_NAV_BODY}")"
GALLERY_ROUTER_COUNT="$(count_matches 'routerlink' "${GALLERY_NAV_BODY}")"
if [[ "${ABOUT_ROUTER_COUNT}" -gt 0 && "${GALLERY_ROUTER_COUNT}" -gt 0 ]]; then
  pass "Public pages expose routerLink-based SPA navigation"
else
  fail "Public pages expose routerLink-based SPA navigation"     "about-us and gallery contain routerLink attributes (about=${ABOUT_ROUTER_COUNT}, gallery=${GALLERY_ROUTER_COUNT})"
fi

echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi
