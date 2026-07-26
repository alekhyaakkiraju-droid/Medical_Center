#!/usr/bin/env bash
# Post-deploy smoke tests for staging/production (WO-026).
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

echo "=== Smoke Test 3: Multi-page navigation ==="
ABOUT_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/about-us")"
SERVICE_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${BASE_URL}/pages/service")"
if [[ "${ABOUT_CODE}" == "200" && "${SERVICE_CODE}" == "200" ]]; then
  pass "About and service pages return HTTP 200"
else
  fail "About and service pages return HTTP 200" "about=${ABOUT_CODE}, service=${SERVICE_CODE}"
fi

echo "=== Smoke Test 4: Image-bearing page validation ==="
TEAM_BODY="$(curl -sf "${BASE_URL}/pages/team" 2>/dev/null || true)"
if [[ -n "${TEAM_BODY}" ]] && echo "${TEAM_BODY}" | grep -qi '<img'; then
  pass "Team page contains at least one image tag"
else
  GALLERY_BODY="$(curl -sf "${BASE_URL}/pages/gallery" 2>/dev/null || true)"
  if [[ -n "${GALLERY_BODY}" ]] && echo "${GALLERY_BODY}" | grep -qi '<img'; then
    pass "Gallery page contains at least one image tag"
  else
    fail "Team or Gallery page contains img tags" "HTTP 200 body containing <img"
  fi
fi

echo "=== Smoke Test 5: Unauthorized API rejection ==="
APPOINTMENTS_TMP="$(mktemp)"
APPOINTMENTS_CODE="$(curl -s -o "${APPOINTMENTS_TMP}" -w '%{http_code}' \
  -H "Accept: application/json" \
  "${API_URL}/Appointments")"
APPOINTMENTS_TYPE="$(curl -s -I -H "Accept: application/json" "${API_URL}/Appointments" | tr -d '\r' | awk 'tolower($0) ~ /^content-type:/ {print $0; exit}')"
if [[ "${APPOINTMENTS_CODE}" == "401" ]] \
  && echo "${APPOINTMENTS_TYPE}" | grep -qi 'application/json' \
  && grep -q '{' "${APPOINTMENTS_TMP}" \
  && ! grep -qi '<html' "${APPOINTMENTS_TMP}"; then
  pass "Unauthorized appointments request returns HTTP 401 JSON"
else
  fail "Unauthorized appointments request returns HTTP 401 JSON" \
    "HTTP 401 with application/json body (got status=${APPOINTMENTS_CODE})"
fi
rm -f "${APPOINTMENTS_TMP}"

echo "=== Smoke Test 6: Antiforgery token endpoint ==="
ANTIFORGERY_TMP="$(mktemp)"
ANTIFORGERY_CODE="$(curl -s -o "${ANTIFORGERY_TMP}" -w '%{http_code}' "${API_URL}/Account/antiforgery-token")"
if [[ "${ANTIFORGERY_CODE}" == "200" ]] && grep -q '"token"' "${ANTIFORGERY_TMP}"; then
  pass "Antiforgery token endpoint returns token field"
else
  fail "Antiforgery token endpoint returns token field" \
    "HTTP 200 JSON with token (got status=${ANTIFORGERY_CODE})"
fi
rm -f "${ANTIFORGERY_TMP}"

echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi

echo "All smoke tests passed."
