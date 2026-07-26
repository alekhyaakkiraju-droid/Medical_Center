# UAT Test Data Manifest

Reference credentials and entity relationships seeded by `DevelopmentDataSeeder` when the API runs in **Development** (e.g. local `docker compose up` or staging with `ASPNETCORE_ENVIRONMENT=Development`).

## Shared password

All seeded accounts use the same test password (not production-strength):

| Field | Value |
|-------|-------|
| Password | `UatSeed123!` |

## Users

| Role | Email | Display name |
|------|-------|--------------|
| admin | `admin@uat.careshift.local` | (admin account) |
| doctor | `dr.smith@uat.careshift.local` | Dr. Alice Smith |
| doctor | `dr.jones@uat.careshift.local` | Dr. Robert Jones |
| user (patient) | `patient.alice@uat.careshift.local` | Alice Nguyen |
| user (patient) | `patient.bob@uat.careshift.local` | Bob Martinez |

Login endpoint: `POST /api/Account/login` with `{ "email": "<email>", "password": "UatSeed123!" }`.

## Specializations

Three specializations are seeded when the table is empty:

- Orthopedics
- Cardiology
- Pediatrics

Each includes two related `Service` records.

## Medical center

| Field | Value |
|-------|-------|
| Id | `2` (matches `AppointmentSettings.DefaultCenterId`) |
| Address | 450 CareShift Medical Plaza, Springfield, IL 62701 |
| Time slot | 30 minutes |
| First consultation fee | $50.00 |
| Follow-up fee | $30.00 |

Default availability: Monday, Wednesday, Friday, 09:00–17:00.

## Doctor profiles

Both doctors are linked to medical center id `2`.

| Doctor | Specialization | Qualification | Hospital affiliation |
|--------|----------------|---------------|----------------------|
| Dr. Alice Smith | Cardiology | MD Cardiology (Johns Hopkins, 2008) | City Heart Institute, Boston, USA |
| Dr. Robert Jones | Orthopedics | MD Orthopedics (Stanford, 2006) | Regional Orthopedic Center, Chicago, USA |

## Appointments

Five appointments are seeded (once per environment) spanning **Active**, **Complete**, and **Canceled** statuses:

| # | Doctor | Patient | Status | Appointment date (relative) |
|---|--------|---------|--------|----------------------------|
| 1 | Dr. Alice Smith | Alice Nguyen | Active | Today |
| 2 | Dr. Alice Smith | Bob Martinez | Complete | Yesterday |
| 3 | Dr. Robert Jones | Alice Nguyen | Canceled | 2 days ago |
| 4 | Dr. Robert Jones | Bob Martinez | Active | 7 days ago |
| 5 | Dr. Alice Smith | Alice Nguyen | Complete | 14 days ago |

All appointments use `Amount = 30.00` (matches `AppointmentSettings.DefaultFee`) and `MedicalCenterId = 2`.

Doctor bookings: `GET /api/Doctors/{doctorId}/bookings` after logging in as the seeded doctor.

## Idempotency

Each entity type is seeded independently. Re-running the application does not duplicate users, doctor profiles, medical centers, availability slots, or appointments. Partial runs (e.g. interrupted startup) resume safely on the next run.

## Related work orders

- **WO-035** — MedicalCenter and doctor availability seeding (merged)
- **WO-036** — AppointmentStatus reference data (baseline statuses are also ensured by WO-034)
- **WO-037** — Extended manifest documentation
