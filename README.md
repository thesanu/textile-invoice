# 🧵 Textile Invoice Management System

A professional desktop application for managing textile business invoices, clients, designs, and billing — built with **Windows Forms (.NET)** and **SQL Server**.

---

## 📸 Screenshots

| Dashboard | Create Invoice |
|-----------|---------------|
| Live stats, recent invoices | Full invoice form with GST calculation |

---

## ✨ Features

### 🔐 Authentication
- Secure login with username & password
- Multi-company support — each user is linked to a company profile
- Session management across all pages

### 📊 Dashboard
- Live stat cards: Total Clients, Designs, Invoices, Revenue
- Today's invoice count and revenue
- Recent invoices table with client name and grand total

### 👥 Account Master
- Full CRUD for clients, brokers, and transport accounts
- Fields: Code, Name, Group, GSTIN, PAN, Billing Address, City, State, Phone, Email
- Search by name, city, or type

### 🎨 Design Master
- Manage fabric designs with HSN code, default rate, and unit
- Used to auto-fill invoice item rows

### 🧾 Create Invoice
- Auto-generated sequential invoice numbers (e.g. `000001`)
- Bill To dropdown → auto-fills address, GSTIN, State
- Transport and Broker dropdowns
- Challan No and Challan Date fields
- **Items grid:** Sr | Description | HSN Code | P.CH.NO | CO.CH.NO | PCS | QTY | RATE | PER | AMOUNT
- Design selection auto-fills HSN and Rate
- AMOUNT = QTY × RATE (calculated live)
- GST section: CGST % + SGST % + IGST % (enter % → rupee amount calculated)
- Roundup field (manual adjustment)
- **Grand Total** and **Amount in Words** (auto-generated)
- Save to database with full transaction support

### 📋 Invoice List
- View all invoices with date range filter
- Search by client name or invoice number
- Shows Amount, CGST, SGST, IGST, Grand Total per invoice

### ⚙️ Service Master
- Manage services offered by the company

### 📈 Reports
- Invoice summary reports with date range filter

### 🏢 Settings
- Edit company profile: name, GSTIN, PAN, address, phone, email

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| UI | Windows Forms (.NET) |
| Language | C# |
| ORM | Entity Framework Core |
| Database | Microsoft SQL Server |
| DB Driver | Microsoft.Data.SqlClient |

---

## 🗄️ Database Schema

### Tables

| Table | Purpose |
|-------|---------|
| `APP_USERS` | User login credentials |
| `COMPANY_PROFILE` | Company details (GSTIN, address, etc.) |
| `ACCOUNTS` | Clients, brokers, transport accounts |
| `DESIGN_MASTER` | Fabric designs with HSN and rates |
| `INVOICE_HEADER` | Invoice header (date, client, GST, totals) |
| `INVOICE_ITEMS` | Invoice line items |
| `INVOICE_NUMBER_TRACKER` | Auto-increment invoice number per company |
| `SERVICE_MASTER` | Services offered |

### Key Columns Added (Migration)

```sql
-- INVOICE_HEADER
ALTER TABLE INVOICE_HEADER
    ADD CHALLAN_NO   NVARCHAR(50)  NULL,
        CHALLAN_DATE DATE          NULL,
        CGST_PCT     DECIMAL(5,2)  NULL DEFAULT 0,
        SGST_PCT     DECIMAL(5,2)  NULL DEFAULT 0,
        IGST_PCT     DECIMAL(5,2)  NULL DEFAULT 0,
        ROUNDUP      DECIMAL(10,2) NULL DEFAULT 0;
```

---

## 🚀 Getting Started

### Prerequisites

- Windows 10/11
- [.NET 6 or later](https://dotnet.microsoft.com/download)
- Microsoft SQL Server (2016 or later)
- Visual Studio 2022

### Setup

**1. Clone the repository**
```bash
git clone https://github.com/YOUR_USERNAME/Textile_Invoice_App.git
cd Textile_Invoice_App
```

**2. Configure the database connection**

Open `AppDbContext.cs` and update the connection string:
```csharp
private static DbContextOptions<AppDbContext> GetOptions()
{
    return new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=YOUR_SERVER;Database=YOUR_DB;User ID=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True")
        .Options;
}
```

**3. Run the DB migration SQL**

Execute the schema SQL in SSMS to create all required tables and columns.

**4. Scaffold models (optional, if DB changes)**
```powershell
Scaffold-DbContext "YOUR_CONNECTION_STRING" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context AppDbContext -Force -NoOnConfiguring
```

**5. Build and run**

Open `Textile_Invoice_App.sln` in Visual Studio → Press `F5`

---

## 📁 Project Structure

```
Textile_Invoice_App/
├── Models/
│   ├── AppUser.cs
│   ├── CompanyProfile.cs
│   ├── Account.cs
│   ├── DesignMaster.cs
│   ├── InvoiceHeader.cs
│   ├── InvoiceItem.cs
│   ├── InvoiceNumberTracker.cs
│   └── ServiceMaster.cs
├── AppDbContext.cs
├── SessionManager.cs
├── Login.cs / Login.Designer.cs
├── Dashboard.cs / Dashboard.Designer.cs
├── UC_Dashboard.cs
├── UC_AccountMaster.cs
├── UC_DesignMaster.cs
├── UC_CreateInvoice.cs
├── UC_InvoiceList.cs
└── UC_ServiceMaster_Reports_Settings.cs
```

---

## 🔑 Default Login

Create a user record in `APP_USERS` manually on first run:

```sql
INSERT INTO APP_USERS (USERNAME, PASSWORD_HASH, FULL_NAME, IS_ACTIVE, COMPANY_PROFILE_ID)
VALUES ('admin', 'your_password', 'Administrator', 1, 1);
```

> ⚠️ Passwords are stored as plain text in this version. Add hashing (e.g. BCrypt) before production deployment.

---

## 📌 Known Limitations

- Print/PDF invoice export — coming soon
- Password hashing not yet implemented
- Single currency (INR ₹) only

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Commit your changes: `git commit -m "Add my feature"`
4. Push to the branch: `git push origin feature/my-feature`
5. Open a Pull Request

---

## 👨‍💻 Author

**Ravi Ranjan** — RR Enterprises  
Textile Invoice Management System

---

## 📄 License

This project is licensed under the MIT License.
