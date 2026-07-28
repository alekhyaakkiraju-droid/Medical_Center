# Medical Center — Forge dev deploy (Sales AWS account)

Deploy target: **`https://medical-center-yarp-dev.agent.opsera.dev`** (UAT/dev, not production).

## AWS account (via Forge — no local `aws configure` required)

| Setting | Value |
|---------|--------|
| Account | **792373136340** (SrinivasanAWS-Sales connector in Forge) |
| Region | **us-west-2** |
| Spoke cluster | **opsera-usw2-np** |
| ArgoCD hub | **argocd-usw2** |
| Namespace | **opsera-medical-center-dev** |

Forge stores AWS credentials in the **connector**. Pipelines use that connector at run time — your Mac's `opsera@pipeline` user (969898466769) is unrelated.

## Forge Shipping pipeline

- **Name:** Medical Center Dev EKS
- **ID:** `be5b3a9e-9348-4d1a-b184-8e18a175bb6b`
- **Connectors:** Alekhya-GH (GitHub) + SrinivasanAWS-Sales (AWS)

### Pre-flight checklist (run once before clicking Run)

| # | Check | Status |
|---|--------|--------|
| 1 | `front-end/package-lock.json` tracked (PR #98) | Done on `main` |
| 2 | `front-end/src/app/api/generated/api.ts` + `openapi/swagger.json` tracked (PR #99) | Done on `main` |
| 3 | ECR repos exist: `medical-center-api`, `-yarp`, `-frontend` | Run `bootstrap-ecr.sh` once |
| 4 | K8s secret `medical-center-app-secrets` + SQL/MailHog in namespace | Applied by deploy step from `dev-secrets.yaml` |
| 5 | Deploy step enabled on pipeline | `deploy-dev-eks` after push |
| 6 | Trigger **new** run from Forge UI — do **not** retry a failed run (Docker images are lost between retries) | Required |

### Pipeline phases

**Phase 1 — Build + push (target: green)**  
scan → sync/build api → build yarp → build frontend → bootstrap ECR → push all three images

**API Docker build context:** use `./scripts/build-api-docker.sh` (context is `backend/`, not `backend/AngularApi/` — the Dockerfile copies `AngularApi.Contracts/`).

**Phase 2 — Deploy (secrets + SQL/MailHog applied by pipeline)**  
render manifests with `$GIT_SHA_SHORT` → kubectl apply → rollout wait

### Trigger from Forge UI

1. Open project **CareShift (Copy) V2** in Forge.
2. Go to **Shipping → Pipelines → Medical Center Dev EKS**.
3. Run **Validate connectors** / **Preflight**.
4. Click **Run pipeline** on branch **`main`**.

### UAT logins

See [`test-data-manifest.md`](test-data-manifest.md). Password is documented there for non-production UAT only.

Reference implementation: `gayathri-opsera/UBR-Open-NMS` (`.opsera-ubr-nms/`).
