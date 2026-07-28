#!/usr/bin/env bash
# Scans public-facing Angular HTML templates for forbidden placeholder strings.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SCAN_DIR="${REPO_ROOT}/front-end/src/app"

FORBIDDEN_PATTERNS=(
  'Lorem ipsum'
  'PrimeCare'
  'Modamba'
  'Supportmedic'
  'Dormamu'
  'Mostafa Sharaby'
  'infotemplatepath'
  'infocleanxer'
  '1-800-700-6200'
  '(88017)'
  'Collins Street'
)

violations=0
matches=()

while IFS= read -r -d '' html_file; do
  for pattern in "${FORBIDDEN_PATTERNS[@]}"; do
    while IFS= read -r line; do
      [[ -z "${line}" ]] && continue
      matches+=("${line}")
      violations=$((violations + 1))
    done < <(grep -n -F "${pattern}" "${html_file}" 2>/dev/null || true)
  done
done < <(find "${SCAN_DIR}" -name '*.html' -print0)

if [[ ${#matches[@]} -gt 0 ]]; then
  echo "Placeholder content lint FAILED — ${violations} violation(s) found:"
  printf '%s\n' "${matches[@]}"
  echo ""
  echo "Remove forbidden placeholder strings from front-end/src/app/**/*.html"
  exit 1
fi

echo "Placeholder content lint PASSED — no forbidden strings in ${SCAN_DIR}/**/*.html"
exit 0
