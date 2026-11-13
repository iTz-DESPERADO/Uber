# Result Pattern

The contracts live in `Uber.Domain.Common` so domain rules and application
handlers can share them without referencing web, persistence or third-party
packages. Domain behavior and Application handlers use the contracts throughout
the migrated vertical slices.

## Contract

- `Error` is an immutable value with a required, nonblank `Code` and a non-null `Message`.
  Codes identify failures independently of message text; messages are preserved
  exactly as supplied, including empty or whitespace exception text. The code
  identifies the failure unambiguously. Repositories do not return Result;
  entities and application handlers use it for business outcomes.
  No HTTP status codes belong here.
- `Result.Success()` has `IsSuccess == true` and `Error == null`.
- `Result.Failure(error)` requires a non-null error and has `IsFailure == true`.
- `Result<T>.Success(value)` carries the supplied value, including zero, false
  or an empty collection. Use a nullable type when null is a valid success.
- `Result<T>.Failure(error)` has no readable value. Accessing `Value` throws
  `InvalidOperationException`; callers must check the outcome first.
- There are no implicit conversions, automatic exception catches or automatic
  null-to-failure conversions. Invalid factory arguments are programming errors.

```csharp
using Uber.Domain.Common;

Result<int> result = Result<int>.Success(42);
Result<int> missing = Result<int>.Failure(
    new Error("Ride.NotFound", "Ride not found"));

if (result.IsSuccess)
{
    int rideId = result.Value;
}
```

Repositories return entities, nullable absence, or collections and let
persistence failures throw. Domain methods and Application handlers return
Result for expected business outcomes. Minimal API endpoints translate failed
results into HTTP problem responses. The global exception handler owns technical
exception, concurrency, and unique-index mapping.

Do not mechanically turn every null into an error or catch every exception into
a Result. A missing repository row becomes a use-case error in its handler;
unexpected database, network, and programming failures remain exceptions.

Validation runs in the mediator pipeline and creates a failed response matching
the request's `Result` or `Result<T>` shape. Infrastructure does not reference
Result solely to describe persistence success.

## Verification

`dotnet test tests/Uber.Domain.Tests/Uber.Domain.Tests.csproj --nologo -v minimal`
checks success/failure invariants, error identity and equality, invalid error
arguments, default and nullable values, and guarded access to failed values.

`dotnet build Uber.sln --no-restore --nologo -v quiet -m:1` checks compatibility
with the application. Database-backed workflow verification additionally
requires applying the Infrastructure migrations.
