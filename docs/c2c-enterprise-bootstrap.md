# Code-to-Cloud Enterprise — medical-center (Quick Setup)

## Confirmed configuration

| Setting | Value |
|---------|--------|
| Tenant | `opsera` |
| App | `medical-center` |
| Cloud | AWS `us-west-2` |
| Hub cluster | `argocd-usw2` |
| Spoke cluster | `opsera-usw2-np` |
| Namespace | `opsera-medical-center-dev` |
| Public URL | `https://medical-center-yarp-dev.agent.opsera.dev` |
| Branch | `main` |

## Mission Control

http://agent.opsera.ai/mission-control?id=dep_d8b6e6689690095dbd4e9565

## GitHub repository secrets (required before bootstrap)

| Secret | Purpose |
|--------|---------|
| `AWS_ACCESS_KEY_ID` | Opsera Sales account EKS/ECR access |
| `AWS_SECRET_ACCESS_KEY` | Paired AWS secret |
| `GH_PAT` | GitHub PAT with `repo` scope for ArgoCD repo registration |

## One-time bootstrap

```bash
gh workflow run 1-bootstrap-infrastructure-medical-center.yaml \
  -f TENANT=opsera \
  -f APP_NAME=medical-center \
  -f ENVIRONMENT=dev

gh run watch
```

Bootstrap creates:

- ECR repos: `medical-center-api`, `medical-center-yarp`, `medical-center-frontend`
- ArgoCD repo + spoke cluster registration
- Namespace `opsera-medical-center-dev`

## After bootstrap

1. Apply ArgoCD Application (hub cluster):

   ```bash
   kubectl --context hub apply -f .opsera-medical-center/argocd/medical-center-dev-application.yaml
   ```

2. Run Forge pipeline **Medical Center Dev EKS** or the CI workflow once added to build/push images and sync manifests.

3. Verify: `https://medical-center-yarp-dev.agent.opsera.dev/health`

## Notes

- Medical Center is a **3-service** app (api, yarp, frontend). Enterprise single-repo ECR pattern is adapted to three immutable repos.
- Existing flat manifests under `.opsera-medical-center/k8s/` are referenced by `k8s/overlays/dev/kustomization.yaml`.
