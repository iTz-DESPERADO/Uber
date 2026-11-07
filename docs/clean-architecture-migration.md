# Incremental Clean Architecture migration

## Working agreement

- One reviewable phase per `codex/step-NN-description` branch.
- Stop after each phase for the owner's review. Start the next branch only after review, from the accepted previous phase (or the updated base after merge).
- Preserve business rules, validation messages, routes, response shapes, redirects, authorization, SignalR events and persistence behavior.
- Do not combine behavior fixes, package upgrades or database schema changes with structural migration.
- Implement Result Pattern, CQRS, Send dispatch and pipeline behaviors locally.
  Do not add MediatR or another mediator library.

## Step 01: foundation (committed as `3a6d923`)

Base commit: `08f910d`. Existing projects target .NET 8.

The running application still uses `Uber.Ui -> Uber.BLL -> Uber.DAL`.
This phase renames the presentation project from `Uber.PLL` to `Uber.Ui`
(folder, project, namespaces, Razor imports and scoped CSS reference), and adds
empty, buildable projects and solution entries:

```text
Uber.Application    -> Uber.Domain
Uber.Infrastructure -> Uber.Application -> Uber.Domain
```

No existing runtime project references these new projects yet. This is the
foundation for migration, not a completed Clean Architecture conversion.

Target responsibilities:

| Project | Responsibility | Allowed dependencies |
| --- | --- | --- |
| Uber.Domain | Business entities, value objects, rules and domain errors | No infrastructure or web frameworks |
| Uber.Application | Use cases, commands, queries, handlers and external-service/repository contracts | Domain |
| Uber.Infrastructure | EF Core, repository implementations, Identity, Stripe, mail and file storage | Application and Domain |
| Uber.Ui | MVC, Razor, HTTP mapping, SignalR transport and composition root | Application; Infrastructure for registration |

The presentation project is now `Uber.Ui`; its existing BLL reference stays in
place until feature migration. Remove BLL/DAL only after all callers migrate.

## Observed boundaries requiring care

- Entities live in DAL. `ApplicationUser` inherits `IdentityUser`, and `User`
  inherits `ApplicationUser`. Moving these classes verbatim into Domain would
  retain the Identity dependency. Separate persistence/identity models with
  explicit mapping in a dedicated phase, preserving keys and existing schema.
- Services currently return tuples, nullable entities and lists. Result adapters
  must preserve each method's current success, absence and failure semantics.
- `RideService` enforces status transitions and rating behavior. Characterize
  these before changing their implementation location.
- `PaymentService` uses Stripe SDK types directly. Introduce application-owned
  contracts and infrastructure adapters while preserving amounts and currency.
- EF migrations remain in DAL until persistence is migrated and schema/model
  equivalence is verified. Do not generate or apply migrations in this phase.

## Planned review phases

Production projects live under `src/`; test projects live under `tests/`.
`Uber.sln`, `README.md` and `.gitignore` stay at the repository root. The
solution mirrors these groups as solution folders. Run build and test commands
from the root; run the web app using `src/Uber.Ui/Uber.Ui.csproj`.

Step 02 added the [Result Pattern contracts](result-pattern.md) and isolated
domain tests (commit `4cc9eb5`). Step 03 added the [CQRS contracts, custom mediator
and pipeline behaviors](cqrs.md), committed as `879617e`. Step 04 establishes the
[ride transition behavior baseline](ride-behavior-baseline.md). Existing
application behavior remains on the legacy path until individual features migrate.
Step 04 was committed as `3f9bbe1`. Step 05 begins the
[domain boundary extraction](domain-boundaries.md) with enums and ride transition
rules. The wider Identity/entity split is deferred to feature-specific reviews
to avoid combining unrelated persistence and authentication changes.

| Phase / branch suffix | Deliverable |
| --- | --- |
| 02 `result-pattern` | Error, Result and Result<T> contracts with invariant tests; no blanket replacement of existing return values |
| 03 `cqrs-foundation` | Unified request/handler contracts, custom mediator with Send, ordered pipeline behaviors, cancellation support and DI/dispatch tests |
| 04 `behavior-baseline` | Characterization tests for the first feature, including current failure messages and observable side effects |
| 05 `domain-boundaries` | Extract enums and ride rules; define Identity/persistence boundaries and verify the generated schema is unchanged |
| 06 `first-feature` | Migrate one small end-to-end feature through Application and Infrastructure; preserve its MVC contract |
| 07+ `feature-name` | Separate reviewed branches for remaining user, driver, ride, admin, account and payment use cases; split large features further |
| final `remove-legacy-layers` | Remove unused BLL/DAL, verify dependency direction and run regression checks |

Choose the first vertical slice after inspecting its complete callers and side
effects. Commands and queries share the existing database; CQRS does not require
event sourcing or separate databases. Expected failures use Result; preserve
existing error handling at boundaries during migration rather than silently
changing what clients receive.

## Validation

Baseline command: `dotnet build Uber.sln --nologo -v minimal -m:1`.
Baseline result: success, 0 errors, 111 warnings on the initial build (including
existing nullable warnings and AutoMapper NU1903). Incremental builds can report
fewer warnings. Package remediation is separate from this migration.

For step 01, build the solution again and inspect the diff: the three new
project files, solution membership, presentation rename and documentation
should change. Verify that moved assets retain their contents and Razor views
compile with the new namespaces and scoped CSS bundle name.
No business rules change in this phase. This
build does not establish database, authentication or external-service health.
Later feature phases require characterization/regression tests and relevant
integration checks before review.
