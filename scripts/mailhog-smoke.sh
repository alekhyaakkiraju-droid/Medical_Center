#!/usr/bin/env bash
# MailHog connectivity smoke test (WO-028).
# Verifies SMTP port 1025 accepts connections and web UI on 8025 returns HTTP 200.
set -euo pipefail

MAILHOG_SMTP_HOST="${MAILHOG_SMTP_HOST:-localhost}"
MAILHOG_SMTP_PORT="${MAILHOG_SMTP_PORT:-1025}"
MAILHOG_WEB_URL="${MAILHOG_WEB_URL:-http://localhost:8025}"

PASS=0
FAIL=0

pass() { echo "PASS: $1"; PASS=$((PASS + 1)); }
fail() { echo "FAIL: $1 (expected: $2)"; FAIL=$((FAIL + 1)); }

echo "=== MailHog Smoke Test ==="
echo "SMTP: ${MAILHOG_SMTP_HOST}:${MAILHOG_SMTP_PORT}"
echo "Web UI: ${MAILHOG_WEB_URL}"

echo "=== Check 1: SMTP port ${MAILHOG_SMTP_PORT} accepting connections ==="
if command -v nc >/dev/null 2>&1; then
  if nc -z -w 3 "${MAILHOG_SMTP_HOST}" "${MAILHOG_SMTP_PORT}" 2>/dev/null; then
    pass "SMTP port ${MAILHOG_SMTP_PORT} reachable via nc"
  else
    fail "SMTP port ${MAILHOG_SMTP_PORT} reachable via nc" "nc -z succeeds"
  fi
elif bash -c "echo >/dev/tcp/${MAILHOG_SMTP_HOST}/${MAILHOG_SMTP_PORT}" 2>/dev/null; then
  pass "SMTP port ${MAILHOG_SMTP_PORT} reachable via /dev/tcp"
else
  fail "SMTP port ${MAILHOG_SMTP_PORT} reachable" "nc or bash /dev/tcp check succeeds"
fi

echo "=== Check 2: Web UI HTTP 200 ==="
HTTP_CODE="$(curl -s -o /dev/null -w '%{http_code}' "${MAILHOG_WEB_URL}" 2>/dev/null || echo "000")"
if [[ "${HTTP_CODE}" == "200" ]]; then
  pass "MailHog web UI returns HTTP 200"
else
  fail "MailHog web UI returns HTTP 200" "HTTP 200, got ${HTTP_CODE}"
fi

echo "=== Summary: ${PASS} passed, ${FAIL} failed ==="
if [[ "${FAIL}" -gt 0 ]]; then
  exit 1
fi
exit 0
