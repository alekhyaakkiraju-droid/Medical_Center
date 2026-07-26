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

A dev pipeline was created in Forge Shipping:

- **Name:** Medical Center Dev EKS
- **ID:** `be5b3a9e-9348-4d1a-b184-8e18a175bb6b`
- **Connectors:** Alekhya-GH (GitHub) + SrinivasanAWS-Sales (AWS)
- **Steps:** scan → build api/yarp/frontend → push ECR → deploy Kubernetes

### Trigger from Forge UI

1. Open project **CareShift (Copy) V2** in Forge.
2. Go to **Shipping → Pipelines → Medical Center Dev EKS**.
3. Run **Validate connectors** / **Preflight** if available.
4. Click **Run pipeline** (branch `main` after merging app fixes).

### UAT logins

See [`test-data-manifest.md`](test-data-manifest.md). Password is documented there for non-production UAT only.

## Next infra work (before first green deploy)

1. Merge local UX/session fixes to `main`.
2. Add Kubernetes manifests under `.opsera-medical-center/k8s/` (NMS-style kustomize per service).
3. Bootstrap ECR repos: `opsera/medical-center-api`, `opsera/medical-center-yarp`, `opsera/medical-center-frontend`.
4. SQL Server strategy for dev (in-cluster or RDS connection string in K8s secrets).

Reference implementation: `gayathri-opsera/UBR-Open-NMS` (`.opsera-ubr-nms/`).
