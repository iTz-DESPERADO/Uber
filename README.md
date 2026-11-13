# 🚗 Uber System

A complete ride-hailing application built with ASP.NET Core MVC, featuring real-time communication between riders and drivers using SignalR.

---

## 🎥 Demo Video


https://github.com/user-attachments/assets/20323e2c-d635-4c9c-ae95-b6c9e4139aad



## 🌟 Features

* **Real-time Ride Requests**: Instant notifications to drivers using SignalR
* **Smart Driver Matching**: Automatic selection of nearest available driver based on coordinates
* **Live Status Updates**: Real-time ride status tracking for both riders and drivers
* **Automatic Fallback**: Seamless transition to next available driver if current driver rejects
* **Active Ride Recovery**: Riders and drivers return to their current ride after navigation or reconnect
* **Atomic Driver Reservation**: SQL Server guarantees that one driver cannot hold two active rides
* **Persistent Ratings**: One database-backed rating per participant and ride
* **Wallet and Stripe Sandbox**: Payment endpoints with wallet settlement after ride completion
* **Comprehensive Error Handling**: User-friendly error messages and graceful degradation

---

## 🔄 How It Works

### Ride Request Flow

```
User Requests Ride
       ↓
Find Nearest Available Driver
       ↓
Create Pending Ride in Database
       ↓
Send Real-time Notification to Driver (SignalR)
       ↓
Driver Receives Request on Dashboard
       ↓
    ┌──────┴──────┐
    ↓             ↓
  Accept       Reject
    ↓             ↓
Update Status   Find Next Nearest Driver
    ↓             ↓
Notify User   Repeat Process or Notify "No Drivers"
```

### Key Workflows

**1. Ride Request**

* User selects pickup and destination suggestions from the location search endpoint
* Server calculates the quoted price from the supplied route distance and duration
* Nearest driver is selected and notified via SignalR
* Ride status set to `Pending`

**2. Driver Response**

* Driver sees incoming request in real-time
* Can accept or reject within the dashboard
* Response triggers immediate status update

**3. Acceptance Path**

* Ride status updated to `Accepted`
* A filtered unique SQL index atomically reserves the driver for this active ride
* User redirected to trip tracking page
* Driver information shared with user

**4. Rejection Path**

* System automatically finds next nearest driver
* New notification sent via SignalR
* Process repeats until driver accepts or no drivers remain

---

## 🏗️ Architecture

The legacy BLL/DAL projects have been removed and the application now follows
Clean Architecture. Domain entities and repository
contracts are independent of EF Core and Identity. Application owns the CQRS
use cases, custom mediator, validation pipeline and integration abstractions.
Infrastructure owns EF models, migrations, repository implementations and external integrations.
See the [migration plan](docs/clean-architecture-migration.md) and
[CQRS guide](docs/cqrs.md). Ride-specific integrity rules are documented in
[the ride workflow guide](docs/ride-transitions-cqrs.md).

```
┌─────────────────────────────────────────┐
│  Presentation Layer (Uber.Ui)          │
│  - Minimal APIs, MVC views, SignalR     │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│  Application                            │
│  - CQRS handlers and pipeline behaviors │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│  Domain                                 │
│  - Entities, rules, repository contracts│
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│  Infrastructure                        │
│  - EF models, repositories, DbContext   │
└─────────────────────────────────────────┘
```

### Layer Responsibilities

**Uber.Ui (Presentation Layer)**

* MVC Controllers for HTTP request handling
* Minimal API groups for application commands and queries
* SignalR Hubs for real-time communication
* Razor views for user interface
* Client-side JavaScript for interactive features

**Uber.Application**

* CQRS commands and queries handled by the custom mediator
* FluentValidation pipeline for incoming requests
* Application abstractions for Identity, payments and data connections
* Result Pattern at application boundaries

**Uber.Domain / Uber.Infrastructure**

* Domain entities own business state and behavior
* Repository contracts and Unit of Work live in Domain
* EF/Identity models, implementations, DbContext and migrations live in Infrastructure
* Repositories stage changes; the Unit of Work commits them
* Dapper handles query-side projections through an application-owned connection factory
* SQL row versions protect ride, wallet and rating updates from lost writes

---

## Configuration

Keep secrets outside source control and provide them through user secrets,
environment variables, or the deployment secret store:

* `ConnectionStrings__Mydatabase`
* `Stripe__SecretKey`
* `Smtp__Host`, `Smtp__Port`, `Smtp__UserName`, `Smtp__Password`, `Smtp__Sender`
* `Authentication__Google__ClientId`, `Authentication__Google__ClientSecret` (optional; callback: `/signin-google`)
* `Authentication__Facebook__AppId`, `Authentication__Facebook__AppSecret` (optional; callback: `/signin-facebook`)

---

## 📁 Project Structure

```
Uber/
├── src/
│   ├── Uber.Ui/                 # MVC, Razor, SignalR and startup
│   ├── Uber.Domain/             # Domain contracts and Result Pattern
│   ├── Uber.Application/        # CQRS, mediator and pipelines
│   └── Uber.Infrastructure/     # EF, Identity, repositories and integrations
├── tests/
│   ├── Uber.Domain.Tests/
│   ├── Uber.Application.Tests/
│   └── Uber.Behavior.Tests/      # Existing ride transition behavior
├── docs/
├── assets/
├── .gitignore
├── README.md
└── Uber.sln
```

---

## 🚀 Getting Started

### Prerequisites

* .NET SDK supporting .NET 8 and the .NET 8 ASP.NET Core runtime
* SQL Server, SQL Server Express, or LocalDB
* Visual Studio 2022 or VS Code
* Modern web browser with JavaScript enabled

Run from the repository root:

```sh
dotnet restore Uber.sln
dotnet build Uber.sln --no-restore
dotnet ef database update --project src/Uber.Infrastructure/Uber.Infrastructure.csproj --startup-project src/Uber.Ui/Uber.Ui.csproj
dotnet test Uber.sln --no-build
dotnet run --project src/Uber.Ui/Uber.Ui.csproj
```

Application startup requires local database and external-service settings.
Copy `src/Uber.Ui/appsettings.example.json` to `appsettings.json` and replace
the placeholders locally. The local file is ignored by Git. The focused unit
and contract tests do not require a running database or external services.

### Development accounts

When the application runs in `Development`, startup creates these accounts once:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@uber.local` | `Admin123!` |
| Rider | `rider1@uber.local` | `Rider123!` |
| Rider | `rider2@uber.local` | `Rider123!` |
| Driver | `driver1@uber.local` | `Driver123!` |
| Driver | `driver2@uber.local` | `Driver123!` |

The driver accounts include active Cairo locations and sample vehicles. Rider
accounts include development wallet balances. The
seed is idempotent and is never executed outside the Development environment.

## Data integrity

Ride lifecycle changes are enforced by the Domain entity and protected by a
SQL `rowversion`. A filtered unique index on `Rides.DriverId` covers the active
statuses `Accepted`, `DriverWaiting`, and `InProgress`; concurrent accept
requests therefore cannot reserve the same driver twice. Ratings are stored in
`RideRatings` with a unique `(RideId, RaterId)` index. Expected business
failures return `Result`, while the global exception handler maps concurrency
and unique-index races to HTTP `409 Conflict`.

---

## 🙏 Acknowledgments

* Built with ASP.NET Core MVC
* Real-time communication powered by SignalR
* Inspired by modern ride-hailing platforms

---

⭐ **Star this repository if you find it helpful!**


