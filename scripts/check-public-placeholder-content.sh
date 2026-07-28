#!/usr/bin/env bash
# WO-017: Fail CI when forbidden placeholder strings appear in enforced public templates.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
FRONTEND="${ROOT}/front-end"

FORBIDDEN=(
  'Lorem ipsum'
  'PrimeCare'
  '1-800-700-6200'
)

# Public-facing template roots (exclude admin/, doctor/, and auth/).
SEARCH_DIRS=(
  "${FRONTEND}/src/app/pages/general"
  "${FRONTEND}/src/app/layout"
)

# Templates pending migration in follow-up stories (WO-018, etc.).
EXCLUSIONS_FILE="${ROOT}/scripts/placeholder-check-exclusions.txt"

collect_templates() {
  find "${SEARCH_DIRS[@]}" -name '*.html' -type f | sort
}

is_excluded() {
  local file="$1"
  local rel="${file#"${ROOT}/"}"
  [[ -f "${EXCLUSIONS_FILE}" ]] && grep -qxF "${rel}" "${EXCLUSIONS_FILE}"
}

violations=0

while IFS= read -r file; do
  if is_excluded "${file}"; then
    continue
  fi

  for pattern in "${FORBIDDEN[@]}"; do
    if grep -qF "${pattern}" "${file}"; then
      echo "FORBIDDEN: '${pattern}' in ${file#"${ROOT}/"}"
      violations=$((violations + 1))
    fi
  done
done < <(collect_templates)

if [[ "${violations}" -gt 0 ]]; then
  echo "Placeholder content check failed with ${violations} violation(s)."
  exit 1
fi

echo "Placeholder content check passed."
