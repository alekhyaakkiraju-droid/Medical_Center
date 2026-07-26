#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT="${ROOT}/openapi/swagger.json"
echo "Generating OpenAPI spec from backend test server..."
mkdir -p "${ROOT}/openapi"
OPENAPI_OUTPUT_PATH="${OUTPUT}" dotnet test "${ROOT}/backend/AngularApi.Tests/AngularApi.Tests.csproj" --filter "ExportOpenApiSpec_WhenOutputPathConfigured" --verbosity quiet
if [[ ! -f "${OUTPUT}" ]]; then echo "ERROR: OpenAPI spec file was not created at ${OUTPUT}" >&2; exit 1; fi
if [[ ! -s "${OUTPUT}" ]]; then echo "ERROR: OpenAPI spec file is empty at ${OUTPUT}" >&2; exit 1; fi
if ! python3 -m json.tool "${OUTPUT}" > /dev/null 2>&1; then echo "ERROR: OpenAPI spec at ${OUTPUT} is not valid JSON" >&2; exit 1; fi
SIZE="$(wc -c < "${OUTPUT}" | tr -d " ")"
echo "OpenAPI spec written to ${OUTPUT} (${SIZE} bytes)"
