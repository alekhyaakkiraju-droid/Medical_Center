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
   kubectl apply -f .opsera-medical-center/k8s/frontend-nginx-configmap.yaml
   kubectl apply -f .opsera-medical-center/k8s/*-service.yaml
   kubectl apply -f .opsera-medical-center/k8s/*-deployment.rendered.yaml
   kubectl apply -f .opsera-medical-center/k8s/ingress.yaml
   ```

Image placeholders (`PLACEHOLDER_*_ECR_URI`) are substituted by `render-k8s-manifests.sh`.

## Ingress routing

Public host `medical-center-yarp-dev.agent.opsera.dev` follows the same pattern as UBR NMS:

- `/` → `medical-center-frontend:80` (Angular SPA)
- `/api` → `medical-center-yarp:8080` (YARP → API)

The frontend pod mounts `frontend-nginx-configmap.yaml`, which proxies `/api/` to the in-cluster YARP service (`medical-center-yarp:8080`) instead of the Docker Compose hostname `yarp-proxy`.

YARP uses `yarp-configmap.yaml` so upstream API is `http://medical-center-api:8080` (K8s service DNS, not Docker Compose `api`).
