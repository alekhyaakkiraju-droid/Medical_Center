# Medical Center dev Kubernetes manifests

Dev manifests for Forge Shipping deploy to `opsera-usw2-np` / `opsera-medical-center-dev`.

Target URL: `https://medical-center-yarp-dev.agent.opsera.dev`

## Before first deploy

1. **ECR repos** (one-time, account `792373136340`):
   ```bash
   bash .opsera-medical-center/scripts/bootstrap-ecr.sh
   ```
2. **Dev secrets + backing services** — applied automatically by the Forge `deploy-dev-eks` step from:
   - `dev-secrets.yaml` (JWT, SQL connection, SMTP placeholders)
   - `sqlserver-dev.yaml` (in-cluster SQL Server for UAT)
   - `mailhog-dev.yaml` (captured outbound email)
3. **Render image tags** at deploy time:
   ```bash
   IMAGE_TAG=<git-sha-short> bash .opsera-medical-center/scripts/render-k8s-manifests.sh
   kubectl apply -f .opsera-medical-center/k8s/namespace.yaml
   kubectl apply -f .opsera-medical-center/k8s/yarp-configmap.yaml
   kubectl apply -f .opsera-medical-center/k8s/*-service.yaml
   kubectl apply -f .opsera-medical-center/k8s/*-deployment.rendered.yaml
   kubectl apply -f .opsera-medical-center/k8s/ingress.yaml
   ```

Image placeholders (`PLACEHOLDER_*_ECR_URI`) are substituted by `render-k8s-manifests.sh`.

YARP uses `yarp-configmap.yaml` so upstream API is `http://medical-center-api:8080` (K8s service DNS, not Docker Compose `api`).
