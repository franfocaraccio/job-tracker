# JobTracker

JobTracker helps people looking for a job keep track of the positions they apply to. Each user has a private account where they
register their applications (company, position and date applied) and move them through the hiring process — from
*Applied* to *Interviewing*, *Offer*, and finally *Accepted* or *Rejected* — so they always know where every process stands.

**Live demo:** https://job-tracker-focaraccio.vercel.app

> **Demo account**
>
> - **Email:** `demo@jobtracker.com`
> - **Password:** `Demo123!`
>
> It comes with four seeded applications in different stages. You can also create your own account.

- [Happy path](#happy-path)
- [User story](#user-story)
- [Stack](#stack)
- [Project structure](#project-structure)
- [Application lifecycle](#application-lifecycle)
- [Design decisions](#design-decisions)
- [API](#api)
- [Setup instructions](#setup-instructions)
- [Running the tests](#running-the-tests)
- [Generative AI exercise](#generative-ai-exercise)

## Happy path

1. Sign in with the demo account — the list shows the seeded applications with their status.
2. Click **New application**, enter a company and a position (the date defaults to today) and save.
3. Click the **pencil** of the new application — the status selector only offers the valid next stages
   (*Applied* → *Interviewing* or *Rejected*).
4. Move it to *Interviewing*, then *Offer*, then *Accepted*. Once accepted, no other status is offered.
5. Delete it with the **trash** icon (a confirmation is shown).
6. Log out — every page except login and register redirects to the login page.

## User story

> As someone looking for a job, I want to register my job applications and their status, so that I always know at what stage
> each hiring process is and I don't lose track of any of them.

**Acceptance criteria**

- I can create an account (name, email, password) and sign in.
- I only see my own applications; nobody else can see or change them.
- I can create, view, edit and delete applications (company, position, date applied).
- A new application starts as *Applied* and can only move forward through valid stages.
- I cannot save empty fields or a date in the future.

## Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API (controllers) |
| Data access | PostgreSQL 17 with plain ADO.NET through Npgsql |
| Authentication | JWT bearer tokens, passwords hashed with the ASP.NET Core Identity password hasher |
| Frontend | Angular 22, Bootstrap 5.3 CSS and Bootstrap Icons |
| Tests | xUnit, NSubstitute, Testcontainers, WebApplicationFactory (backend); Vitest (frontend) |
| Tooling | Docker Compose, OpenAPI + Swagger UI |
| Hosting | Vercel (frontend), Railway (API), Neon (PostgreSQL) |

## Project structure

```
JobTracker.sln
src/
  JobTracker.Domain                    Entities and business rules — no dependencies
  JobTracker.Application               Use cases and the interfaces they need (repositories, hasher, tokens)
  JobTracker.Infrastructure            PostgreSQL repositories (ADO.NET), password hashing, JWT
  JobTracker.Api                       ASP.NET Core Web API: controllers, contracts, Program.cs
tests/
  JobTracker.Domain.Tests              Unit tests of the business rules
  JobTracker.Application.Tests         Unit tests of the use cases (mocked repositories)
  JobTracker.Infrastructure.Tests      Unit tests of password hashing and JWT generation
  JobTracker.Infrastructure.IntegrationTests   Repositories against a real PostgreSQL (Testcontainers)
  JobTracker.Api.IntegrationTests      HTTP endpoints with the API hosted in memory
web/                                   Angular frontend (auth/ and applications/ features)
database/
  001_schema.sql                       Tables, constraints and index
  002_seed.sql                         Demo user and applications
docker-compose.yml                     PostgreSQL + API + web
```

## Application lifecycle

The core business rule is the hiring-process state machine. It lives in the Domain
(`ApplicationStatusExtensions.CanTransitionTo`) and is enforced by the API; the frontend only offers the valid options.

```
            ┌─────────┐     ┌──────────────┐     ┌───────┐     ┌──────────┐
 create ──► │ Applied │ ──► │ Interviewing │ ──► │ Offer │ ──► │ Accepted │
            └────┬────┘     └──────┬───────┘     └───┬───┘     └──────────┘
                 │                 │                 │         ┌──────────┐
                 └─────────────────┴─────────────────┴───────► │ Rejected │
                                                               └──────────┘
```

| From | Can move to |
|---|---|
| Applied | Interviewing, Rejected |
| Interviewing | Offer, Rejected |
| Offer | Accepted, Rejected |
| Accepted, Rejected | — (final) |

**Other rules**

- Company and position are required (max 100 characters) and trimmed; the date applied is required and cannot be in the future.
- Users: name required (max 100), valid email stored in lowercase (no duplicates by case), password of at least 6 characters.
- Entities have a private constructor: `Create` validates and builds a new one, `FromDatabase` rebuilds a persisted one.
  An entity can never exist in an invalid state.

## Design decisions

**Why this stack?**

- **.NET 10 and ASP.NET Core Web API.** .NET 10 is the current long-term support release. Controllers fit the
  MVC / Web API model the exercise asks for and keep each endpoint easy to read and test.
- **PostgreSQL.** The data is relational — a user owns many applications — so a relational database fits it naturally.
  It is open source, runs anywhere in Docker and has free managed hosting (Neon). Access is plain ADO.NET through Npgsql,
  PostgreSQL's official provider, as no ORM is allowed.
- **Angular 22.** A complete framework with routing, dependency injection, forms and an HTTP client built in, which gives
  the frontend a clear, consistent structure without extra libraries.
- **Bootstrap CSS.** A responsive, consistent UI with a few utility classes and no UI component library, keeping the
  frontend small.
- **Docker Compose.** One command starts the database, the API and the web app with the demo data.

**Clean Architecture.** Dependencies point inwards: `Api → Infrastructure → Application → Domain`.

- The **Domain** holds the rules and knows nothing about databases or HTTP.
- **Application** contains the use cases (`UserService`, `JobApplicationService`) and defines the interfaces it needs
  (`IUserRepository`, `IJobApplicationRepository`, `IPasswordHasher`, `ITokenService`).
- **Infrastructure** implements them (Npgsql, ASP.NET Core Identity hasher, JWT).
- The **Api** only translates HTTP to use cases and back. Use cases receive plain parameters, never API types, and responses
  are DTO records — entities and password hashes are never exposed.
- Each layer registers its own services (`AddApplication()`, `AddInfrastructure()`), so `Program.cs` stays small.

**Data access.** Repositories use hand-written, parameterized SQL and map each row to an entity with `FromDatabase`.
Every query on applications filters by the owner (`WHERE id = @id AND user_id = @userId`), and the schema backs the rules
with a foreign key, a `UNIQUE` email and a `CHECK` on the status.

**Security.** Passwords are hashed with PBKDF2 (ASP.NET Core Identity hasher). JWTs are signed with HMAC SHA-256, expire
after 60 minutes and carry only `sub`, `email` and `name`; the user id always comes from the token, never from the request.
Login returns the same 401 for an unknown email and a wrong password.

**Test-driven development (TDD).** Each feature started from its tests. The first one shows the cycle in two separate commits:
the tests for the status transitions were committed failing
([17a887b](https://github.com/franfocaraccio/job-tracker/commit/17a887b)) and then made to pass by the implementation
([63924a4](https://github.com/franfocaraccio/job-tracker/commit/63924a4)). The rest of the features (entities, use cases,
repositories and endpoints) followed the same red → green → refactor cycle, but each one was committed once green, with
its tests and implementation together.

**Testing strategy.** Unit tests for the domain, the use cases (NSubstitute) and the security helpers; integration tests for
the repositories against a disposable PostgreSQL started by Testcontainers (Microsoft recommends testing data access against
the real database engine) and for the HTTP endpoints with `WebApplicationFactory`.

## API

All endpoints return JSON; errors use `ProblemDetails`. Swagger UI is available at `/swagger` when the API runs in Development.

**Users API**

| Method | Route | Auth | Responses |
|---|---|---|---|
| POST | `/api/auth/register` | No | 201 user · 400 validation error or email already registered |
| POST | `/api/auth/login` | No | 200 `{ accessToken }` · 401 invalid credentials |
| GET | `/api/users/me` | Yes | 200 current user · 401 |

**Job applications API** (all endpoints require a token)

| Method | Route | Responses |
|---|---|---|
| GET | `/api/applications` | 200 the user's applications, newest first |
| GET | `/api/applications/{id}` | 200 · 404 (missing or owned by another user) |
| POST | `/api/applications` | 201 with `Location` header · 400 |
| PUT | `/api/applications/{id}` | 200 · 400 (validation or invalid status change) · 404 |
| DELETE | `/api/applications/{id}` | 204 · 404 |

## Setup instructions

### Option 1 — Docker (fastest)

Requires only [Docker](https://www.docker.com/products/docker-desktop/).

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Web app | http://localhost:4200 |
| API + Swagger UI | http://localhost:5000/swagger |
| PostgreSQL | `localhost:5432` (user `postgres`, password `jobtracker`, database `jobtracker`) |

The database is created with the schema and the demo data on first start. `docker compose down` followed by
`docker compose up` resets it to the seed.

### Option 2 — Run locally

Requires the [.NET SDK 10.0.114+](https://dotnet.microsoft.com/download), [Node.js 24.15+](https://nodejs.org) and Docker
(for PostgreSQL only).

```bash
docker compose up -d postgres
```

```bash
dotnet run --project src/JobTracker.Api --launch-profile http
```

```bash
cd web
npm install
npm start
```

Open http://localhost:4200. The API listens on http://localhost:5000 (Swagger UI at `/swagger`). The connection string,
JWT settings and allowed CORS origin are in `src/JobTracker.Api/appsettings.json`; they are development values and can be
overridden with environment variables (for example `Jwt__SigningKey`).

## Running the tests

**Backend** — 100 tests (Docker must be running: the integration tests start a PostgreSQL container):

```bash
dotnet test JobTracker.sln
```

**Frontend** — 9 tests for the route guard, the HTTP interceptor and the status options of the form:

```bash
cd web
npm test
```

## Generative AI exercise

The answer to the Generative AI exercise is in [GENAI.md](GENAI.md).
