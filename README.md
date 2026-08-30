# Municipality-System-Admnistration
eThekwini Municipality struggles to effectively traCurrentlyck, monitor, and manage its 
physical assets. Asset information is stored in spreadsheets or manual records, which 
leads to inaccurate data, loss of assets, duplicate purchases, and inefficient maintenance 
scheduling. There is also a lack of accountability on who is responsible for each asset. 
These challenges result in increased operational costs, reduced productivity, and poor 
decision-making regarding asset procurement and disposal.

The system improves the Asset tracking and monitoring, manage assets accordingly. Assign assets to users, which will held everyone accountable.

Assets Management System
•User Registration and login
•Secure Authentication 
•Dashboard
•Update asset status
•Schedule Maintenance
•Submit Maintenance request 
•View request history 
•Track asset maintenance 
•Edit user profile
•Password reset 
•User information 
•Asset information 
•Delete User
•Delete Asset 


System Administrator portal
•System Administrator login 
•Create users
•Select their roles
•Create user password 
•Update user information 
•Manage permissions

Asset Manager portal
•Asset Manager login
•Add new asset 
•Manage asset
•Update status
•Schedule maintenance 
•Requests disposal
•Assign Technician
.Record disposal

Department head portal
•Login
•Dashboard 
•View reports 
•Approve requests

Technician portal
•Login
•Dashboard 
.Schedule maintenance
.Record maintenance
.Update maintenance
.Generate maintenance history

Finance officer portal
.Manage depreciation
.Approval disposal

Backend
Technology Version purpose 
ASP.NET MVC 5.2.7Web Framework
Entity Framework 6 6.4.4 ORM for database operations 
ASP.NET identity 2.2.3 Authentication & Authorization 
OWIN 4.2.2 Middleware & Startup configuration
C# 7.0+ Programming language 
Unity Container 5.11.10 Dependency Injection

Frontend

Technology Version Purpose 
Bootstarp 3.4.1 Responsive UI framework
jQuery 3.5.1 DOM manipulation & AJAX 
jQuery Validation 111.19.3 CLient-side validation 
Font Awesome 6.5.1 Icons
HTML5/CSS3 - structure & styling 
Razor Syntax - Server-side templating

Database 

Technology Version Purpose
SQL Server 2019+ Database
SQL Server LocalDB - Development database 
Entity Framework Migrations

API & Services

Service Purpose
Google Maps API Location & Searching

 System Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                                     │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────────────────────┐  │
│  │    Views    │  │  CSS/JS    │  │     Layouts/Partials              │  │
│  │  (.cshtml)  │  │  (Static)  │  │        (_Layout)                  │  │
│  └─────────────┘  └─────────────┘  └──────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                                       │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────────────────────┐  │
│  │ Controllers │  │  ViewModels │  │  Filters/Attributes              │  │
│  │   (MVC)    │  │    (DTOs)   │  │    ([Authorize])                 │  │
│  └─────────────┘  └─────────────┘  └──────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    BUSINESS LAYER                                          │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────────────────────┐  │
│  │   Models    │  │   Services  │  │  Business Rules                  │  │
│  │   (POCO)    │  │   (Logic)   │  │  (Validation)                    │  │
│  └─────────────┘  └─────────────┘  └──────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    DATA ACCESS LAYER                                       │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────────────────────┐  │
│  │   DbContext │  │   Migrations│  │   Repositories                   │  │
│  │     (EF6)   │  │   (Schema)  │  │   (Queries)                      │  │
│  └─────────────┘  └─────────────┘  └──────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    DATABASE LAYER                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    SQL Server / SQL Express                         │  │
│  │              (Tables, Views, Stored Procedures)                     │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

👥 User Roles

Role Responsibilities
Admin Full system control, user management, role assignment, asset oversight
Asset Manager Create and manage assets, request repairs, assign technicians, quick disposal
Technician View and complete repair tasks, start/complete repairs
Department Head Approve repair requests, view department staff and reports
Finance Officer Approve disposal requests, manage depreciation, financial reports
Municipal Employee View assets, request disposal (if assigned)

---

📥 Installation Guide

Prerequisites

Software Version Download
Visual Studio 2019 or 2022 Download
.NET Framework 4.7.2 or 4.8 Included with VS
SQL Server 2019 Express or LocalDB Download
Git Latest Download

Step 1: Clone the Repository

```cmd
git clone https://github.com/nutuhuko07/Municipality-Management-System.git
cd Municipality-Management-System
```

Step 2: Open the Project

1. Open Visual Studio
2. Click Open a project or solution
3. Navigate to the project folder
4. Select Municipality System Administration.sln
5. Click Open

Step 3: Restore NuGet Packages

In Visual Studio:

1. Right-click the solution in Solution Explorer
2. Select "Restore NuGet Packages"
3. Wait for packages to download

Or via Package Manager Console:

```powershell
Update-Package -Reinstall
```

Step 4: Update the Database

In Package Manager Console:

```powershell
Update-Database -Verbose
```

