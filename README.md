# FitTrack Pro - Gym Management System

![FitTrack Pro Logo](https://img.shields.io/badge/FitTrack%20Pro-Gym%20Management-blueviolet?style=for-the-badge&logo=.net)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core%208.0-512BD4?style=flat&logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-512BD4?style=flat&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat&logo=microsoftsqlserver)

**FitTrack Pro** is a modern, comprehensive Gym Management System built with ASP.NET Core MVC. It streamlines gym operations, fitness tracking, and class scheduling, providing a professional experience for administrators, trainers, and members.

---

## 🎬 Live Demo

Experience **FitTrack Pro** in action! Watch our project demonstration video:
👉 **[Watch Video Demo on Google Drive](https://drive.google.com/drive/folders/1dzHmwZwQSa-KDxX38w0JeALInTtfxzcm?usp=drive_link)**

---

## 🚀 Key Features

### 📅 Advanced Class Scheduling
- **Weekly Calendar View**: Visualize gym classes in an interactive weekly schedule.
- **Role-based Management**: Admins can manage class assignments and capacities.
- **Real-time Updates**: Powered by a robust service layer ensuring data consistency.

### 💬 Real-time Communication
- **Member-Staff Chat**: Built with **SignalR** for instant messaging.
- **Categorized Discovery**: Easily find and chat with staff or fellow members.
- **Persistent History**: Track conversations for better member support.

### 👥 User Roles & Dashboards
- **Admin**: Full control over members, trainers, plans, and reports.
- **Trainer**: Manage assigned classes and member interactions.
- **Member**: Access personalized dashboards, book classes, and track subscriptions.

### 🏗️ Architected for Growth
- **Repository & Unit of Work**: Ensures clean, testable logic.
- **Service Layer Separation**: Decouples business rules from controllers.
- **Identity Security**: Robust authentication and authorization with ASP.NET Core Identity.

---

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 8.0 (MVC Framework)
- **Database**: SQL Server with Entity Framework Core (Code-First)
- **Real-time**: ASP.NET Core SignalR
- **Frontend**: Razor Pages, Vanilla CSS, Bootstrap 5, JavaScript
- **Security**: ASP.NET Core Identity

---

## 🚦 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/AMahfouz144/FitTrack-Pro.git
   ```

2. **Update Connection String**:
   Edit `appsettings.json` with your local SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=FitTrackProDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

3. **Apply Migrations**:
   ```powershell
   Update-Database
   ```

4. **Run the Application**:
   Press `F5` in Visual Studio or use `dotnet run`.

---

## 👥 Authors & Contributors

This project was developed by a team from the Information Technology Institute (**ITI**):

| Author | Role | GitHub Profile |
|--------|------|----------------|
| **KHALEDMO07** | Lead Developer | [@KHALEDMO07](https://github.com/KHALEDMO07) |
| **AZIZ20035** | Core Developer | [@AZIZ20035](https://github.com/AZIZ20035) |
| **Ahmed-Saayed** | Core Developer | [@Ahmed-Saayed](https://github.com/Ahmed-Saayed) |
| **AdhamMohamed200** | Core Developer | [@AdhamMohamed200](https://github.com/AdhamMohamed200) |

---

## 📄 License
This project is for educational purposes as part of the ITI Training Program.
