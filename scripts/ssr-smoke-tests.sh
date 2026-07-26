#!/usr/bin/env bash
# SSR integration smoke tests for Express 5 upgrade (WO-058) and route hardening (WO-059).
set -euo pipefail

SSR_PORT="${SSR_PORT:-4000}"
SSR_BASE_URL="${SSR_BASE_URL:-http://localhost:${SSR_PORT}}"
SSR_STARTUP_TIMEOUT="${SSR_STARTUP_TIMEOUT:-120}"

PASS=0
FAIL=0
SSR_PID=""

pass() { echo "PASS: $1"; PASS=$((PASS + 1)); }
fail() { echo "FAIL: $1 (expected: $2)"; FAIL=$((FAIL + 1)); }

cleanup() {
  if [[ -n "${SSR_PID}" ]] && kill -0 "${SSR_PID}" 2>/dev/null; then
    kill "${SSR_PID}" 2>/dev/null || true
    wait "${SSR_PID}" 2>/dev/null || true
  fi
}
trap cleanup EXIT

echo "Running SSR smoke tests against ${SSR_BASE_URL}"

cd front-end

if [[ ! -d dist/medical-center/server ]]; then
  echo "Building Angular SSR bundle..."
  npm run build
fi

echo "Starting SSR server on port ${SSR_PORT}..."
PORT="${SSR_PORT}" npm run serve:ssr:MedicalCenter &
SSR_PID=$!

elapsed=0
while [[ "${elapsed}" -lt "${SSR_STARTUP_TIMEOUT}" ]]; do
  if curl -sf "${SSR_BASE_URL}/" >/dev/null 2>&1; then
    echo "SSR server ready after ${elapsed}s"
    break
  fi
  sleep 2
  elapsed=$((elapsed + 2))
done

if [[ "${elapsed}" -ge "${SSR_STARTUP_TIMEOUT}" ]]; then
  fail "SSR server startup" "listening on port ${SSR_PORT} within ${SSR_STARTUP_TIMEOUT}s"
  echo "SSR smoke summary: ${PASS} passed, ${FAIL} failed"
  exit 1
fi

echo "=== SSR Smoke Test 1: Root page renders app-root ==="
ROOT_BODY="$(curl -sf "${SSR_BASE_URL}/" 2>/dev/null || true)"
if [[ -n "${ROOT_BODY}" ]] && echo "${ROOT_BODY}" | grep -qi 'app-root'; then
  pass "SSR root page contains app-root"
else
  fail "SSR root page contains app-root" "HTTP 200 body containing app-root"
fi

echo "=== SSR Smoke Test 2: Public Angular routes render app-root ==="
for route in /pages/about-us /pages/gallery /pages/team; do
  ROUTE_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${SSR_BASE_URL}${route}" 2>/dev/null || true)"
  ROUTE_BODY="$(curl -sf "${SSR_BASE_URL}${route}" 2>/dev/null || true)"
  if [[ "${ROUTE_CODE}" == "200" ]] && echo "${ROUTE_BODY}" | grep -qi 'app-root'; then
    pass "SSR route ${route} returns app-root"
  else
    fail "SSR route ${route} returns app-root" "HTTP 200 body containing app-root (status=${ROUTE_CODE})"
  fi
done

echo "=== SSR Smoke Test 3: Unknown API path returns 404 JSON ==="
API_TMP="$(mktemp)"
API_CODE="$(curl -s -o "${API_TMP}" -w '%{http_code}' "${SSR_BASE_URL}/api/nonexistent" 2>/dev/null || true)"
API_TYPE="$(curl -s -I "${SSR_BASE_URL}/api/nonexistent" 2>/dev/null | tr -d '' | awk 'tolower($0) ~ /^content-type:/ {print $0; exit}')"
if [[ "${API_CODE}" == "404" ]] && echo "${API_TYPE}" | grep -qi 'application/json' && grep -q 'API endpoint not found' "${API_TMP}" && ! grep -qi '<html' "${API_TMP}"; then
  pass "Unknown /api path returns 404 JSON without SSR HTML"
else
  fail "Unknown /api path returns 404 JSON without SSR HTML" "HTTP 404 JSON body (status=${API_CODE})"
fi
rm -f "${API_TMP}"

echo "=== SSR Smoke Test 4: Static asset returns cache headers ==="
STATIC_ASSET="$(find dist/medical-center/browser -type f \( -name '*.js' -o -name '*.css' \) | head -n 1 || true)"
if [[ -z "${STATIC_ASSET}" ]]; then
  fail "Static asset discovery" "at least one JS or CSS file in browser dist"
else
  STATIC_PATH="${STATIC_ASSET#dist/medical-center/browser}"
  STATIC_HEADERS="$(curl -s -I "${SSR_BASE_URL}${STATIC_PATH}" 2>/dev/null || true)"
  STATIC_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${SSR_BASE_URL}${STATIC_PATH}" 2>/dev/null || true)"
  if [[ "${STATIC_CODE}" == "200" ]] && echo "${STATIC_HEADERS}" | grep -qi 'cache-control'; then
    pass "Static asset returns 200 with Cache-Control header"
  else
    fail "Static asset returns 200 with Cache-Control header" "status=${STATIC_CODE}, headers contain cache-control"
  fi
fi

echo "SSR smoke summary: ${PASS} passed, ${FAIL} failed"
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi
