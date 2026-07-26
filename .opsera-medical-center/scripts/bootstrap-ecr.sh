#!/usr/bin/env bash
# One-time (idempotent) ECR repo bootstrap for Medical Center dev deploy.
# Runs in Forge Shipping with the Sales AWS connector (792373136340).
set -euo pipefail

REGION="${AWS_REGION:-us-west-2}"
REPOS=(medical-center-api medical-center-yarp medical-center-frontend)

for repo in "${REPOS[@]}"; do
  if aws ecr describe-repositories --repository-names "${repo}" --region "${REGION}" >/dev/null 2>&1; then
    echo "ECR repo exists: ${repo}"
  else
    echo "Creating ECR repo: ${repo}"
    aws ecr create-repository \
      --repository-name "${repo}" \
      --image-scanning-configuration scanOnPush=true \
      --region "${REGION}"
  fi
done

echo "ECR bootstrap complete in ${REGION}"
