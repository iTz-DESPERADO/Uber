# Domain boundaries: step 05, first extraction

> Historical migration record. The current architecture is documented in
> [clean-architecture-migration.md](clean-architecture-migration.md); later steps
> completed the entity split and removed the legacy projects.

Branch: `codex/step-05-domain-boundaries`, based on behavior baseline `3f9bbe1`.

## Implemented in this review

- Move the six business enums from DAL into `Uber.Domain.Enums`, preserving
  names, numeric values and underlying integer types. Update C# and Razor callers.
- Extract arrival/start/completion preconditions into `RideTransitionRules`.
  Rules return the existing Domain Result type with stable error codes and the
  exact previous message text. They do not access repositories or mutate entities.
- Keep `RideService` as the compatibility adapter: load the existing ride, call
  the domain rule, preserve the existing tuple return type, update the repository
  and retain existing exception handling. Controllers and SignalR flow are unchanged.
- Add inward references from the legacy layers to Domain. Domain stays free of
  EF, Identity, ASP.NET Core and Stripe dependencies.

This is a bounded first extraction, not completion of the entity/Identity split.
Moving all existing entities at once would bring Identity, EF attributes and
`IFormFile` into Domain or require a much larger persistence change.

## Identity and persistence boundaries for later slices

| Current coupling | Migration boundary |
| --- | --- |
| `ApplicationUser : IdentityUser`, with User/Driver/Admin subtypes | Keep these EF/Identity models in the persistence layer until account mapping is implemented. Domain models must not inherit IdentityUser. |
| Ride navigations point to Identity-derived User and Driver | Future application contracts use string participant IDs and application-owned read models; repository adapters map persistence models. |
| Driver contains an `IFormFile` property | Upload input belongs to UI; storage is accessed through an application interface implemented in Infrastructure. |
| EF annotations and migration CLR names | Preserve the existing entity types/table mappings during staged extraction. Move migrations only with verified model and migration compatibility. |
| Stripe IDs and SDK interactions | Keep SDK types behind infrastructure adapters; preserve existing identifiers and payment behavior. |

The next ride slice can use the extracted status and rules with application-owned
repository contracts. It need not move the whole Identity model first. Account
and profile entity separation remains a dedicated later review, including identity
keys, discriminator values, relationship mapping and database compatibility tests.

## Schema and behavior evidence

`tests/Uber.Behavior.Tests/Baselines/schema-before-domain.sql` was generated from
the SQL Server EF model at `3f9bbe1` before production edits. The schema test compares
the full current create script to that baseline, normalizing only line endings.
This verifies unchanged generated tables, columns, keys, indexes and relationships,
including Identity tables. It does not connect to SQL Server, execute SQL, compare
a live database or assert compatibility of future data migrations.

Existing migration files and model snapshot are unchanged. No migration is generated
or applied. Enum tests additionally fix names and numeric values, because SQL schema
comparison alone cannot detect changed enum semantics. The 39 existing ride behavior
tests remain the external response and side-effect regression checks.

Run `dotnet test Uber.sln` and `dotnet build Uber.sln` from the repository root.
