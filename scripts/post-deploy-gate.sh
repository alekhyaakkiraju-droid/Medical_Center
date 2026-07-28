#!/usr/bin/env bash
# Post-deploy gate orchestrator (WO-054).
# Runs all journey smoke suites and produces deployment-verification-report.json.
#
# Usage:
#   SMOKE_BASE_URL=http://localhost:8081 SMOKE_API_URL=http://localhost:8080 ./scripts/post-deploy-gate.sh
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

SMOKE_BASE_URL="${SMOKE_BASE_URL:-${FRONTEND_URL:-http://localhost:8081}}"
SMOKE_API_URL="${SMOKE_API_URL:-${API_URL:-http://localhost:8080}}"
DEPLOY_ENV="${DEPLOY_ENV:-local}"
GIT_SHA="${GIT_SHA:-$(git -C "${REPO_ROOT}" rev-parse HEAD 2>/dev/null || echo unknown)}"
REPORT_PATH="${REPORT_PATH:-${REPO_ROOT}/deployment-verification-report.json}"

TEST_SUITES=(
  "scripts/smoke-tests.sh"
  "scripts/journey-smoke-public.sh"
  "scripts/journey-smoke-auth.sh"
  "scripts/journey-smoke-assets.sh"
)

declare -a SUITE_NAMES=()
declare -a SUITE_STATUSES=()
declare -a SUITE_DURATIONS=()

overall_pass=true
timestamp="$(date -u +"%Y-%m-%dT%H:%M:%SZ")"

run_suite() {
  local suite_path="$1"
  local suite_name
  suite_name="$(basename "${suite_path}" .sh)"
  local start end duration exit_code

  if [[ ! -x "${REPO_ROOT}/${suite_path}" ]]; then
    echo "SKIP: ${suite_path} not found or not executable"
    SUITE_NAMES+=("${suite_name}")
    SUITE_STATUSES+=("fail")
    SUITE_DURATIONS+=("0")
    overall_pass=false
    return
  fi

  echo ""
  echo ">>> Running ${suite_path}"
  start=$(date +%s)
  set +e
  timeout 120 env SMOKE_BASE_URL="${SMOKE_BASE_URL}" SMOKE_API_URL="${SMOKE_API_URL}" \
    "${REPO_ROOT}/${suite_path}"
  exit_code=$?
  set -e
  end=$(date +%s)
  duration=$((end - start))

  SUITE_NAMES+=("${suite_name}")
  SUITE_DURATIONS+=("${duration}")
  if [[ "${exit_code}" -eq 0 ]]; then
    SUITE_STATUSES+=("pass")
    echo "<<< ${suite_name}: PASS (${duration}s)"
  else
    SUITE_STATUSES+=("fail")
    overall_pass=false
    echo "<<< ${suite_name}: FAIL (${duration}s, exit=${exit_code})"
  fi
}

echo "Post-deploy verification gate"
echo "  environment: ${DEPLOY_ENV}"
echo "  git_sha:     ${GIT_SHA}"
echo "  frontend:    ${SMOKE_BASE_URL}"
echo "  api:         ${SMOKE_API_URL}"

for suite in "${TEST_SUITES[@]}"; do
  run_suite "${suite}"
done

overall_result="pass"
if [[ "${overall_pass}" != true ]]; then
  overall_result="fail"
fi

{
  echo "{"
  echo "  \"timestamp\": \"${timestamp}\","
  echo "  \"environment\": \"${DEPLOY_ENV}\","
  echo "  \"git_sha\": \"${GIT_SHA}\","
  echo "  \"overall_result\": \"${overall_result}\","
  echo "  \"suites\": ["
  for i in "${!SUITE_NAMES[@]}"; do
    comma=","
    [[ "${i}" -eq $((${#SUITE_NAMES[@]} - 1)) ]] && comma=""
    echo "    {"
    echo "      \"name\": \"${SUITE_NAMES[$i]}\","
    echo "      \"status\": \"${SUITE_STATUSES[$i]}\","
    echo "      \"duration_seconds\": ${SUITE_DURATIONS[$i]}"
    echo "    }${comma}"
  done
  echo "  ]"
  echo "}"
} > "${REPORT_PATH}"

if command -v python3 >/dev/null 2>&1; then
  python3 -m json.tool "${REPORT_PATH}" >/dev/null || echo "WARNING: JSON validation failed for ${REPORT_PATH}"
fi

echo ""
echo "=== Post-Deploy Gate Summary ==="
printf "%-30s %-8s %s\n" "Suite" "Status" "Duration"
for i in "${!SUITE_NAMES[@]}"; do
  printf "%-30s %-8s %ss\n" "${SUITE_NAMES[$i]}" "${SUITE_STATUSES[$i]}" "${SUITE_DURATIONS[$i]}"
done
echo "Report: ${REPORT_PATH}"
echo "Overall: ${overall_result}"

if [[ "${overall_pass}" != true ]]; then
  exit 1
fi

exit 0
