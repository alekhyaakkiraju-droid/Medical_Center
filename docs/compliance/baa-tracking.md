# Business Associate Agreement (BAA) Tracking Register

This document is the single source of truth for Business Associate Agreement status across all vendors that create, receive, maintain, or transmit PHI on behalf of the Medical Center platform.

Last reviewed: 2026-07-26

## Purpose and Scope

HIPAA requires covered entities to obtain Business Associate Agreements (BAAs) with all business associates who handle PHI on their behalf (45 CFR §164.502(e)). This register tracks BAA status, PHI access scope, review cadence, and escalation paths for vendor relationships.

**In scope:** Vendors that process, store, or transmit ePHI for CareShift production and staging environments.

**Out of scope for this document:** Actual vendor contract terms, pricing, or legal language. BAA execution requires legal review and vendor negotiation; this register tracks metadata only.

**Related documents:** [`data-classification.md`](data-classification.md) (PHI tier definitions), [`hipaa-checklist.md`](hipaa-checklist.md) (overall compliance status).

## BAA Register

| Vendor Name | Service Type | PHI Access Description | BAA Status | Execution Date | Renewal Date | Responsible Party | Notes |
|-------------|--------------|------------------------|------------|----------------|--------------|-------------------|-------|
| SMTP Provider (configured via `SmtpSettings`) | Email Delivery | T3/T4 — patient names and email addresses in appointment confirmation, registration, and password reset emails (see [`data-classification.md`](data-classification.md)) | Pending | — | — | Privacy / Security Officer | BAA required before production email flows carry PHI |
| AWS (ECS Fargate, RDS SQL Server) | Cloud Infrastructure | T4 — full database including patient records, appointments, payments, and audit logs containing PHI (see [`data-classification.md`](data-classification.md)) | Pending | — | — | Privacy / Security Officer | AWS BAA available through AWS Artifact |
| Analytics Vendor (placeholder) | Application Monitoring | N/A — no analytics vendor currently integrated | Not Applicable | — | — | Engineering Lead | Re-evaluate when observability vendor is selected |

## PHI Access Matrix

Maps each vendor to data classification tiers defined in [`data-classification.md`](data-classification.md).

| Vendor | T1 Public | T2 Internal | T3 Confidential (PII) | T4 Restricted (PHI/ePHI) | Notes |
|--------|-----------|-------------|----------------------|--------------------------|-------|
| SMTP Provider | — | — | Email addresses, patient names in transactional messages | Appointment context in confirmation emails | MailKit SMTP; credentials from secrets management |
| AWS (ECS Fargate, RDS) | — | Operational metadata in logs | User IDs in infrastructure logs (must be redacted) | Full application database at rest and in transit within VPC | Encryption at rest depends on RDS configuration |
| Analytics Vendor | — | — | — | — | No vendor integrated; row retained for future onboarding |

## Review Cadence

| Trigger | Action | Owner |
|---------|--------|-------|
| **Annual review** (every 12 months from last reviewed date) | Verify all register entries, statuses, and renewal dates; update this document | Privacy / Security Officer |
| **Vendor contract renewal** | Confirm BAA remains executed and covers current PHI access patterns | Privacy / Legal |
| **New PHI access pattern** | Add or update vendor row before production deployment; cross-check [`data-classification.md`](data-classification.md) | Engineering Lead + Privacy |
| **New vendor integration** | Add vendor row in the same pull request that integrates the vendor | Engineering Lead |

## Escalation Process

1. **Vendor refuses to sign BAA** — Engineering stops production integration; Privacy Officer escalates to legal counsel; document refusal in the Notes column and mark status **Pending** with escalation date.
2. **BAA expires or is terminated** — Mark status **Expired**; suspend vendor PHI access within 24 hours; initiate renewal or vendor replacement.
3. **Undocumented PHI access discovered** — Add vendor to register immediately; treat as compliance incident; notify Privacy Officer within 1 business day.
4. **AWS Artifact BAA executed** — Update AWS row to **Executed** with execution date; set renewal date per AWS agreement terms.

## Next Actions

1. Execute AWS BAA via AWS Artifact (highest priority — blocks production hosting).
2. Identify production SMTP provider and initiate BAA negotiation.
3. Update this register when either BAA moves to **In Review** or **Executed**.
4. Re-evaluate analytics placeholder when observability tooling is selected for Phase 2+.
