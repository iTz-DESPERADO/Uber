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

* User submits pickup and destination coordinates
* System calculates distances to all active drivers
* Nearest driver is selected and notified via SignalR
* Ride status set to `Pending`

**2. Driver Response**

* Driver sees incoming request in real-time
* Can accept or reject within the dashboard
* Response triggers immediate status update

**3. Acceptance Path**

* Ride status updated to `Accepted`
* User redirected to trip tracking page
* Driver information shared with user

**4. Rejection Path**

* System automatically finds next nearest driver
* New notification sent via SignalR
* Process repeats until driver accepts or no drivers remain

---

## 🏗️ Architecture

The running application currently uses three layers. Migration to Clean
Architecture is in progress: Domain and Application contain Result Pattern
and custom CQRS/mediator foundations; features still run through BLL/DAL.
See the [migration plan](docs/clean-architecture-migration.md) and
[CQRS guide](docs/cqrs.md).

```
┌─────────────────────────────────────────┐
│  Presentation Layer (Uber.Ui)          │
│  - Controllers, Views, SignalR Hubs     │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│  Business Logic Layer (Uber.BLL)        │
│  - Services, Business Rules             │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│  Data Access Layer (Uber.DAL)           │
│  - Repositories, Entities, Database     │
└─────────────────────────────────────────┘
```

### Layer Responsibilities

**Uber.Ui (Presentation Layer)**

* MVC Controllers for HTTP request handling
* SignalR Hubs for real-time communication
* Razor views for user interface
* Client-side JavaScript for interactive features

**Uber.BLL (Business Logic Layer)**

* RideService: Manages ride lifecycle and business rules
* DriverService: Handles driver selection and availability
* Coordinate-based distance calculations
* Business logic validation

**Uber.DAL (Data Access Layer)**

* Entity models (Ride, Driver, User)
* Repository pattern implementation
* Database context and migrations
* Data persistence layer

---

## 📁 Project Structure

```
Uber/
├── src/
│   ├── Uber.Ui/                 # MVC, Razor, SignalR and startup
│   ├── Uber.Domain/             # Domain contracts and Result Pattern
│   ├── Uber.Application/        # CQRS, mediator and pipelines
│   ├── Uber.Infrastructure/     # Infrastructure foundation
│   ├── Uber.BLL/                # Existing services, pending migration
│   └── Uber.DAL/                # Existing persistence, pending migration
├── tests/
│   ├── Uber.Domain.Tests/
│   └── Uber.Application.Tests/
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
* SQL Server (or SQL Server Express)
* Visual Studio 2022 or VS Code
* Modern web browser with JavaScript enabled

Run from the repository root:

```sh
dotnet restore Uber.sln
dotnet build Uber.sln --no-restore
dotnet test Uber.sln --no-build
dotnet run --project src/Uber.Ui/Uber.Ui.csproj
```

Application startup requires local database and external-service settings.
The local `src/Uber.Ui/appsettings.json` is ignored by Git. Contract tests
do not require a database or external services.

---

## 🙏 Acknowledgments

* Built with ASP.NET Core MVC
* Real-time communication powered by SignalR
* Inspired by modern ride-hailing platforms

---

⭐ **Star this repository if you find it helpful!**


