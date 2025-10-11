# 🚗 Uber System

A complete ride-hailing application built with ASP.NET Core MVC, featuring real-time communication between riders and drivers using SignalR.

## 🌟 Features

- **Real-time Ride Requests**: Instant notifications to drivers using SignalR
- **Smart Driver Matching**: Automatic selection of nearest available driver based on coordinates
- **Live Status Updates**: Real-time ride status tracking for both riders and drivers
- **Automatic Fallback**: Seamless transition to next available driver if current driver rejects
- **Comprehensive Error Handling**: User-friendly error messages and graceful degradation

## 🏗️ Architecture

The application follows a clean three-layer architecture:

```
┌─────────────────────────────────────────┐
│  Presentation Layer (Uber.PLL)          │
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

**Uber.PLL (Presentation Layer)**
- MVC Controllers for HTTP request handling
- SignalR Hubs for real-time communication
- Razor views for user interface
- Client-side JavaScript for interactive features

**Uber.BLL (Business Logic Layer)**
- RideService: Manages ride lifecycle and business rules
- DriverService: Handles driver selection and availability
- Coordinate-based distance calculations
- Business logic validation

**Uber.DAL (Data Access Layer)**
- Entity models (Ride, Driver, User)
- Repository pattern implementation
- Database context and migrations
- Data persistence layer

## 🚀 Getting Started

### Prerequisites

- .NET 6.0 or higher
- SQL Server (or SQL Server Express)
- Visual Studio 2022 or VS Code
- Modern web browser with JavaScript enabled

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
- User submits pickup and destination coordinates
- System calculates distances to all active drivers
- Nearest driver is selected and notified via SignalR
- Ride status set to `Pending`

**2. Driver Response**
- Driver sees incoming request in real-time
- Can accept or reject within the dashboard
- Response triggers immediate status update

**3. Acceptance Path**
- Ride status updated to `Accepted`
- User redirected to trip tracking page
- Driver information shared with user

**4. Rejection Path**
- System automatically finds next nearest driver
- New notification sent via SignalR
- Process repeats until driver accepts or no drivers remain

## 📁 Project Structure

```
Uber/
├── Uber.PLL/                          # Presentation Layer
│   ├── Controllers/
│   │   ├── RideController.cs          # Ride request handling
│   │   ├── DriverController.cs        # Driver dashboard
│   │   └── AccountController.cs       # Authentication
│   ├── Hubs/
│   │   └── RideHub.cs                 # SignalR real-time hub
│   ├── Views/
│   │   ├── Ride/
│   │   │   ├── Request.cshtml         # Ride request form
│   │   │   ├── WaitingForDriver.cshtml
│   │   │   ├── NoDrivers.cshtml
│   │   │   └── Test.cshtml            # Testing interface
│   │   └── Driver/
│   │       └── Dashboard.cshtml        # Driver interface
│   └── wwwroot/                       # Static files (CSS, JS)
│
├── Uber.BLL/                          # Business Logic Layer
│   ├── Services/
│   │   ├── RideService.cs             # Ride business logic
│   │   ├── DriverService.cs           # Driver management
│   │   └── Interfaces/
│   └── DTOs/                          # Data transfer objects
│
└── Uber.DAL/                          # Data Access Layer
    ├── Entities/
    │   ├── Ride.cs                    # Ride entity
    │   ├── Driver.cs                  # Driver entity
    │   └── User.cs                    # User entity
    ├── Repo/
    │   ├── RideRepo.cs                # Ride repository
    │   └── Interfaces/
    ├── Enums/
    │   └── RideStatus.cs              # Pending, Accepted, Rejected, etc.
    └── Data/
        └── ApplicationDbContext.cs     # EF Core context
```

## 🙏 Acknowledgments

- Built with ASP.NET Core MVC
- Real-time communication powered by SignalR
- Inspired by modern ride-hailing platforms

---

⭐ **Star this repository if you find it helpful!**
