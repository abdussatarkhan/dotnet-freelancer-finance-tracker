# Freelancer Invoicing & Expense Tracker API

A production-grade, enterprise-ready REST API built with **.NET 8**, **PostgreSQL**, and **Clean Architecture**. Designed for freelancers and independent agencies to manage multi-currency client invoicing, milestones, automated recurring retainer subscriptions, billable expense reimbursements, receipt attachments, and financial analytics.

---

## 🏛️ Architectural Overview

This solution follows **Clean Architecture** and **Domain-Driven Design (DDD)** principles:

* **`FreelancerTrack.Domain`**: Pure C# domain model with zero external dependencies. Contains core business entities (`Client`, `Project`, `Milestone`, `Invoice`, `InvoiceItem`, `RecurringProfile`, `Expense`, `ReceiptAttachment`), domain enums, auditable base classes, soft-delete interfaces, and optimistic concurrency tokens.
* **`FreelancerTrack.Application`**: CQRS implementation with **MediatR**, functional **`Result<T>`** pattern, automatic validation via **FluentValidation** pipeline behaviors, application interfaces, and DTO isolation.
* **`FreelancerTrack.Infrastructure`**: PostgreSQL persistence via `Npgsql.EntityFrameworkCore.PostgreSQL`, entity Fluent API mappings (`numeric(12,2)` monetary fields, `numeric(18,6)` historical exchange rate snapshotting, PostgreSQL concurrency versioning), SHA-256 file storage service, automated background worker (`RecurringInvoiceBillingWorker`), and database seeders.
* **`FreelancerTrack.Api`**: ASP.NET Core Web API controllers, multipart form-data receipt upload & streaming download, centralized RFC 7807 `ProblemDetails` exception middleware, JWT Bearer authentication, and Swagger OpenAPI documentation.

---

## 🚀 Key Features

### 1. Projects & Milestones Management
* Complete client profiles with billing addresses, VAT/tax IDs, and default currencies.
* Projects with budget tracking, timelines, and status lifecycle.
* Milestones (`Pending` ➔ `InProgress` ➔ `Completed` ➔ `Billed` ➔ `Paid`).
* Ability to transition completed milestones directly into invoice line items.

### 2. Multi-Currency Invoicing Engine
* Base currency is `USD`, with native support for `EUR`, `GBP`, `PKR`, `CAD`, `AUD`.
* **Historical Rate Freeze**: Each invoice records both the transactional currency amount and the historical exchange rate at issue time (`ExchangeRateToBase`), ensuring immutable tax reporting.
* Invoice Lifecycle: `Draft` ➔ `Issued` ➔ `PartiallyPaid` ➔ `Paid` ➔ `Overdue` ➔ `Void`.
* Dynamic line items supporting milestones, hourly rates, retainer billing, and billable expense recharges.

### 3. Automated Recurring Client Billing (Retainers)
* Recurring profiles (`Weekly`, `Monthly`, `Quarterly`, `Yearly`) with automated next-run date calculation.
* A hosted background worker (`RecurringInvoiceBillingWorker`) scans PostgreSQL periodically, generates draft/issued retainer invoices in isolated transactions, and automatically advances schedules.

### 4. Expense Logging & Billable Reimbursement
* Categorized expense tracking (Software, Hardware, Travel, Subcontractor, Office) with gross amount, tax, currency, and payment method.
* Billable vs. non-billable flags with reimbursement tracking (`NotBillable`, `Unbilled`, `Reimbursed`).
* Receipt attachment storage with SHA-256 integrity hashing and secure streaming download endpoints.

### 5. Financial Reports & PostgreSQL Aggregations
* **Net Cash Flow**: Total paid invoice revenue converted to base currency minus total business expenses.
* **Accounts Receivable Aging**: Grouped outstanding receivables (Current, 1–30 days, 31–60 days, 61–90 days, 90+ days overdue).
* **Project Profitability**: Invoiced revenue vs. accumulated expenses and profit margins per project.

---

## ⚙️ Getting Started

### Prerequisites
* [.NET 8 SDK or .NET 10 SDK](https://dotnet.microsoft.com/download)
* [PostgreSQL 14+](https://www.postgresql.org/download/)

### Configuration
Update the connection string in `src/FreelancerTrack.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=FreelancerTrackDb;Username=postgres;Password=your_password;"
}
```

### Running the API
From the root solution directory:
```bash
dotnet run --project src/FreelancerTrack.Api/FreelancerTrack.Api.csproj
```
On startup:
1. EF Core automatically migrates the database schema.
2. The seeder populates realistic clients, projects, milestones, multi-currency invoices, retainers, and expenses.
3. The background worker initializes and starts scanning for due schedules.
4. Navigate to `https://localhost:7290/` or `http://localhost:5094/` to open the **Swagger UI**.

---

## 🔐 Authentication & Testing

Generate a test JWT Bearer token:
* Send a `POST` request to `/api/auth/token`:
  ```json
  {
    "email": "freelancer@example.com",
    "password": "Password123!"
  }
  ```
* Copy the returned token, click **Authorize** in Swagger, and paste: `Bearer <your_token>`.
