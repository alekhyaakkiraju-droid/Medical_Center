# Data Classification Policy

This document maps Medical Center domain entities to data classification tiers. Tiers drive access controls, encryption requirements, retention, and audit expectations.

## Classification Tiers

| Tier | Label | Description | Examples |
|------|-------|-------------|----------|
| T1 | Public | Non-sensitive information safe for public disclosure | Specialization names, appointment status labels, medical center public profiles |
| T2 | Internal | Business-operational data not intended for public release | Doctor qualifications, hospital affiliations, service catalog, system audit metadata |
| T3 | Confidential | Personally identifiable information (PII) | Names, email, phone, address, user IDs |
| T4 | Restricted (PHI/ePHI) | Protected health information subject to HIPAA | Appointment clinical context, patient medical history fields, payment linked to care events, audit logs containing PHI |

## Entity Mapping

| Entity | Primary Fields | Tier | Rationale |
|--------|----------------|------|-----------|
| `AppUser` | Email, PhoneNumber, Address | T3 | Identity PII via ASP.NET Core Identity |
| `Patient` | Name, Address, Image, Email (inherited) | T4 | Patient profile tied to care relationships |
| `Doctor` | Name, ProfessionalStatement, Image | T3/T2 | Provider directory; statement is professional not clinical PHI |
| `Appointment` | Name, Email, Phone, ProbableStartTime, Amount, PatientId, DoctorId | T4 | Scheduling record linking patient to care event |
| `Payment` | Amount, PaymentMethod, PaymentStatus, PaymentDate | T4 | Financial data linked to healthcare appointments |
| `PatientReview` | Review content, ratings | T3/T4 | May contain patient opinion; treat as T4 when tied to care |
| `RefreshToken` | TokenHash, UserId, JwtId | T3 | Authentication artifact; never log raw tokens |
| `AuditLog` | Actor, Action, OldValues, NewValues | T3/T4 | Tier escalates to T4 when values contain PHI |
| `MedicalCenter` | Name, address, contact | T2 | Operational directory data |
| `MedicalCenterDoctorAvailability` | Schedule slots | T2 | Operational scheduling metadata |
| `Specialization` | Name, description | T1 | Public reference data |
| `DoctorSpecialization` | Links doctor to specialization | T2 | Professional association |
| `DoctorQualification` | Degree, institution | T2 | Professional credentials |
| `HospitalAffiliation` | Hospital name, role | T2 | Professional affiliation |
| `AppointmentStatus` | Status label | T1 | Reference data |
| `Service` | Service name, description | T1/T2 | Catalog data |

## Handling Requirements by Tier

| Tier | Encryption at Rest | Encryption in Transit | Access Control | Audit Logging |
|------|-------------------|----------------------|----------------|---------------|
| T1 | Recommended | TLS 1.2+ | Authenticated users | Optional |
| T2 | Required (DB) | TLS 1.2+ | Role-based (doctor/admin) | Recommended |
| T3 | Required (DB) | TLS 1.2+ | Owner or admin | Required |
| T4 | Required (DB + field-level where feasible) | TLS 1.2+ | Minimum necessary (owner, treating doctor, admin) | Required (`AuditMiddleware`, `AuditLog`) |

## Data Flow Notes

- JWT auth cookies and refresh tokens are **T3** authentication artifacts; store only hashed refresh tokens server-side.
- Swagger/OpenAPI documentation must not include live PHI; use synthetic data in examples.
- Logs via Serilog must redact T3/T4 fields; correlation IDs are T2.

## Review Cadence

Reclassify entities when new fields are added or integrations export data to third parties. Update this document and the RTM in the same pull request.
