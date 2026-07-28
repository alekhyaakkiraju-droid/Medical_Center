#!/usr/bin/env bash
# Static asset and image verification smoke tests (WO-053).
# Crawls public pages, extracts asset references, and verifies HTTP 200 responses.
#
# Usage:
#   SMOKE_BASE_URL=http://localhost:8081 ./scripts/journey-smoke-assets.sh
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

PASS=0
FAIL=0
WARN=0
declare -A SEEN_URLS=()

pass() { echo "PASS: $1"; PASS=$((PASS + 1)); }
fail() { echo "FAIL: $1 (expected: $2)"; FAIL=$((FAIL + 1)); }
warn() { echo "WARN: $1"; WARN=$((WARN + 1)); }

is_external_url() {
  local url="$1"
  [[ "${url}" == http://* || "${url}" == https://* ]] && [[ "${url}" != "${SMOKE_BASE_URL}"* ]]
}

is_data_uri() {
  local url="$1"
  [[ "${url}" == data:* ]]
}

normalize_url() {
  local raw="$1"
  if [[ -z "${raw}" ]]; then
    echo ""
    return
  fi
  if is_data_uri "${raw}" || is_external_url "${raw}"; then
    echo "${raw}"
    return
  fi
  if [[ "${raw}" == //* ]]; then
    local scheme="${SMOKE_BASE_URL%%://*}"
    echo "${scheme}:${raw}"
    return
  fi
  if [[ "${raw}" == /* ]]; then
    echo "${SMOKE_BASE_URL}${raw}"
    return
  fi
  echo "${SMOKE_BASE_URL}/${raw}"
}

register_asset() {
  local raw="$1"
  [[ -z "${raw}" ]] && return 0
  if is_data_uri "${raw}" || is_external_url "${raw}"; then
    return 0
  fi
  local resolved
  resolved="$(normalize_url "${raw}")"
  [[ -n "${resolved}" ]] && SEEN_URLS["${resolved}"]=1
}

extract_assets_from_html() {
  local html="$1"
  while IFS= read -r src; do
    register_asset "${src}"
  done < <(echo "${html}" | grep -oiE 'src="[^"]+"' | sed 's/src="//;s/"$//' || true)

  while IFS= read -r src; do
    register_asset "${src}"
  done < <(echo "${html}" | grep -oiE "src='[^']+'" | sed "s/src='//;s/'$//" || true)

  while IFS= read -r href; do
    if echo "${html}" | grep -qi "rel=.stylesheet.*${href}" || echo "${href}" | grep -qiE '\.css(\?|$)'; then
      register_asset "${href}"
    fi
  done < <(echo "${html}" | grep -oiE 'href="[^"]+"' | sed 's/href="//;s/"$//' || true)
}

echo "Running static asset verification against ${SMOKE_BASE_URL}"

for route in "${PUBLIC_ROUTES[@]}"; do
  html="$(curl -s -L --connect-timeout 10 --max-time 30 "${SMOKE_BASE_URL}${route}" || true)"
  extract_assets_from_html "${html}"

  while IFS= read -r src; do
    [[ -z "${src}" ]] && continue
    if echo "${src}" | grep -qi 'localhost'; then
      warn "Hardcoded localhost in img src '${src}' on ${route}"
    fi
  done < <(echo "${html}" | grep -oiE 'src="[^"]+"' | sed 's/src="//;s/"$//' || true)
done

if [[ ${#SEEN_URLS[@]} -eq 0 ]]; then
  fail "asset discovery" "at least one local asset URL to verify"
  exit 1
fi

for asset_url in "${!SEEN_URLS[@]}"; do
  status="$(curl -s -o /dev/null -w '%{http_code}' -L --connect-timeout 5 --max-time 10 "${asset_url}" 2>/dev/null || echo "000")"
  if [[ "${status}" == "200" ]]; then
    pass "Asset ${asset_url}"
  else
    fail "Asset ${asset_url}" "HTTP 200 (got ${status})"
  fi
done

total=${#SEEN_URLS[@]}
echo ""
echo "=== Summary: ${total} assets checked, ${PASS} passed, ${FAIL} failed, ${WARN} warnings ==="

if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi

echo "All static asset verification checks passed."
exit 0
