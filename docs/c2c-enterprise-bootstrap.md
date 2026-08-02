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

1. ~~Apply ArgoCD Application~~ — done via `2-apply-argocd-medical-center-dev.yaml`

2. **Run CI/CD (Code-to-Cloud — no Forge):**

   ```bash
   gh workflow run 2-ci-build-scan-push-medical-center-dev.yaml
   gh run watch
   ```

   This builds all 3 images, pushes to ECR, updates K8s manifests in git, refreshes ECR pull secret, syncs ArgoCD, and verifies the public URL.

3. Verify: `https://medical-center-yarp-dev.agent.opsera.dev/health`

## Notes

- Medical Center is a **3-service** app (api, yarp, frontend). Enterprise single-repo ECR pattern is adapted to three immutable repos.
- Existing flat manifests under `.opsera-medical-center/k8s/` are referenced by `k8s/overlays/dev/kustomization.yaml`.
