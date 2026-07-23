#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SPEC="${ROOT}/openapi/swagger.json"
OUT="${ROOT}/front-end/src/app/api/generated/api.ts"

"${ROOT}/scripts/generate-openapi.sh"

mkdir -p "$(dirname "${OUT}")"
cd "${ROOT}/front-end"
npx openapi-typescript "${SPEC}" -o "${OUT}"

echo "TypeScript API types written to ${OUT}"
