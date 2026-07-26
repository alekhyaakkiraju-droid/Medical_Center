# UAT Test Data Manifest

Reference data seeded by `DevelopmentDataSeeder` in Development/staging environments.

## Medical Center

| Field | Value |
| --- | --- |
| Display name | CareShift Medical Center |
| ID | `2` (matches `AppointmentSettings.DefaultCenterId`) |
| Street address | 450 CareShift Medical Plaza |
| City | Springfield |
| State | IL |
| Zip | 62701 |
| Time slot per client (minutes) | 30 |
| First consultation fee | $50.00 |
| Follow-up consultation fee | $30.00 |

## Doctor Availability (MedicalCenter ID 2)

| Day | Start | End | Available |
| --- | --- | --- | --- |
| Monday | 09:00 | 17:00 | Yes |
| Wednesday | 09:00 | 17:00 | Yes |
| Friday | 09:00 | 17:00 | Yes |

Availability records are linked to the medical center. When UAT doctors are seeded with a `MedicalCenterId`, the seeder provisions the same Monday/Wednesday/Friday schedule for each distinct doctor medical center that does not already have availability rows.

## Specializations

Three demo specializations are seeded when the database is empty:

- Orthopedics
- Cardiology
- Pediatrics

Each specialization includes two related services.
