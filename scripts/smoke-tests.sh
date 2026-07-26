#!/usr/bin/env bash
# Post-deploy smoke tests for staging (WO-024 pipeline / WO-029 / WO-004).
set -euo pipefail

BASE_URL="${SMOKE_BASE_URL:-http://localhost:8080}"
API_URL="${SMOKE_API_URL:-http://localhost:5000}"

echo "Running smoke tests against frontend=${BASE_URL} api=${API_URL}"

curl -sf "${API_URL}/health" >/dev/null
echo "PASS: API health check"

curl -sf "${BASE_URL}/" >/dev/null
echo "PASS: Frontend root responds"

echo "=== Smoke Test 3: Image-bearing page validation ==="
GALLERY_BODY="$(curl -sf "${BASE_URL}/pages/gallery")"
if echo "${GALLERY_BODY}" | grep -qi '<img'; then
  echo "PASS: Gallery page contains at least one image tag"
else
  echo "FAIL: Gallery page contains at least one image tag"
  exit 1
fi

echo "All smoke tests passed."
