#!/usr/bin/env bash
# Builds the API image. Context must be backend/ (not backend/AngularApi/) because
# backend/AngularApi/Dockerfile COPY paths include AngularApi.Contracts/.
set -euo pipefail
TAG="${1:-medical-center-api:ci}"
docker build -f backend/AngularApi/Dockerfile -t "${TAG}" backend
