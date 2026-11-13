# Clean Architecture migration

## Status

The migration is complete in the current source tree. Production projects live
under `src/`, tests live under `tests/`, and the repository-level solution,
README, and Git ignore file remain at the root.

The former `Uber.BLL`, `Uber.DAL`, and `Uber.PLL` projects have been removed.
`Uber.Ui` is the presentation and composition project.

```text
Uber.Ui -> Uber.Application -> Uber.Domain
   |              ^
   +----> Uber.Infrastructure
                |        |
                +--------+----> Uber.Domain
```

Infrastructure is referenced by UI only to register implementations. Domain
and Application do not reference EF Core, ASP.NET Identity, Stripe, SignalR, or
the web project.

## Layer responsibilities

| Project | Owns | Does not own |
| --- | --- | --- |
| `Uber.Domain` | Entities, business behavior, errors, Result contracts, repository contracts, Unit of Work contract, domain services | HTTP, EF Core, Identity, Stripe, UI |
| `Uber.Application` | Commands, queries, handlers, validators, custom mediator, pipeline behaviors, use-case errors, external-service abstractions | Database models, MVC, SignalR transport |
| `Uber.Infrastructure` | EF/Identity models, DbContext, repositories, migrations, Dapper connection factory, Stripe, SMTP, location provider, development seed | HTTP endpoints and view behavior |
| `Uber.Ui` | Minimal APIs, page controllers, Razor views, permission policies, SignalR hub, global exception mapping, composition root | Business state transitions and persistence rules |

Domain entities and persistence models are deliberately separate. Infrastructure
maps tracked EF rows to Domain entities, applies successful domain changes back
to those rows, and commits through the scoped `IUnitOfWork`.

## Architectural decisions

### Result Pattern

Expected business failures use `Result` or `Result<T>`. Repository absence is
represented by `null`, persistence failures remain exceptions, and HTTP status
mapping stays in UI. See [result-pattern.md](result-pattern.md).

### CQRS without MediatR

Application provides its own `IMediator.Send`, request/handler contracts,
assembly-based handler discovery, and ordered pipeline behaviors. FluentValidation
runs in the validation behavior. Queries use Dapper projections; commands load
Domain entities through repositories and commit once through Unit of Work. See
[cqrs.md](cqrs.md).

### Identity and permissions

ASP.NET Identity models remain in Infrastructure. Domain users expose shared
behavior through the abstract `User` and `RatedUser` types, with customer,
driver, and admin specializations. UI permission requirements support claim-based
permissions with compatible role fallbacks for the current roles.

### Ride integrity

The `Ride` entity owns its lifecycle. Participant ownership is checked in
handlers. SQL `rowversion` columns protect rides and Identity users from lost
updates. SQL unique indexes guarantee:

- One active ride per driver for `Accepted`, `DriverWaiting`, or `InProgress`.
- One rating per `(RideId, RaterId)`.

The global exception handler converts concurrency and unique-index races into
HTTP `409 Conflict`. See [ride-transitions-cqrs.md](ride-transitions-cqrs.md).

## Migration history

| Step | Branch | Outcome |
| --- | --- | --- |
| 01 | `codex/step-01-clean-architecture-foundation` | Renamed presentation project to `Uber.Ui`; created the target projects and root layout |
| 02 | `codex/step-02-result-pattern` | Added Domain `Error`, `Result`, and `Result<T>` |
| 03 | `codex/step-03-cqrs-foundation` | Added custom mediator, unified request contracts, pipelines, and assembly scanning |
| 04 | `codex/step-04-behavior-baseline` | Captured legacy ride behavior before migration |
| 05 | `codex/step-05-domain-boundaries` | Began Domain enum and rule extraction |
| 06 | `codex/step-06-ride-transitions` | Moved ride transitions and repository boundaries into Clean Architecture |
| 07 | `codex/step-07-ride-integrity` | Completed vertical slices, removed legacy projects, added UI/API integrations and ride integrity safeguards |

## Build and database

Run from the repository root:

```sh
dotnet restore Uber.sln
dotnet build Uber.sln --no-restore
dotnet ef database update \
  --project src/Uber.Infrastructure/Uber.Infrastructure.csproj \
  --startup-project src/Uber.Ui/Uber.Ui.csproj
dotnet run --project src/Uber.Ui/Uber.Ui.csproj
```

Secrets belong in user secrets, environment variables, or the deployment secret
store. `src/Uber.Ui/appsettings.example.json` documents the required keys.

## Review rules for future changes

- Keep business transitions in Domain entities or Domain services.
- Keep expected failures as Result values; do not wrap every infrastructure
  exception in Result.
- Keep repository interfaces in Domain and query/external service abstractions
  in Application.
- Add migrations for schema changes and preserve database-level invariants.
- Keep Minimal API endpoints thin: authorize, send, translate, notify.
- Do not introduce MediatR; extend the local mediator and pipeline contracts.
