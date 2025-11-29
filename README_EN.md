# 💰 FinanceFlow - Visual Financial Goals Tracker

[![](https://img.shields.io/badge/AvaloniaUI-blue?style=flat&logo=avaloniaui&labelColor=333333&color=1e90ff)]() [![](https://img.shields.io/badge/.NET-8.0-purple?style=flat&logo=dotnet&labelColor=333333&color=8A2BE2)]() [![](https://img.shields.io/badge/PostgreSQL-424242?style=flat&logo=postgresql&labelColor=333333&color=30689b)]()
[![](https://img.shields.io/badge/Entity%20Framework-Core-green?style=flat&logo=entityframework&labelColor=333333&color=008000)]() [![](https://img.shields.io/badge/QuestPDF-PDF-red?style=flat&labelColor=333333&color=EF4444)]() [![](https://img.shields.io/badge/Architecture-MVVM-yellowgreen?style=flat&labelColor=333333&color=9CCC65)]()

---

<p align="center">
  <a href="README_RU.md">Читать на русском 🇷🇺</a>
</p>

**FinanceFlow** is a professional desktop financial goals tracker. The application is designed for those who want to visualize their savings, track progress in real-time, and get detailed analytics in PDF format.

Unlike regular Excel spreadsheets, FinanceFlow provides a modern graphical interface, protection against input errors, and a gamified approach to savings.

---

### **Table of Contents**

1.  [Features](#features)
2.  [Technologies](#technologies)
3.  [Installation & Database](#installation--database)
4.  [Build & Run](#build--run)
5.  [Gallery](#gallery)

---

### **Features**<a id="features"></a>

- 🎯 **Goal Management:** Create goals with images, categories, and priorities. Full CRUD cycle (Create, Read, Update, Delete).

- 💳 **Transaction History:** Make deposits, edit deposit history. Progress bar recalculates automatically.
- 📊 **Analytics:** Dashboard with pie chart (colors generated dynamically), savings statistics, and list of upcoming deadlines.
- 📄 **Reporting:** Generate professional PDF reports with summaries and tables. Files are saved to the `Documents/FinanceFlow-Reports` folder.
- 🛡️ **Data Security:** Validation system prevents incorrect data input (such as negative amounts or invalid dates) by displaying clear notifications.

---

### **Technologies**<a id="technologies"></a>

The project is built on a modern technology stack. Click on a technology to go to the official download site.

- **Language:** [C# / .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) — Main platform.
- **UI Framework:** [AvaloniaUI 11.3.8](https://avaloniaui.net/) — Cross-platform XAML.
- **Database:** [PostgreSQL 16+](https://www.postgresql.org/download/) — Reliable relational DBMS.
- **ORM:** [Entity Framework Core 8](https://learn.microsoft.com/en-us/ef/core/) — Data access through objects.
- **PDF:** [QuestPDF](https://www.questpdf.com/) — Document generation.

---

### **Installation & Database**<a id="installation--database"></a>

⚠️ **Important:** A PostgreSQL server installation is required for the application to work.

**Default credentials:**
The application is configured to connect with the following data:

- **Database:** `financeflow_db`
- **User:** `financeflow_user`
- **Password:** `Ff_Postgres_Mdk_2025!`

> If you want to use your own login/password, open the `Data/AppDbContext.cs` file and change the connection string in the `OnConfiguring` method.

#### **Method 1: Automatic Setup (Windows)**

There is a `setup_db.bat` file in the project root.

1.  Run it as administrator (or just double-click).
2.  The script will automatically:
    - Create the user and database.
    - **Create the table structure.**
    - Populate the database with test categories from the `seed_data.sql` file.
      _(Requires PostgreSQL to be in the PATH environment variable)_.

#### **Method 2: Manual Setup (SQL)**

If you prefer to configure the database manually (via pgAdmin or psql):

1.  **Create database and user:**

    ```sql
    CREATE USER financeflow_user WITH PASSWORD 'Ff_Postgres_Mdk_2025!';
    CREATE DATABASE financeflow_db OWNER financeflow_user;
    ALTER USER financeflow_user CREATEDB;
    ```

2.  **Create tables and populate data:**
    Find the `seed_data.sql` file in the project root and execute its contents in the created `financeflow_db` database. This script will create all necessary tables and add goal categories.

#### **Useful scripts in the project:**

- `seed_data.sql` — Full initialization script (drop old, create tables, insert data).
- `cleanup.sql` — Script for complete database cleanup (Drop All).

---

### **Build & Run**<a id="build--run"></a>

#### **Option A: Ready EXE (Without .NET Installation)**

1.  Go to the **[Releases](../../releases)** section of this repository.
2.  Download the `FinanceFlow_Release_v1.0.zip` archive.
3.  Run `FinanceFlow.exe`.

#### **Option B: Run from Source Code**

Requires installed .NET 8 SDK.

1.  **Cloning:**

    ```bash
    git clone https://github.com/RamenOfficialGovPatsy/FinanceFlow.git
    cd FinanceFlow
    ```

2.  **Database Setup (via EF Core):**
    If you haven't used the SQL scripts above, you can create the table structure with the command:

    ```bash
    dotnet ef database update
    ```

3.  **Run:**
    ```bash
    dotnet run
    ```

---

### **🖼️ Gallery**<a id="gallery"></a>

### Main Dashboard and Goals Overview

[![Main Dashboard](screenshots/1_main_dashboard.png)](screenshots/1_main_dashboard.png)
_Main menu with goals overview and progress bar_

### Goal Management

[![Create Goal](screenshots/5_goal_create_top.png)](screenshots/5_goal_create_top.png)
_New goal creation form_

[![Goal Settings](screenshots/6_goal_create_bottom.png)](screenshots/6_goal_create_bottom.png)
_New goal creation form (part 2)_

### Deposits and Transaction History

[![Deposit](screenshots/7_deposit_form.png)](screenshots/7_deposit_form.png)
_Making a deposit_

[![History](screenshots/8_deposit_history.png)](screenshots/8_deposit_history.png)
_Transaction journal_

### Analytics and Details

[![Dashboard](screenshots/2_analytics_view.png)](screenshots/2_analytics_view.png)
_General statistics_

[![Chart](screenshots/3_analytics_chart.png)](screenshots/3_analytics_chart.png)
_Distribution by categories_

[![Deadlines](screenshots/4_analytics_deadlines.png)](screenshots/4_analytics_deadlines.png)
_Upcoming deadlines_

### Reporting

[![PDF Report](screenshots/9_report_preview_summary.png)](screenshots/9_report_preview_summary.png)
_Report page 1_

[![Table](screenshots/10_report_preview_table.png)](screenshots/10_report_preview_table.png)
_Report page 2_

### Validation Demo

[![Name Error](screenshots/11_error_empty_name.png)](screenshots/11_error_empty_name.png)
_Mandatory field check - goal name_

[![Amount Error](screenshots/12_error_zero_amount.png)](screenshots/12_error_zero_amount.png)
_Entered amount correctness control_

[![Mismatch](screenshots/13_error_sum_mismatch.png)](screenshots/13_error_sum_mismatch.png)
_Protection against exceeding target amount_
