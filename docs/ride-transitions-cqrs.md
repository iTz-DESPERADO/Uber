# Ride workflow and integrity

## Flow

```text
Customer selects route
        |
        v
Find nearest available driver -> Create Pending ride -> SignalR request
                                                    |
                                      +-------------+-------------+
                                      |                           |
                                   Accept                       Reject
                                      |                           |
                                   Accepted              Assign next driver
                                      |
                               DriverWaiting
                                      |
                                 InProgress
                                      |
                                  Completed
                                      |
                          Settle payment and rate
```

The application uses a single SQL database. Commands and queries are separated
by code responsibility, not by database or event sourcing.

## Domain lifecycle

`Uber.Domain.Entities.Ride` owns status changes:

| Operation | Allowed source | Target |
| --- | --- | --- |
| Accept | `Pending` | `Accepted` |
| Reject | `Pending` | `Rejected` |
| Assign next driver | `Rejected` | `Pending` |
| Driver arrived | `Accepted` | `DriverWaiting` |
| Start | `DriverWaiting` | `InProgress` |
| Complete | `InProgress` | `Completed` |
| Cancel | `Pending`, `Accepted`, `DriverWaiting` | `Cancelled` |

Invalid transitions return a failed Result without mutating the entity.
Application handlers verify that the authenticated customer or driver belongs
to the ride before invoking Domain behavior.

## Creation and matching

The customer supplies selected coordinates plus route distance and duration.
`RidePricingService` calculates the price on the server; the client cannot send
an authoritative price. Driver matching uses Dapper and excludes drivers with
active or recent pending rides. Creation also performs friendly active-ride
checks for the customer and selected driver.

Those checks improve the response but are not the final concurrency authority.
The database constraint described below protects simultaneous accept requests.

## Atomic driver reservation

Migration `AddAtomicDriverReservation` creates the filtered unique index:

```sql
CREATE UNIQUE INDEX UX_Rides_ActiveDriver
ON Rides (DriverId)
WHERE DriverId IS NOT NULL AND Status IN (1, 4, 5);
```

The covered values are `Accepted`, `DriverWaiting`, and `InProgress`. If two
rides for the same driver are accepted concurrently, one database update wins
and the other violates the index. UI maps that race to HTTP `409 Conflict` with
a driver-reservation message.

Pending invitations are intentionally not covered: a driver may receive a new
pending assignment after rejection, but can hold only one accepted/ongoing ride.

## Optimistic concurrency

The persistence Ride and ApplicationUser models contain SQL `rowversion`
columns. EF Core includes the original token in updates, so stale ride, wallet,
availability, and aggregate-rating writes fail instead of silently overwriting
a newer change. The global exception handler maps `DbUpdateConcurrencyException`
to HTTP `409 Conflict`.

## Completion and payment

Payment is settled only when the driver completes an `InProgress` ride.
`RidePaymentService` verifies participant identity, completion state, ride
price, and wallet balance. Wallet payment debits the customer and credits the
driver in the same Unit of Work commit as completion. Stripe sandbox operations
remain behind the application payment abstraction.

## Persistent ratings

Ratings are allowed only after completion and only by an assigned participant:

- Customer rates the assigned driver.
- Driver rates the assigned customer.
- Rating value must be from 1 through 5.

Each rating is stored in `RideRatings`. The unique index on `(RideId, RaterId)`
prevents duplicates even under concurrent requests. The target user's aggregate
rating and the rating row are saved together. Ride detail queries return both
persisted rating states; the UI does not use local storage as the source of truth.

## Recovery and authorization

`GET /api/rides/active` returns the authenticated participant's active or recent
pending ride. Driver dashboard and customer home redirect back to that workflow,
and the waiting page reloads persisted status after load or SignalR reconnect.

`RideHub` requires authentication. Driver groups use the authenticated user ID,
and joining a ride group requires being the customer, assigned driver, or admin.
Ride detail queries enforce the same ownership rule.

## Persistence boundary

```text
Minimal API -> IMediator -> Application handler -> Domain entity/service
                                                -> Domain repository contract
                                                -> IUnitOfWork

Dapper query <- Application query handler <- IDbConnectionFactory
```

Repositories stage tracked changes and never call `SaveChanges`. Handlers commit
once after all Domain operations succeed. Expected failures use Result;
persistence and unexpected failures propagate to the global handler.

## Current follow-up work

- Recompute route distance and duration through a trusted server-side routing
  provider instead of trusting those two client measurements.
- Expire or reassign stale pending requests with an explicit background policy.
- Persist rejection history so a ride cannot cycle back to a driver who already
  rejected it.
