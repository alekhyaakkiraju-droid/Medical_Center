#!/usr/bin/env bash
# Verifies static image assets referenced on public pages return HTTP 200 (WO-022).
set -euo pipefail

BASE_URL="${1:-${SMOKE_BASE_URL:-${BASE_URL:-http://localhost:8081}}}"
BASE_URL="${BASE_URL%/}"

PAGE_PATHS=(
  "/"
  "/pages/about-us"
  "/pages/service"
  "/pages/contact"
)

# Critical assets referenced across public header/footer/pages (CSR-safe baseline).
KNOWN_ASSETS=(
  "images/loggggo-3.png"
  "images/blog/5.jpg"
  "images/blog/6.jpg"
  "images/background/3.jpg"
  "images/gallery/gallery-01.jpg"
  "images/services/service-one.jpg"
)

PASS=0
FAIL=0
declare -A SEEN_URLS=()
declare -A URL_SOURCE=()

pass() { echo "PASS: $1"; PASS=$((PASS + 1)); }
fail_msg() { echo "FAIL: $1 (expected: $2)"; FAIL=$((FAIL + 1)); }

register_asset() {
  local page="$1"
  local raw_url="$2"

  [[ -z "${raw_url}" ]] && return 0
  [[ "${raw_url}" == data:* ]] && return 0
  [[ "${raw_url}" == http://* || "${raw_url}" == https://* ]] && return 0

  local resolved="${raw_url}"
  if [[ "${resolved}" == /* ]]; then
    resolved="${BASE_URL}${resolved}"
  elif [[ "${resolved}" != "${BASE_URL}"* ]]; then
    resolved="${BASE_URL}/${resolved}"
  fi

  if [[ -z "${SEEN_URLS[${resolved}]+x}" ]]; then
    SEEN_URLS["${resolved}"]=1
    URL_SOURCE["${resolved}"]="${page}"
  fi
}

extract_assets_from_html() {
  local page="$1"
  local html="$2"

  while IFS= read -r src; do
    register_asset "${page}" "${src}"
  done < <(echo "${html}" | grep -oiE 'src="[^"]+"' | sed 's/src="//;s/"$//' || true)

  while IFS= read -r src; do
    register_asset "${page}" "${src}"
  done < <(echo "${html}" | grep -oiE "src='[^']+'" | sed "s/src='//;s/'$//" || true)

  while IFS= read -r bg; do
    register_asset "${page}" "${bg}"
  done < <(echo "${html}" | grep -oiE 'background-image:[^;]*url\([^)]+\)' | sed -E 's/.*url\((["'"'"']?)([^)"'"'"']+)\1\).*/\2/' || true)
}

echo "Checking static assets against ${BASE_URL}"

for page_path in "${PAGE_PATHS[@]}"; do
  html="$(curl -s -L "${BASE_URL}${page_path}" 2>/dev/null || true)"
  extract_assets_from_html "${page_path}" "${html}"
done

for asset_path in "${KNOWN_ASSETS[@]}"; do
  register_asset "known-assets" "${asset_path}"
done

if [[ ${#SEEN_URLS[@]} -eq 0 ]]; then
  fail_msg "asset discovery" "at least one image URL to verify"
  echo "=== Summary: 0 assets checked, ${PASS} passed, ${FAIL} failed ==="
  exit 1
fi

for asset_url in "${!SEEN_URLS[@]}"; do
  status_code="$(curl -s -o /dev/null -w '%{http_code}' -L -I "${asset_url}" 2>/dev/null || echo "000")"
  source_page="${URL_SOURCE[${asset_url}]:-unknown}"
  if [[ "${status_code}" == "200" ]]; then
    pass "Asset ${asset_url} (from ${source_page})"
  else
    fail_msg "Asset ${asset_url} from page ${source_page}" "HTTP 200 (got ${status_code})"
  fi
done

total_checked=${#SEEN_URLS[@]}
echo "=== Summary: ${total_checked} assets checked, ${PASS} passed, ${FAIL} failed ==="

if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi

echo "All static asset checks passed."
exit 0
