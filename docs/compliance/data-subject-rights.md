# Data Subject Rights Procedures

Medical Center supports individual rights requests for patients and users whose personal data we process. This document defines intake, verification, fulfillment, and escalation with a **30-day SLA** from verified receipt.

## Scope

Applies to requests from data subjects (patients, doctors, registered users) regarding:

- **Access** — copy of personal data held
- **Rectification** — correction of inaccurate data
- **Erasure** — deletion where legally permitted
- **Restriction** — limit processing pending dispute resolution
- **Portability** — machine-readable export of provided data
- **Objection** — opt out of non-essential processing (e.g., marketing)

PHI requests follow HIPAA Right of Access timelines (30 days, one 30-day extension with notice).

## Request Intake

| Channel | Owner | Logging |
|---------|-------|---------|
| Email: privacy@medicalcenter.example | Privacy team | Ticket ID assigned within 1 business day |
| In-app form (planned) | Product | Pending UI implementation |
| Written mail | Privacy team | Scan and log upon receipt |

All requests are logged in the privacy request register with: requester identity, date received, request type, verification status, due date (received + 30 calendar days).

## Identity Verification

Before fulfilling any request:

1. Match requester email to registered `AppUser` / `Patient` account, **or**
2. Collect government-issued ID + date of birth for phone/mail requests, **or**
3. For authorized representatives, collect signed HIPAA authorization form.

Unverified requests are paused; SLA clock starts after successful verification.

## Fulfillment Procedures

### Access (30-day SLA)

1. Query `Patient`, `Appointment`, `Payment`, and related entities for the verified `UserId`.
2. Redact third-party PHI and other subjects' data from exports.
3. Deliver PDF + JSON export via secure channel (encrypted email link or portal — portal **Pending**).
4. Record fulfillment in audit log (`AuditLog` action: `DataSubjectAccess`).

### Rectification (30-day SLA)

1. Validate requested changes against source documents when clinical data is involved.
2. Apply updates via existing API services (`PatientService`, admin tools).
3. Propagate `UpdatedAt` / `UpdatedBy` audit fields.
4. Notify requester of completion.

### Erasure (30-day SLA)

1. Assess legal retention requirements (medical records may require retention despite erasure requests).
2. If erasure permitted: anonymize or delete T3/T4 fields; revoke refresh tokens; disable account.
3. Retain `AuditLog` entries as required by law (immutable audit trail).
4. Document denial rationale when retention applies.

### Portability (30-day SLA)

1. Export structured JSON of user-provided fields: profile, appointments (subject-owned), reviews.
2. Exclude derived/analytics data and secrets.

## SLA Tracking

| Milestone | Target |
|-----------|--------|
| Acknowledgment | 3 business days |
| Verification complete | 10 business days |
| Fulfillment | **30 calendar days** from verified receipt |
| Extension (if needed) | +30 days with written notice and reason |

Escalation: unresolved requests at day 25 escalate to Privacy Officer and Engineering lead.

## Breach Notification Procedures

> **Status: Pending (infrastructure not deployed)**

The following procedures are documented for operational readiness but automated breach detection and notification workflows are **not yet implemented**:

### Detection (Pending)

- Automated anomaly detection on `AuditLog` and authentication failures
- SIEM integration for correlated alerts
- Workforce reporting hotline

### Assessment (Pending)

- Privacy Officer triage within 24 hours of suspected breach
- Risk assessment: nature of PHI, individuals affected, mitigation steps

### Notification (Pending)

| Audience | Timeline | Method |
|----------|----------|--------|
| Internal leadership | 24 hours | Secure incident channel |
| Affected individuals | Without unreasonable delay, max 60 days | Written notice (email/letter) |
| HHS OCR | Within 60 days if ≥500 individuals | HHS breach portal |
| Media | If ≥500 individuals in one state | Press release |

### Documentation

- Maintain breach register with root cause, remediation, and notification dates
- Post-incident review within 14 days of closure

Until notification infrastructure is deployed, manual procedures above apply with Engineering + Privacy joint runbooks.

## Related Documents

- [Data Classification](data-classification.md)
- [HIPAA Checklist](hipaa-checklist.md)
- [ADR-002 JWT Cookie Migration](../adr/002-jwt-cookie-migration.md)
