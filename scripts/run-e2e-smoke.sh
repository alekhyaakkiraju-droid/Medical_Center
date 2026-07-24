#!/usr/bin/env bash
# End-to-end smoke tests for critical flows (WO-029).
# Run against Docker Compose: SMOKE_BASE_URL=http://localhost:8080 SMOKE_API_URL=http://localhost:5000 ./scripts/run-e2e-smoke.sh
set -euo pipefail

BASE_URL="${SMOKE_BASE_URL:-http://localhost:8080}"
API_URL="${SMOKE_API_URL:-http://localhost:5000}"
PASS=0
FAIL=0

pass() { echo "PASS: $1"; PASS=$((PASS + 1)); }
fail() { echo "FAIL: $1"; FAIL=$((FAIL + 1)); }

echo "=== Smoke Test 1: Patient registration & login pages ==="
if curl -sf "${BASE_URL}/login" | grep -qi "login"; then
  pass "Patient login page loads"
else
  fail "Patient login page loads"
fi

if curl -sf "${BASE_URL}/register" | grep -qi "register\|sign"; then
  pass "Patient registration page loads"
else
  fail "Patient registration page loads"
fi

echo "=== Smoke Test 2: Doctor dashboard route (frontend shell) ==="
if curl -sf "${BASE_URL}/" >/dev/null; then
  pass "Application shell responds"
else
  fail "Application shell responds"
fi

echo "=== Smoke Test 3: Admin API health & public specialization listing ==="
if curl -sf "${API_URL}/health" >/dev/null; then
  pass "API health endpoint"
else
  fail "API health endpoint"
fi

if curl -sf "${API_URL}/api/Specializations" >/dev/null 2>&1 || curl -sf "${API_URL}/api/Specializations/GetSpecializations" >/dev/null 2>&1; then
  pass "Public or authenticated specialization API reachable"
else
  # Allow anonymous doctors listing endpoint used by booking flow
  if curl -sf "${API_URL}/api/DoctorsWithSpectialization" >/dev/null 2>&1; then
    pass "Public doctors listing API reachable"
  else
    fail "Specialization or doctors listing API reachable"
  fi
fi

echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi
