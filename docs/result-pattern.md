# Result Pattern: step 02

Branch: `codex/step-02-result-pattern`, based on foundation commit `3a6d923`.

The contracts live in `Uber.Domain.Common` so domain rules and application
handlers can share them without referencing web, persistence or third-party
packages. Existing services, controllers and repository return types are
unchanged. Adoption happens one feature at a time in later branches.

## Contract

- `Error` is an immutable value with a required, nonblank `Code` and `Message`.
  Codes identify failures independently of message text; messages are preserved
  exactly as supplied. No HTTP status codes belong here.
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

During feature migration, preserve the original meaning of not-found, error
messages and external responses. Do not mechanically turn every null into an
error or catch every exception into a Result. Expected business failures use
these contracts; exception handling and HTTP mapping remain boundary concerns.

## Verification

`dotnet test tests/Uber.Domain.Tests/Uber.Domain.Tests.csproj --nologo -v minimal`
checks success/failure invariants, error identity and equality, invalid error
arguments, default and nullable values, and guarded access to failed values.

`dotnet build Uber.sln --no-restore --nologo -v quiet -m:1`
checks compatibility with the existing application. No database or external
services are required by the contract tests.
