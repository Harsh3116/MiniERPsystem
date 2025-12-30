# Mini ERP System

A role-based Mini ERP (Enterprise Resource Planning) system built using **ASP.NET Core MVC**.  
This project demonstrates real-world business workflows such as inventory management, sales tracking, reporting, and role-based access control.

---

## 🔹 Features

### Authentication & Authorization
- Session-based login system
- Role-based access (Admin / Staff)
- Secure logout handling

### Product & Inventory Management
- Add, edit, and manage products
- Low stock alerts
- Soft delete (no hard data loss)
- Restore deleted products (Admin-only)

### Sales Management
- Create and manage sales
- Automatic stock deduction
- Daily, monthly, and date-range sales reports

### Reports & Export
- Sales reports (Daily / Monthly / Date Range)
- Export reports to **Excel**
- Export reports to **PDF** (Rotativa)

### User Experience
- Search and pagination
- Clean dashboard overview
- Empty-state messages
- Responsive UI with Bootstrap

### Logging
- Activity logging for critical actions
- Audit-friendly design

---

## 🔹 Tech Stack

- **ASP.NET Core MVC**
- **Entity Framework Core**
- **SQL Server**
- **Bootstrap**
- **Rotativa.AspNetCore** (PDF Export)
- **ClosedXML** (Excel Export)

---

## 🔹 Roles & Permissions

| Role  | Permissions |
|------|------------|
| Admin | Full access (CRUD, reports, exports, restore) |
| Staff | Read-only access to data and reports |

---

## 🔹 Project Structure

MiniERPsystem/
├── Controllers/
├── Models/
├── Views/
├── Data/
├── Migrations/
├── wwwroot/
├── Program.cs
├── appsettings.json


## 🔹 How to Run the Project

1. Clone the repository
   git clone https://github.com/Harsh3116/MiniERPsystem.git
2. Open the solution in Visual Studio
3. Update database
4. Run the project

🔹 Key Learning Outcomes

1. ASP.NET Core MVC architecture
2. Role-based authorization
3. Soft delete design pattern
4. Reporting and exports
5. Git & GitHub workflow
6. Real-world ERP logic

🔹 Author
Harsh Tripathi
