# Ride transition behavior baseline: step 04

Branch: `codex/step-04-behavior-baseline`, based on `879617e`.

The first slice is driver arrival, ride start and ride completion. Tests invoke
the existing `RideController` and real `RideService` together, using strict Moq
substitutes for repository and SignalR boundaries. Production code is unchanged.

| Action | Required status | New status | SignalR event |
| --- | --- | --- | --- |
| DriverArrived | Accepted | DriverWaiting | DriverArrived |
| StartRide | DriverWaiting | InProgress | RideStarted |
| CompleteRide | InProgress | Completed | RideCompleted |

## Preserved observations

- Each transition reads the ride once. Only its required status allows a write.
  The matrix covers all seven existing statuses plus an unknown numeric value.
- Wrong status returns HTTP 400 with the existing exact status-specific message;
  it performs no update or SignalR notification.
- Missing rides and repository lookup errors return `Ride not found`, including
  the case where the repository returns both an error and a ride.
- Update failures preserve their message. A null message selects the controller's
  fallback; an empty message remains empty.
- Exceptions from lookup or update become HTTP 400 containing the exception
  message, through the existing service catch blocks.
- Success returns HTTP 200 with exactly `success` and `message`. One notification
  follows the update, targeting `ride-{id}` with the ride ID as its only argument.
  The client-supplied `rideGroup` does not control this notification target.
- Notification failure returns HTTP 400 with `An error occurred: ...` after the
  update; the current path performs no rollback or retry. This records existing
  behavior rather than recommending it.
- Endpoint metadata remains POST, body-bound and authorized for the Driver role.

## Validation and limits

```sh
dotnet test tests/Uber.Behavior.Tests/Uber.Behavior.Tests.csproj
dotnet test Uber.sln
dotnet build Uber.sln
```

There are 39 test cases; several failure cases also exercise multiple repository
outcomes. No database, live SignalR connection or external service is used.
Authorization metadata is inspected; these are not HTTP middleware/authentication
integration tests. SQL persistence, serialization through MVC, concurrent requests,
and the remainder of the ride lifecycle are outside this baseline. The repository
implementation was inspected but its EF behavior is not exercised here.

Keep these external response and side-effect assertions when wiring the slice to
CQRS later. Extend coverage as each further feature is selected. Database model
and Identity mapping checks remain part of the domain/persistence migration.
