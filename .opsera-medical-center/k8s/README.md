# Medical Center dev Kubernetes manifests

Dev manifests for Forge Shipping deploy to `opsera-usw2-np`.

Target URL: `https://medical-center-yarp-dev.agent.opsera.dev`

Image placeholders (`PLACEHOLDER_*_ECR_URI`) are substituted at deploy time from ECR push outputs.

Before first successful deploy, provision namespace secrets (SQL connection string, JWT, SMTP, OAuth).
