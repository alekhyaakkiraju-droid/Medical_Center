#!/usr/bin/env bash
# Post-deploy smoke tests for staging/production (WO-026, WO-038).
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

echo "Running smoke tests against frontend=${BASE_URL} api=${HEALTH_URL}"

echo "Waiting for API health (up to 120s)..."
elapsed=0
while [[ "${elapsed}" -lt 120 ]]; do
  if curl -sf "${HEALTH_URL}" >/dev/null 2>&1; then
    echo "API ready after ${elapsed}s"
    break
  fi
  sleep 5
  elapsed=$((elapsed + 5))
done
if [[ "${elapsed}" -ge 120 ]]; then
  fail "API startup wait" "health endpoint available within 120s"
fi

echo "=== Smoke Test 1: API health check ==="
if curl -sf "${HEALTH_URL}" >/dev/null 2>&1; then
  pass "API health check"
else
  fail "API health check" "HTTP 200 from /health"
fi

echo "=== Smoke Test 2: Frontend root with app-root ==="
ROOT_BODY="$(curl -sf "${BASE_URL}/" 2>/dev/null || true)"
if [[ -n "${ROOT_BODY}" ]] && echo "${ROOT_BODY}" | grep -qi 'app-root'; then
  pass "Frontend root responds with app-root"
else
  fail "Frontend root responds with app-root" "HTTP 200 body containing app-root"
fi

echo "=== Smoke Test 3: Multi-page SPA shell ==="
ABOUT_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/about-us")"
SERVICE_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/service")"
if [[ "${ABOUT_CODE}" == "200" && "${SERVICE_CODE}" == "200" ]]; then
  pass "About and service pages return HTTP 200"
else
  fail "About and service pages return HTTP 200" "about=${ABOUT_CODE}, service=${SERVICE_CODE}"
fi

echo "=== Smoke Test 4: Image-bearing Team page ==="
TEAM_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/team")"
TEAM_BODY="$(curl -sf "${BASE_URL}/pages/team" 2>/dev/null || true)"
TEAM_IMG_COUNT="$(count_matches '<img' "${TEAM_BODY}")"
if [[ "${TEAM_CODE}" == "200" && "${TEAM_IMG_COUNT}" -gt 0 ]]; then
  pass "Team page contains at least one image tag"
else
  fail "Team page contains at least one image tag" "HTTP 200 body containing <img (status=${TEAM_CODE}, imgs=${TEAM_IMG_COUNT})"
fi

echo "=== Smoke Test 5: Image-bearing Gallery page ==="
GALLERY_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/gallery")"
GALLERY_BODY="$(curl -sf "${BASE_URL}/pages/gallery" 2>/dev/null || true)"
GALLERY_IMG_COUNT="$(count_matches '<img' "${GALLERY_BODY}")"
if [[ "${GALLERY_CODE}" == "200" && "${GALLERY_IMG_COUNT}" -gt 0 ]]; then
  pass "Gallery page contains at least one image tag"
else
  fail "Gallery page contains at least one image tag" "HTTP 200 body containing <img (status=${GALLERY_CODE}, imgs=${GALLERY_IMG_COUNT})"
fi

echo "=== Smoke Test 6: Unauthorized API rejection ==="
APPOINTMENTS_TMP="$(mktemp)"
APPOINTMENTS_CODE="$(curl -s -o "${APPOINTMENTS_TMP}" -w '%{http_code}'   -H "Accept: application/json"   "${API_URL}/Appointments")"
APPOINTMENTS_TYPE="$(curl -s -I -H "Accept: application/json" "${API_URL}/Appointments" | tr -d '\r' | awk 'tolower($0) ~ /^content-type:/ {print $0; exit}')"
if [[ "${APPOINTMENTS_CODE}" == "401" ]]   && echo "${APPOINTMENTS_TYPE}" | grep -qi 'application/json'   && grep -q '{' "${APPOINTMENTS_TMP}"   && ! grep -qi '<html' "${APPOINTMENTS_TMP}"; then
  pass "Unauthorized appointments request returns HTTP 401 JSON"
else
  fail "Unauthorized appointments request returns HTTP 401 JSON"     "HTTP 401 with application/json body (got status=${APPOINTMENTS_CODE})"
fi
rm -f "${APPOINTMENTS_TMP}"

echo "=== Smoke Test 7: SPA navigation uses routerLink ==="
ABOUT_NAV_BODY="$(curl -sf "${BASE_URL}/pages/about-us" 2>/dev/null || true)"
TEAM_NAV_BODY="$(curl -sf "${BASE_URL}/pages/team" 2>/dev/null || true)"
ABOUT_ROUTER_COUNT="$(count_matches 'routerlink' "${ABOUT_NAV_BODY}")"
TEAM_ROUTER_COUNT="$(count_matches 'routerlink' "${TEAM_NAV_BODY}")"
if [[ "${ABOUT_ROUTER_COUNT}" -gt 0 && "${TEAM_ROUTER_COUNT}" -gt 0 ]]; then
  pass "Public pages expose routerLink-based SPA navigation"
else
  fail "Public pages expose routerLink-based SPA navigation"     "about-us and team contain routerLink attributes (about=${ABOUT_ROUTER_COUNT}, team=${TEAM_ROUTER_COUNT})"
fi

echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi

echo "All smoke tests passed."
