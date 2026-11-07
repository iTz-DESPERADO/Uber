# CQRS and mediator foundation: step 03

Branch: `codex/step-03-cqrs-foundation`, based on Result Pattern commit `4cc9eb5`.

## Unified contracts

Contracts live in `Uber.Application.Abstractions.Messaging`:

| Request | Unified request | Handler outcome |
| --- | --- | --- |
| `ICommand` | `IRequest<Result>` | `Task<Result>` |
| `ICommand<T>` | `IRequest<Result<T>>` | `Task<Result<T>>` |
| `IQuery<T>` | `IRequest<Result<T>>` | `Task<Result<T>>` |

Command/query handler interfaces specialize `IRequestHandler<TRequest, TResponse>`.
All handlers expose `HandleAsync(request, cancellationToken)`. Commands change
state; queries read data. Implementations must honor that distinction. Both
paths use the existing database.

Consumers inject `IMediator` and call `Send(request, cancellationToken)`; the
response type is inferred from the request. No MediatR package is used.

## Registration and use

```csharp
services.AddApplication(); // scoped IMediator + handlers from the Application assembly

// In a consumer with IMediator injected:
Result<int> result = await mediator.Send(command, cancellationToken);
```

`AddApplication` loops through concrete, closed classes in its own assembly and
registers their implemented `IRequestHandler<,>` contracts as scoped services.
Inherited command/query handler contracts are discovered too. Abstract classes,
open generic classes and unrelated types are ignored. No per-handler registration
is required. For separate feature assemblies or tests, pass additional assemblies:
`services.AddApplication(typeof(FeatureMarker).Assembly)`.

Repeated calls and repeated assemblies are idempotent. Two different handlers
for the same request/response contract, or a conflicting existing registration,
cause an explicit startup error before adding services. A matching existing
scoped registration is retained. Pipeline behaviors are registered explicitly
to preserve their intentional execution order; they are not auto-scanned.

## Pipeline behaviors

`IPipelineBehavior<TRequest, TResponse>` wraps the remaining pipeline through
`RequestHandlerDelegate<TResponse> next`. The behavior receives the request
and the caller's cancellation token. It may await `next()`, inspect the result,
handle an exception, or return early without invoking the handler.

```csharp
// Example behavior classes to implement when their features are needed:
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddScoped<IPipelineBehavior<CreateRideCommand, Result<int>>, CreateRideValidation>();
```

Both open generic and request-specific registrations are supported. The first
registered behavior is outermost:

```text
Logging before -> Validation before -> Handler
Logging after  <- Validation after  <- Handler result
```

Behaviors resolve from the current scope on each send. The handler resolves
only when the final continuation is invoked, so short-circuiting skips its
construction and execution. Behaviors normally call `next()` exactly once;
calling it multiple times re-executes the handler and is the behavior author's
responsibility. The continuation captures the caller's cancellation token.

The mediator rejects already-cancelled tokens before entering the pipeline
and checks cancellation again before invoking the handler. Handlers and
behaviors must forward the token to asynchronous dependencies. Cancellation
and unexpected exceptions propagate unchanged unless a behavior explicitly
handles them; they are not automatically converted into business errors.

## Implementation scope

A stateless generic adapter bridges the request's runtime type to the typed
handler/pipeline. Its construction uses reflection; actual handler calls use
typed interfaces so exceptions are not wrapped in reflection exceptions.
Scoped service instances are never cached. This implementation targets the
existing .NET MVC application; trimming/Native AOT compatibility is not claimed.

This phase adds dispatch and pipeline infrastructure only. It does not add
notifications, streaming, automatic transactions, retries, business validation
or logging policies. Those can be introduced as separately reviewed behaviors
when needed. Existing UI/BLL/DAL behavior and runtime wiring remain unchanged.

## Verification

`dotnet test tests/Uber.Application.Tests/Uber.Application.Tests.csproj` covers:

- All command/query shapes and preserved Result values/errors.
- Assembly discovery, excluded types, duplicate detection, DI scope isolation and repeat registration.
- Open and closed behaviors, nesting order and cancellation token forwarding.
- Short-circuiting without constructing the handler.
- Cancellation before and during the pipeline.
- Unwrapped handler/behavior exceptions and explicit missing-handler errors.

The example handlers in tests do not replace business characterization tests.
Run Domain tests for regression coverage and build `Uber.sln` before review.
