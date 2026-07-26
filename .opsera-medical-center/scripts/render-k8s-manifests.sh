#!/usr/bin/env bash
# Substitute image placeholders in k8s manifests before kubectl apply.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
K8S_DIR="${ROOT}/k8s"
REGISTRY="${ECR_REGISTRY:-792373136340.dkr.ecr.us-west-2.amazonaws.com}"
TAG="${IMAGE_TAG:?IMAGE_TAG is required}"

export API_IMAGE="${REGISTRY}/medical-center-api:${TAG}"
export YARP_IMAGE="${REGISTRY}/medical-center-yarp:${TAG}"
export FRONTEND_IMAGE="${REGISTRY}/medical-center-frontend:${TAG}"

for file in \
  "${K8S_DIR}/api-deployment.yaml" \
  "${K8S_DIR}/yarp-deployment.yaml" \
  "${K8S_DIR}/frontend-deployment.yaml"; do
  sed \
    -e "s|PLACEHOLDER_API_ECR_URI|${API_IMAGE}|g" \
    -e "s|PLACEHOLDER_YARP_ECR_URI|${YARP_IMAGE}|g" \
    -e "s|PLACEHOLDER_FRONTEND_ECR_URI|${FRONTEND_IMAGE}|g" \
    "${file}" > "${file}.rendered"
done

echo "Rendered manifests with tag ${TAG}"