This will:

· Create the database
· Create all tables
· Seed default roles
· Create default admin user

Step 5: Configure Google Maps API (Optional)

1. Get a Google Maps API key from Google Cloud Console
2. Add to Web.config:

```xml
<appSettings>
    <add key="GoogleMapsApiKey" value="YOUR_API_KEY_HERE" />
</appSettings>
```

Step 6: Build and Run

1. Press F5 (Start Debugging) or Ctrl + F5 (Start Without Debugging)
2. The application will open in your browser

---

🔑 Default Login Credentials

Role Email Password
Admin systemadmin@gmail.com Admin@123
Asset Manager (Create via Admin) (Set during creation)
Technician (Create via Admin) (Set during creation)
Department Head (Create via Admin) (Set during creation)
Finance Officer (Create via Admin) (Set during creation)

---

📁 Project Structure

```
Municipality System Administration/
├── Controllers/
│   ├── AccountController.cs          # Authentication
│   ├── AdminController.cs             # Admin dashboard
│   ├── AssetsController.cs            # Asset CRUD operations
│   ├── DepartmentHeadController.cs    # Department Head actions
│   ├── DisposalController.cs          # Disposal management
│   ├── FinanceController.cs           # Finance & depreciation
│   ├── HomeController.cs              # Landing page
│   ├── MaintenanceController.cs       # Maintenance scheduling
│   ├── StaffController.cs             # Staff management
│   └── TechnicianController.cs        # Technician actions
├── Models/
│   ├── AccountViewModels.cs           # Login/Register models
│   ├── Asset.cs                       # Asset entity
│   ├── DisposalRequest.cs             # Disposal request entity
│   ├── IdentityModels.cs              # Identity & DbContext
│   ├── ManageViewModels.cs            # Password change models
│   └── Staff.cs                       # Staff entity
├── Views/
│   ├── Account/                       # Login/Register pages
│   ├── AdminDashboard/                # Admin dashboard
│   ├── Assets/                        # Asset views
│   ├── DepartmentHead/                # Department Head views
│   ├── Disposal/                      # Disposal views
│   ├── Finance/                       # Finance views
│   ├── Home/                          # Home page
│   ├── Maintenance/                   # Maintenance views
│   ├── Shared/                        # Layout & partials
│   ├── Staff/                         # Staff management views
│   └── Technician/                    # Technician views
├── Services/
│   └── DepreciationService.cs         # Depreciation calculations
├── App_Start/
│   ├── BundleConfig.cs                # CSS/JS bundling
│   ├── FilterConfig.cs                # Global filters
│   ├── RouteConfig.cs                 # URL routing
│   └── Startup.Auth.cs                # OWIN authentication
├── Migrations/                        # EF migrations
├── Scripts/                           # JavaScript files
├── Web.config                         # Application configuration
└── packages.config                    # NuGet packages
```

---

🔄 Workflows

Asset Lifecycle

```
CREATE → ASSIGN → REPAIR → MAINTENANCE → DISPOSAL
   │         │         │         │           │
   │         │         │         │           ▼
   │         │         │         │      DISPOSED
   │         │         │         ▼
   │         │         │    SCHEDULED
   │         │         ▼
   │         │    REPAIRED
   │         ▼
   │     ASSIGNED
   ▼
 NEW ASSET
```

Repair Workflow

```
Asset Manager Request → Department Head Approve → Technician Work → Admin Approve
         │                      │                      │              │
         ▼                      ▼                      ▼              ▼
    PENDING              APPROVED              IN PROGRESS      AVAILABLE
```

Disposal Workflow

```
Asset Manager Request → Finance Officer Approve → Asset Disposed
         │                      │                      │
         ▼                      ▼                      ▼
     PENDING             APPROVED/REJECTED      DISPOSED
```

---

📸 Screenshots

Login Page

screenshots/login.png

Admin Dashboard

screenshots/admin-dashboard.png

Asset Management

screenshots/asset-index.png

Asset Registration

screenshots/asset-create.png

Staff Management

screenshots/staff-index.png

---

🤝 Contributing

1. Fork the repository
2. Create a feature branch

```cmd
git checkout -b feature/your-feature-name
```

3. Commit your changes

```cmd
git commit -m "Add your feature description"
```

4. Push to the branch

```cmd
git push origin feature/your-feature-name
```

5. Open a Pull Request

Guidelines:

· Follow the existing code style
· Write meaningful commit messages
· Add comments for complex logic
· Test your changes thoroughly

---

📄 License

This project is for educational purposes. All rights reserved.

---

📞 Support

For any issues or questions:

· Email: support@municipality.gov.za
· Create an Issue: GitHub Issues

---

🙏 Acknowledgments

· Visual Studio Community - IDE
· GitHub - Version control
· ASP.NET MVC - Web framework
· Bootstrap - UI framework
· Font Awesome - Icons
· Google Maps Platform - Location services

---

📊 Version History

Version Date Changes
2.0 August 2026 Full release with all features
1.0 July 2026








