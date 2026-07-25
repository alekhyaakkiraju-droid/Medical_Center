#!/usr/bin/env bash
# Post-deploy smoke tests for staging (WO-024 pipeline / WO-029).
set -euo pipefail

BASE_URL="${SMOKE_BASE_URL:-http://localhost:8080}"
API_URL="${SMOKE_API_URL:-http://localhost:5000}"

echo "Running smoke tests against frontend=${BASE_URL} api=${API_URL}"

curl -sf "${API_URL}/health" >/dev/null
echo "PASS: API health check"

curl -sf "${BASE_URL}/" >/dev/null
echo "PASS: Frontend root responds"

echo "All smoke tests passed."
