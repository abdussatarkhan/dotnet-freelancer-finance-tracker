# .NET Freelancer Finance & Invoice Management Platform

<div align="center">

[![Daily Streak](https://img.shields.io/badge/Daily%20Streak-Active%20%F0%9F%94%A5-brightgreen?style=flat-square&logo=github)](https://github.com/abdussatarkhan)
[![Master Portfolio](https://img.shields.io/badge/Portfolio-50%2B%20Enterprise%20Projects-0e75b6?style=flat-square&logo=github)](https://github.com/abdussatarkhan/abdussatarkhan)
[![Author: Abdussatar](https://img.shields.io/badge/Author-Abdussatar-24292e?style=flat-square&logo=github)](https://github.com/abdussatarkhan)

</div>


[![CI](https://github.com/abdussatarkhan/dotnet-freelancer-finance-tracker/actions/workflows/ci.yml/badge.svg)](https://github.com/abdussatarkhan/dotnet-freelancer-finance-tracker/actions)
[![.NET](https://img.shields.io/badge/.NET_8-ASP.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/) [![C#](https://img.shields.io/badge/C%23-Clean_Architecture-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/) [![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF_Core-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/) [![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![Author](https://img.shields.io/badge/Author-Abdussatar-E50914?style=for-the-badge&logo=github&logoColor=white)](https://github.com/abdussatarkhan)

> **A Clean Architecture ASP.NET Core Web API and background service platform with PostgreSQL and Entity Framework Core — managing freelance client contracts, automated recurring invoicing, multi-currency revenue analytics, and tax forecasting.**

---

## 🏛️ System Architecture

```mermaid
graph TD
    API[ASP.NET Core Web API Controllers] --> MediatR[CQRS MediatR Application Handlers]
    MediatR --> Domain[Domain Entities: Invoices, Clients, Ledger]
    MediatR --> Infra[EF Core & PostgreSQL Repositories]
    Infra --> DB[(PostgreSQL Database)]
    Worker[Hosted Background Worker: Auto-Invoicing & Reminders] --> MediatR
```

---

## 🌟 Key Features & Capabilities

- **Production-Grade Implementation**: Built with modular design patterns, type safety, and clean separation of concerns.
- **Robust Ledger & Data Persistence**: ACID-compliant transactions and optimized queries.
- **Comprehensive Tech Stack**: `C#` `ASP.NET Core` `.NET 8` `EF Core` `PostgreSQL` `Clean Architecture` `Docker`.

---

## 🚀 Quickstart & Setup

### 1. Clone the Repository
```bash
git clone https://github.com/abdussatarkhan/dotnet-freelancer-finance-tracker.git
cd dotnet-freelancer-finance-tracker
```

---

## 🗺️ Roadmap & Upcoming Features

- [x] ASP.NET Core Web API with CQRS MediatR architecture
- [x] PostgreSQL database and recurring billing background workers
- [ ] Multi-currency exchange rate live API sync
- [ ] Automated PDF invoice email delivery
- [ ] Quarterly tax liability forecasting module

---

## 👨‍💻 Author & Profile

Built and maintained by **Abdussatar** ([@abdussatarkhan](https://github.com/abdussatarkhan)).  
For collaboration or queries, feel free to reach out via [LinkedIn](https://www.linkedin.com/in/abdus-satar-5150813b5/) or [GitHub](https://github.com/abdussatarkhan).

---

## 📜 License

This project is licensed under the **MIT License** — see the LICENSE file for details.


---

<div align="center">

### 👨‍💻 Maintained by [Abdussatar (@abdussatarkhan)](https://github.com/abdussatarkhan)
Part of the **[Master Enterprise Data Analytics & AI Portfolio](https://github.com/abdussatarkhan/abdussatarkhan)**.

⭐ If you find this repository valuable, consider dropping a star! ⭐

</div>
