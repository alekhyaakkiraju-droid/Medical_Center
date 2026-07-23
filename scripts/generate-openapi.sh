#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT="${ROOT}/openapi/swagger.json"

mkdir -p "${ROOT}/openapi"

OPENAPI_OUTPUT_PATH="${OUTPUT}" dotnet test "${ROOT}/backend/AngularApi.Tests/AngularApi.Tests.csproj" \
  --filter "ExportOpenApiSpec_WhenOutputPathConfigured" \
  --verbosity quiet

echo "OpenAPI spec written to ${OUTPUT}"
