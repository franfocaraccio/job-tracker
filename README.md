# JobTracker

JobTracker helps job seekers keep track of the positions they apply to. Each user has a private
account where they register their applications (company, position and date applied) and move them
through the hiring process — from *Applied* to *Interviewing*, *Offer*, and finally *Accepted* or
*Rejected* — so they always know where every process stands.

## Tech stack

- **Backend:** .NET 10, ASP.NET Core Web API (controllers), Clean Architecture
- **Data:** PostgreSQL accessed with raw ADO.NET (Npgsql) — no Entity Framework, Dapper or MediatR
- **Frontend:** Angular
- **Tests:** xUnit

## Solution structure

```
src/
  JobTracker.Domain               Entities and business rules (no dependencies)
  JobTracker.Application          Use cases and abstractions (depends on Domain)
  JobTracker.Infrastructure       PostgreSQL repositories, hashing, JWT (depends on Application)
  JobTracker.Identity.Api         User API: register, login, current user — issues JWTs
  JobTracker.Applications.Api     Job applications CRUD API — validates JWTs
tests/
  JobTracker.*.Tests              One test project per production project
```