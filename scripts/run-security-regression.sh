#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

FILTER='FullyQualifiedName~AuditLoggingIntegrationTests|FullyQualifiedName~AuthRateLimitingIntegrationTests|FullyQualifiedName~CsrfProtectionIntegrationTests|FullyQualifiedName~CookieAuthIntegrationTests|FullyQualifiedName~AuditServiceTests|FullyQualifiedName~AuditLogAppendOnlyTests'

echo "Running security controls regression tests..."
echo "Filter: ${FILTER}"
echo

if dotnet test backend/AngularApi.Tests/AngularApi.Tests.csproj \
  --filter "${FILTER}" \
  -c Release \
  --logger "console;verbosity=normal"; then
  echo
  echo "Security regression: PASSED"
  exit 0
else
  echo
  echo "Security regression: FAILED"
  exit 1
fi
