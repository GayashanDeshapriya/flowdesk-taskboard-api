# FlowDesk TaskBoard API

A layered ASP.NET Core 8.0 service that secures task and project management workflows for engineering teams. The solution follows a clean separation between API, Application, Domain, and Infrastructure layers, applies role-based authorization on every controller, and issues short-lived JWT access tokens. PostgreSQL (via EF Core) backs persistence, while the development startup path can reseed deterministic demo data for faster testing.

## Key Decisions
- **JWT + policies** – JSON Web Tokens are issued during login; API endpoints enforce `Admin`, `TeamLead`, or `TeamMember` roles via named authorization policies so each feature has explicit access rules.
- **Layered architecture** – Domain entities are persistence-agnostic, Application layer exposes DTOs/services, and Infrastructure wires EF Core/Identity hashing. This keeps business logic testable and ready for future transports.
- **Deterministic seeding** – Development startup can truncate and reseed reference users, a sample project, and tasks (see `SeedDatabase:ForceReseed`) so QA environments stay reproducible without manual SQL scripts.

## Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download)
- PostgreSQL 14+ (local instance or managed provider such as Neon)
- (Optional) `dotnet-ef` CLI if you plan to add or re-create migrations: `dotnet tool install --global dotnet-ef`

## Run Locally
1. **Clone & restore**
   ```bash
   git clone https://github.com/GayashanDeshapriya/flowdesk-taskboard-api.git
   cd flowdesk-taskboard-api
   dotnet restore
   ```
2. **Configure secrets**
   - Copy `FlowDesk.TaskBoard.Api/appsettings.Development.json` (or use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)).
   - Set `ConnectionStrings:DefaultConnection` to your PostgreSQL database.
   - Provide a 32+ char `Jwt:Key`, and update `Issuer/Audience` if the defaults do not match your environment.
   - Optional: set `SeedDatabase:ForceReseed` to `true` only when you want to wipe and reseed the dev database on the next run.
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
5. **Explore the docs** – Browse https://localhost:5001/swagger (or the HTTPS port logged on startup) to exercise the endpoints. Use the `Authorize` button to paste a JWT issued by the login endpoint.

### Seeded Accounts (Development)
If seeding is enabled, the following users become available (passwords are defined in `SeedData.cs`).

| Role      | Email                           |
|-----------|---------------------------------|
| Admin     | `nimal.perera@flowdesk.io`      |
| Team Lead | `kasun.silva@flowdesk.io`       |
| Team Lead | `tharushi.kumari@flowdesk.io`   |
| Team Member | `amali.fernando@flowdesk.io` |
| Team Member | `sachin.jayawardena@flowdesk.io` |

## Known Limitations / Next Steps
- **No refresh tokens or logout endpoints** – access tokens expire after one hour; rolling refresh/blacklist support would improve security.
- **Minimal validation & error unification** – controllers handle common exceptions but lack a global problem-details formatter and FluentValidation rules.
- **Missing automated tests** – the solution currently has no unit or integration tests; adding tests around services and controllers would harden future refactors.
- **Single-project scope** – multi-project ownership and richer task board features (columns, attachments) are not implemented.

## Questions / Improvements
- Replace the hard-coded development connection strings with secrets or environment variables.
- Add CI (GitHub Actions) to run `dotnet test`, linting, and DB migrations automatically.
- Introduce pagination/filters across all list endpoints plus OpenAPI examples for easier client onboarding.
