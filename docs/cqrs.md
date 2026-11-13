# CQRS and custom mediator

The application uses local CQRS contracts and a custom mediator. It does not
reference MediatR.

## Request contracts

Contracts live in `Uber.Application.Abstractions.Messaging`:

| Request | Response |
| --- | --- |
| `ICommand` | `Result` |
| `ICommand<T>` | `Result<T>` |
| `IQuery<T>` | `Result<T>` |

Handlers implement `IRequestHandler<TRequest, TResponse>` through the command or
query handler specializations. Consumers inject `IMediator` and call:

```csharp
Result<CreateRideResponse> result = await mediator.Send(command, cancellationToken);
```

## Handler discovery

`AddApplication()` scans the Application assembly and any explicitly supplied
assemblies for closed `IRequestHandler<,>` implementations. Each handler is
registered as scoped. Duplicate handlers and conflicting pre-registrations fail
at startup with an explicit exception; individual handlers are not registered
manually.

Domain services such as ride pricing and payment settlement are registered
separately. Infrastructure registers repository and external-service
implementations against Domain or Application contracts.

## Pipeline

Pipeline behaviors implement `IPipelineBehavior<TRequest, TResponse>`. They wrap
the next behavior or handler in registration order; the first registration is
the outermost behavior.

The current production pipeline includes `ValidationBehavior<,>`:

1. FluentValidation validators are discovered from the Application assembly.
2. Validators run asynchronously with the request cancellation token.
3. Failures are combined into an application validation error.
4. The behavior returns a failed `Result`/`Result<T>` without constructing or
   invoking the handler.
5. Valid requests continue to the handler.

Cancellation is checked by the mediator and forwarded through the pipeline,
handlers, repositories, Dapper, EF Core, and integrations. Unexpected exceptions
are not converted into business Result values; they reach the global exception
handler.

## Command side

Commands load Domain entities through repository contracts, invoke entity or
Domain service behavior, stage persistence changes, and call the scoped
`IUnitOfWork.SaveChangesAsync` once after successful business operations.
Repositories do not commit and do not return Result.

Examples include:

- Ride create, accept, reject, arrive, start, complete, and cancel.
- Persistent customer/driver ratings.
- Driver availability and profile changes.
- User, driver, and admin management.
- Wallet and Stripe payment operations.
- Local and external registration.

## Query side

Queries use an application-owned `IDbConnectionFactory`. Infrastructure opens
the SQL connection, while handlers execute Dapper projections. This keeps query
models independent from EF tracking and avoids exposing persistence entities to
UI.

Ride detail queries also project the persisted participant rating state, and the
active-ride query lets both participants recover an in-progress workflow after
navigation or reconnect.

## UI boundary

Minimal API route groups authorize the caller, construct a request, call
`IMediator.Send`, translate Result failures into HTTP problem responses, and
publish SignalR events after successful commits. Page controllers are used for
Razor navigation and view composition, not business transitions.

Permission requirements support explicit `permission` claims and compatible
role fallbacks. Participant ownership is still verified by handlers or the
query endpoint; role authorization alone is not treated as ride ownership.

## Concurrency and errors

CQRS pre-checks provide friendly errors, but database constraints remain the
authority for races:

- Ride and user `rowversion` columns detect concurrent updates.
- `UX_Rides_ActiveDriver` permits only one active ride per driver.
- `IX_RideRatings_RideId_RaterId` permits only one participant rating per ride.

The global exception handler maps these races to HTTP `409 Conflict`.

## Adding a use case

1. Add the command/query and response record in a feature folder.
2. Add a FluentValidation validator when request validation is required.
3. Implement one handler using Domain/Application abstractions.
4. Add Infrastructure implementation only when a new abstraction is needed.
5. Map a thin Minimal API endpoint and permission.
6. Add or update persistence constraints when the invariant must survive races.

Do not add per-handler DI registration or a second mediator library.
