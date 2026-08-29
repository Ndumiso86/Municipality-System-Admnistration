
🏛️ Municipality Asset Management System

A comprehensive, role-based asset management platform designed for municipalities to efficiently track, manage, and maintain public assets.

https://img.shields.io/badge/version-2.0-blue
https://img.shields.io/badge/ASP.NET%20MVC-5.2.7-purple
https://img.shields.io/badge/Entity%20Framework-6.4.4-green
https://img.shields.io/badge/SQL%20Server-2019-red
https://img.shields.io/badge/license-MIT-yellow

---

📋 Table of Contents

· Overview
· Features
· Technologies Used
· System Architecture
· User Roles
· Installation Guide
· Default Login Credentials
· Project Structure
· Workflows
· Screenshots
· Contributing
· License

---

📖 Overview

The Municipality Asset Management System is a web-based application designed to help municipal authorities manage their assets efficiently. It provides a centralized platform for tracking assets, managing repairs, handling disposals, and maintaining accurate financial records.

Key Objectives:

· ✅ Centralized asset tracking and management
· ✅ Role-based access control for different stakeholders
· ✅ Streamlined repair and maintenance workflows
· ✅ Financial oversight with depreciation calculations
· ✅ Complete audit trail for all asset activities

---

🚀 Features

🔐 User Management

· Role-based authentication with ASP.NET Identity
· Six user roles: Admin, AssetManager, Technician, DepartmentHead, FinanceOfficer, MunicipalEmployee
· User registration and profile management
· Account activation/deactivation

📦 Asset Management

· Complete CRUD operations for assets
· Auto-generated asset numbers (AST0001, AST0002, etc.)
· Asset categorization (Computer, Vehicle, Office Furniture, etc.)
· Asset condition tracking (New, Excellent, Good, Fair, Poor, Damaged)
· Asset status tracking (Available, Assigned, Pending Repair, In Repair, Repaired, Disposed)
· Location tracking with Google Maps integration
· Asset assignment and unassignment to staff members

🔧 Repair Management

· Repair request workflow: Request → Department Head Approval → Technician Work → Admin Approval
· Technician assignment to repair requests
· Repair report generation with cost tracking
· Repair history for each asset

📅 Maintenance Scheduling

· Schedule preventive maintenance
· Maintenance frequency tracking
· Maintenance history logging
· Record maintenance activities

♻️ Disposal Management

· Asset disposal request workflow: Asset Manager Request → Finance Officer Approval → Auto Disposal
· Disposal method selection (Auction, Scrap, Donation, Recycle, Trade-in, Write-off)
· Disposal value tracking
· Complete disposal history

💰 Financial Management

· Asset depreciation calculation (Straight-Line, Declining Balance, Double Declining)
· Financial reports generation
· Disposal value tracking
· Asset book value tracking

📊 Reporting

· Staff reports with role information
· Asset reports with status and condition
· Repair reports with cost analysis
· Financial reports with depreciation summary
· Disposal reports with value tracking

---

🛠️ Technologies Used

Backend

Technology Version Purpose
ASP.NET MVC 5.2.7 Web framework
Entity Framework 6 6.4.4 ORM for database operations
ASP.NET Identity 2.2.3 Authentication & Authorization
OWIN 4.2.2 Middleware & Startup configuration
C# 7.0+ Programming language
Unity Container 5.11.10 Dependency Injection

Frontend

Technology Version Purpose
Bootstrap 3.4.1 Responsive UI framework
jQuery 3.5.1 DOM manipulation & AJAX
jQuery Validation 1.19.3 Client-side validation
Font Awesome 6.5.1 Icons
HTML5/CSS3 - Structure & styling
Razor Syntax - Server-side templating

Database

Technology Version Purpose
SQL Server 2019+ Database
SQL Server LocalDB - Development database
Entity Framework Migrations - Schema management

APIs & Services

Service Purpose
Google Maps API Location search & mapping

---

🏗️ System Architecture

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
