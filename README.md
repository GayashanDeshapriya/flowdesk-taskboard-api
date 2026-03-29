# FlowDesk TaskBoard API

FlowDesk TaskBoard is a production-ready ASP.NET Core 8.0 service for managing engineering projects, tasks, and team collaboration. The solution uses a domain-driven, layered architecture, enforces granular authorization with JWT access tokens, and persists data in PostgreSQL via Entity Framework Core. A live instance runs on Render at `https://flowdesk-taskboard-api.onrender.com`, connected to a managed Neon PostgreSQL cluster.

## Architecture & Key Decisions
- **Layered solution** – API, Application, Domain, and Infrastructure projects isolate transport, orchestration, business logic, and persistence concerns for clean evolution.
- **JWT with policy-based authorization** – Authenticated clients receive short-lived tokens; controllers apply `Admin`, `TeamLead`, or `TeamMember` policies so each route enforces the appropriate scope.
- **PostgreSQL + EF Core** – PostgreSQL (local or Neon managed) stores authoritative data. EF Core migrations capture schema changes and keep environments aligned.
- **Deterministic development seed** – Opt-in reseeding (`SeedDatabase:ForceReseed`) can rebuild a known dataset of users, project members, and tasks to accelerate demos and QA.

## Live Environment
- **Base URL**: `https://flowdesk-taskboard-api.onrender.com`
- **Database**: Neon-managed PostgreSQL (connection strings supplied through environment-specific `appsettings*.json` or deployment secrets)
- **Authentication flow**: Client apps authenticate via `/api/auth/login`, store the returned JWT, and pass it in the `Authorization: Bearer <token>` header for all protected routes.

## Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- PostgreSQL 14+ (Neon, Docker, or local installation)
- (Optional) `dotnet-ef` CLI for managing migrations:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Local Setup
1. **Clone & restore packages**
   ```bash
   git clone https://github.com/GayashanDeshapriya/flowdesk-taskboard-api.git
   cd flowdesk-taskboard-api
   dotnet restore
   ```
2. **Configure settings**
   - Copy `FlowDesk.TaskBoard.Api/appsettings.Development.json` or configure [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets).
   - Provide a `ConnectionStrings:DefaultConnection` that targets your PostgreSQL instance.
   - Ensure `Jwt:Key` is at least 32 characters and adjust `Issuer/Audience` if necessary.
   - Toggle `SeedDatabase:ForceReseed` to `true` when you want to rebuild the demo dataset (development use only).
3. **Apply migrations**
   ```bash
   dotnet ef database update \
     --project FlowDesk.TaskBoard.Infrastructure \
     --startup-project FlowDesk.TaskBoard.Api
   ```
4. **Run the API**
   ```bash
   dotnet run --project FlowDesk.TaskBoard.Api
   ```
5. **Explore the API surface**
   - Navigate to https://localhost:5001/swagger (or the HTTPS URL printed in the console).
   - Authenticate using the login endpoint, then click `Authorize` to test secured routes.

## Seeded Accounts (Development)
When seeding is enabled, the following reference users are created.

| Role        | Email                              | Password         |
|-------------|------------------------------------|------------------|
| Admin       | `nimal.perera@flowdesk.io`         | `P@ssw0rd#2026!` |
| Team Lead   | `kasun.silva@flowdesk.io`          | `Secure#1234!`   |
| Team Lead   | `tharushi.kumari@flowdesk.io`      | `Tharushi#2026!` |
| Team Member | `amali.fernando@flowdesk.io`       | `Amali@2026!`    |
| Team Member | `sachin.jayawardena@flowdesk.io`   | `Sachin#Dev01`   |

## Layout
- `FlowDesk.TaskBoard.Api` – ASP.NET Core entry point, controllers, dependency registration, middleware.
- `FlowDesk.TaskBoard.Application` – DTOs, service abstractions, and business workflows.
- `FlowDesk.TaskBoard.Domain` – Entities and enums shared across tiers.
- `FlowDesk.TaskBoard.Infrastructure` – EF Core context, migrations, and service implementations (auth, projects, tasks).
