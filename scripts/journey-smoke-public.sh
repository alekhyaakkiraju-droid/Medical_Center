#!/usr/bin/env bash
# Journey-based public page smoke tests (WO-051).
# Validates public Angular routes return HTTP 200 with CareShift branding and no placeholder content.
#
# Usage:
#   SMOKE_BASE_URL=http://localhost:8081 ./scripts/journey-smoke-public.sh
set -euo pipefail

SMOKE_BASE_URL="${SMOKE_BASE_URL:-${FRONTEND_URL:-${BASE_URL:-http://localhost:8081}}}"
SMOKE_BASE_URL="${SMOKE_BASE_URL%/}"

PUBLIC_ROUTES=(
  "/"
  "/pages/about-us"
  "/pages/contact"
  "/pages/service"
  "/pages/blog"
  "/pages/gallery"
  "/pages/team"
)

BANNED_STRINGS=(
  "Lorem ipsum"
  "PrimeCare"
  "Modamba"
  "1-800-700-6200"
  "Supportmedic.com"
)

REQUIRED_STRINGS=(
  "CareShift"
)

PASS=0
FAIL=0
TOTAL=0

pass() { echo "PASS: $1"; PASS=$((PASS + 1)); }
fail() { echo "FAIL: $1 (expected: $2)"; FAIL=$((FAIL + 1)); }

check_route() {
  local route="$1"
  local url="${SMOKE_BASE_URL}${route}"
  local tmp
  tmp="$(mktemp)"

  local status body
  status="$(curl -s -L --connect-timeout 10 --max-time 30 -o "${tmp}" -w '%{http_code}' "${url}" || echo "000")"
  body="$(cat "${tmp}")"
  rm -f "${tmp}"

  TOTAL=$((TOTAL + 1))
  local route_ok=true

  printf '\n--- Route: %s (HTTP %s) ---\n' "${route}" "${status}"

  if [[ "${status}" != "200" ]]; then
    fail "${route} HTTP status" "200 (got ${status})"
    route_ok=false
  else
    pass "${route} returns HTTP 200"
  fi

  local banned
  for banned in "${BANNED_STRINGS[@]}"; do
    if echo "${body}" | grep -qi "${banned}"; then
      fail "${route} banned content" "no '${banned}' in response body"
      route_ok=false
    fi
  done

  if [[ "${route_ok}" == true ]]; then
    pass "${route} has no banned placeholder strings"
  fi

  local required
  for required in "${REQUIRED_STRINGS[@]}"; do
    if ! echo "${body}" | grep -q "${required}"; then
      fail "${route} branding" "response contains '${required}'"
      route_ok=false
    fi
  done

  if [[ "${route_ok}" == true ]]; then
    pass "${route} contains CareShift branding"
  fi

  if [[ "${route_ok}" == true ]]; then
    echo "RESULT: ${route} -> PASS"
  else
    echo "RESULT: ${route} -> FAIL"
  fi
}

echo "Running public page journey smoke tests against ${SMOKE_BASE_URL}"

for route in "${PUBLIC_ROUTES[@]}"; do
  check_route "${route}"
done

echo ""
echo "=== Summary: ${TOTAL} routes checked, ${PASS} checks passed, ${FAIL} checks failed ==="

if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi

echo "All public page journey smoke tests passed."
exit 0
