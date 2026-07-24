# Medical Center - Full Stack Web Application 

The  **Medical Center** full-stack web application is an innovative platform designed to streamline healthcare services. Built with .NET 8 for a powerful backend and Angular 18 for an intuitive and responsive frontend, this application ensures seamless experiences for patients, doctors, and administrators. With advanced features like appointment scheduling, medical record access, and role-based dashboards, this platform sets a new standard in healthcare management.

## Table of Contents
- [Technologies Used](#technologies-used)
- [Features](#features) 
- [Admin Dashboard](#Admin-Dashboard)
- [Doctor Role and Appointments](#Doctor-Role-and-Appointments)
- [Security](#security)
- [Getting Started](#getting-started)
- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Technologies Used
- **Backend**: ASP.NET Core (.NET 8)
- **Frontend**: Angular 18
- **Database**: SQL Server
- **Authentication & Authorization**: ASP.NET Core Identity, JWT, and OAuth (Google Login)
- **API Communication**: RESTful APIs
- **UI/UX Framework**: Angular Material for a modern and accessible interface

## Features

### User-Facing Features
- **Secure User Authentication**: Multi-layered authentication including:
  - **Traditional Login**: Email and password-based login.
  - **Google OAuth Integration**: One-click Google login for seamless access.
  - **Email Confirmation**: Mandatory email confirmation to verify and activate user accounts.
  - **Password Reset**: Secure password recovery with email notifications.
- **Appointment Scheduling**: Comprehensive appointment management system for patients and doctors.
- **Medical Records Access**: Patients can securely view and update their medical history.
- **Notifications System**: Automated reminders for upcoming appointments via email and in-app notifications.
- **Mobile-Responsive Design**: Optimized for desktops, tablets, and smartphones.

### Admin Dashboard
#### The Admin Dashboard is a powerful control center designed for administrators to efficiently manage the platform:
- **User Role Managemen**: Create, edit, and assign roles (e.g., Administrator, Doctor, Patient).
- **System Analytics**:  Gain insights into appointments, user activity, and system performance through visually appealing charts.
- **Appointment Oversight**:  View, edit, or cancel appointments for seamless administrative control.

### Doctor Role and Appointments
#### Doctors have access to a personalized dashboard that enhances their workflow:
- **Appointments Overview**: View upcoming and past appointments with detailed patient information.
- **Appointment Status Updates:**: Update appointment statuses (e.g., Completed, Cancelled, Pending).
- **Medical Records Access**: View patients’ medical histories for informed consultations.


### Security
- **Data Protection**: End-to-end encryption for sensitive data.
- **JWT Authentication**: Secure token-based authentication for API communication.
- **OAuth 2.0**: Google login integration with secure token exchange.
- **Email Verification**: Ensures only verified users can access the system.
- **Password Policies**: Enforced strong password rules and secure password storage using hashing algorithms.
- **Pre-commit Secret Scanning**: Gitleaks runs on every commit via pre-commit hooks to block credential leaks before they reach the repository.

## Getting Started

### Prerequisites
To run this project locally, ensure the following tools are installed:
- **Node.js** and **npm** for Angular development.
- **.NET SDK 8** for backend development.
- **SQL Server** for database management.
- **Git** for version control.

### Installation

#### Clone the Repository
```bash
git clone https://github.com/mostafasharaby/Medical-Center.git
cd Medical-Center
```

#### Install Pre-commit Secret Scanning

Install [pre-commit](https://pre-commit.com/) and enable the repository hooks before committing:

```bash
pip install pre-commit
pre-commit install
```

Hooks scan staged files with [Gitleaks](https://github.com/gitleaks/gitleaks) using `.gitleaks.toml`. To run manually:

```bash
pre-commit run --all-files
```

To verify the hook blocks secrets, create a temporary file containing a test pattern such as `"password": "realpassword123"` in a JSON object and attempt to commit it — the hook should fail.

Enable [GitHub secret scanning](https://docs.github.com/en/code-security/secret-scanning/about-secret-scanning) on the repository in GitHub Settings → Code security and analysis.

## Architecture Overview

Medical Center follows a **modular monolith** backend with a separate Angular frontend and YARP reverse proxy for containerized deployments.

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐     ┌────────────┐
│   Browser   │────▶│ YARP Proxy   │────▶│  AngularApi     │────▶│ SQL Server │
│  (Angular)  │     │  (port 8080) │     │  (.NET 8 REST)  │     │            │
└─────────────┘     └──────────────┘     └─────────────────┘     └────────────┘
                           │
                           └──▶ Angular static UI (port 8081)
```

| Component | Path | Role |
|-----------|------|------|
| API | `backend/AngularApi` | REST API, Identity, JWT cookies, EF Core |
| Frontend | `front-end` | Angular 18 SPA (NgModules) |
| Reverse proxy | `backend/YARPReverseProxy` | Routes `/api/*` to API, UI to frontend |
| Database | SQL Server | Persistent storage via EF Core migrations |

Key architectural decisions are documented in [docs/adr/](docs/adr/).

### Authentication

- HttpOnly JWT cookies for browser sessions (`MedCenter.Auth`)
- Bearer tokens for programmatic API access
- Google OAuth, refresh token rotation, antiforgery on mutating requests

See [ADR-002](docs/adr/002-jwt-cookie-migration.md) for details.

## Setup (Quick Reference)

See [CONTRIBUTING.md](CONTRIBUTING.md) for full contributor setup. Summary:

```bash
# Backend
cd backend/AngularApi && dotnet restore && dotnet ef database update && dotnet run

# Frontend
cd front-end && npm ci && npm start

# Full stack (Docker)
docker compose up --build
```

Environment variables for Docker Compose: `MSSQL_SA_PASSWORD`, `JWT_SECRET`, `JWT_VALID_ISSUER`, `JWT_VALID_AUDIENCE`, and optional Google/SMTP settings.

## Deployment

### Docker Compose (local / staging-like)

```bash
docker compose up --build -d
```

- YARP entry point: `http://localhost:8080`
- Frontend direct: `http://localhost:8081`
- Health checks: `/health` on API and YARP

### CI/CD (Forge Shipping)

The `.forge/pipeline.yaml` pipeline runs on push/PR to `main`:

1. Security scans (Gitleaks, Semgrep, npm audit)
2. .NET and Angular builds
3. Backend and frontend unit tests
4. Docker image build (API, YARP, frontend) + Grype scan
5. Staging smoke tests (`scripts/smoke-tests.sh`)

See [ADR-005](docs/adr/005-forge-shipping-cicd.md) and [CONTRIBUTING.md](CONTRIBUTING.md#pull-request-process).

## Compliance Documentation

- [Data classification](docs/compliance/data-classification.md) — entity tier mapping
- [HIPAA checklist](docs/compliance/hipaa-checklist.md) — control status
- [Data subject rights](docs/compliance/data-subject-rights.md) — 30-day SLA procedures

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.
