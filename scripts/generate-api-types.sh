#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SPEC="${ROOT}/openapi/swagger.json"
OUT="${ROOT}/front-end/src/app/api/generated/api.ts"
if [[ ! -s "${SPEC}" ]]; then echo "OpenAPI spec missing at ${SPEC}; running generate-openapi.sh..."; "${ROOT}/scripts/generate-openapi.sh"; else SPEC_SIZE="$(wc -c < "${SPEC}" | tr -d " ")"; echo "Using existing OpenAPI spec at ${SPEC} (${SPEC_SIZE} bytes)"; fi
mkdir -p "$(dirname "${OUT}")"
cd "${ROOT}/front-end"
npx openapi-typescript "${SPEC}" -o "${OUT}"
if [[ ! -f "${OUT}" ]]; then echo "ERROR: TypeScript API types file was not created at ${OUT}" >&2; exit 1; fi
if [[ ! -s "${OUT}" ]]; then echo "ERROR: TypeScript API types file is empty at ${OUT}" >&2; exit 1; fi
EXPORT_COUNT="$(grep -c "^export " "${OUT}" || true)"
if [[ "${EXPORT_COUNT}" -lt 1 ]]; then echo "ERROR: ${OUT} contains no export statements (found ${EXPORT_COUNT})" >&2; exit 1; fi
echo "TypeScript API types written to ${OUT} (${EXPORT_COUNT} export statements)"
