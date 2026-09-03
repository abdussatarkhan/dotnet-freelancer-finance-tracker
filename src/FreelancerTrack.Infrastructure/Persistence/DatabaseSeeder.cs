using FreelancerTrack.Domain.Entities;
using FreelancerTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelancerTrack.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Clients.AnyAsync())
        {
            logger.LogInformation("Database already seeded. Skipping initial seeding.");
            return;
        }

        logger.LogInformation("Seeding database with realistic enterprise freelancer dataset...");

        DateTimeOffset now = DateTimeOffset.UtcNow;

        // 1. Clients
        var clientAcme = new Client
        {
            Name = "Acme Cloud Solutions Inc.",
            Email = "billing@acmecloud.io",
            TaxOrVatNumber = "US-EIN-88492019",
            BillingStreet = "101 Silicon Way, Suite 400",
            BillingCity = "San Francisco",
            BillingState = "CA",
            BillingPostalCode = "94107",
            BillingCountry = "United States",
            DefaultCurrency = "USD",
            IsActive = true
        };

        var clientNordic = new Client
        {
            Name = "Nordic FinTech Innovations AB",
            Email = "accounts@nordicfintech.se",
            TaxOrVatNumber = "SE-556123456701",
            BillingStreet = "Kungsgatan 14",
            BillingCity = "Stockholm",
            BillingState = "Stockholm",
            BillingPostalCode = "11143",
            BillingCountry = "Sweden",
            DefaultCurrency = "EUR",
            IsActive = true
        };

        var clientLondon = new Client
        {
            Name = "London Digital Media Partners",
            Email = "finance@ldmpartners.co.uk",
            TaxOrVatNumber = "GB-992817263",
            BillingStreet = "35 Brick Lane",
            BillingCity = "London",
            BillingState = "Greater London",
            BillingPostalCode = "E1 6PU",
            BillingCountry = "United Kingdom",
            DefaultCurrency = "GBP",
            IsActive = true
        };

        var clientIndus = new Client
        {
            Name = "Indus Capital & Tech Ventures",
            Email = "invoicing@induscap.pk",
            TaxOrVatNumber = "PK-NTN-4829104",
            BillingStreet = "74 Main Boulevard, Gulberg III",
            BillingCity = "Lahore",
            BillingState = "Punjab",
            BillingPostalCode = "54000",
            BillingCountry = "Pakistan",
            DefaultCurrency = "PKR",
            IsActive = true
        };

        context.Clients.AddRange(clientAcme, clientNordic, clientLondon, clientIndus);
        await context.SaveChangesAsync();

        // 2. Projects
        var projectMicroservices = new Project
        {
            ClientId = clientAcme.Id,
            Name = "Event-Driven Microservices Architecture",
            Description = "Design and development of real-time Kafka event streaming pipelines and microservices in .NET 8.",
            Currency = "USD",
            TotalBudget = 25000.00m,
            Status = ProjectStatus.Active,
            StartDateUtc = now.AddMonths(-2),
            EndDateUtc = now.AddMonths(2)
        };

        var projectPaymentGateway = new Project
        {
            ClientId = clientNordic.Id,
            Name = "PSD2 Compliant Payment Gateway Integration",
            Description = "Integration of Open Banking APIs and biometric 3D-Secure 2.0 authentication workflow.",
            Currency = "EUR",
            TotalBudget = 18000.00m,
            Status = ProjectStatus.Active,
            StartDateUtc = now.AddMonths(-1),
            EndDateUtc = now.AddMonths(3)
        };

        var projectSeoPlatform = new Project
        {
            ClientId = clientLondon.Id,
            Name = "Omnichannel Headless CMS & SEO Revamp",
            Description = "Full-stack migration to modern JAMstack with dynamic GraphQL catalog indexing.",
            Currency = "GBP",
            TotalBudget = 12000.00m,
            Status = ProjectStatus.Active,
            StartDateUtc = now.AddMonths(-3),
            EndDateUtc = now.AddMonths(1)
        };

        context.Projects.AddRange(projectMicroservices, projectPaymentGateway, projectSeoPlatform);
        await context.SaveChangesAsync();

        // 3. Milestones
        var ms1 = new Milestone
        {
            ProjectId = projectMicroservices.Id,
            Title = "Architecture Blueprint & Schema Design",
            Description = "Event storming documents, Protobuf definitions, and Docker local dev cluster.",
            DeadlineUtc = now.AddMonths(-1).AddDays(15),
            Amount = 6000.00m,
            Status = MilestoneStatus.Paid
        };

        var ms2 = new Milestone
        {
            ProjectId = projectMicroservices.Id,
            Title = "Kafka Event Producer & Consumer Services",
            Description = "Resilient Outbox pattern implementation with EF Core and PostgreSQL.",
            DeadlineUtc = now.AddDays(-5),
            Amount = 9000.00m,
            Status = MilestoneStatus.Completed
        };

        var ms3 = new Milestone
        {
            ProjectId = projectMicroservices.Id,
            Title = "End-to-End Stress & Chaos Testing",
            Description = "k6 load testing scenarios and Kubernetes automated failover verification.",
            DeadlineUtc = now.AddMonths(1),
            Amount = 10000.00m,
            Status = MilestoneStatus.Pending
        };

        var ms4 = new Milestone
        {
            ProjectId = projectPaymentGateway.Id,
            Title = "Bank Open API Authentication & Consent Flow",
            Description = "OAuth2 / mTLS protocol handshake and token lifecycle management.",
            DeadlineUtc = now.AddDays(-10),
            Amount = 8000.00m,
            Status = MilestoneStatus.Completed
        };

        context.Milestones.AddRange(ms1, ms2, ms3, ms4);
        await context.SaveChangesAsync();

        // 4. Expenses
        var exp1 = new Expense
        {
            ProjectId = projectMicroservices.Id,
            Title = "JetBrains Rider Enterprise Subscription",
            Vendor = "JetBrains s.r.o.",
            Category = ExpenseCategory.Software,
            DateUtc = now.AddDays(-40),
            GrossAmount = 249.00m,
            TaxAmount = 49.80m,
            Currency = "USD",
            ExchangeRateToBase = 1.000000m,
            GrossAmountInBaseCurrency = 249.00m,
            PaymentMethod = PaymentMethod.CreditCard,
            IsBillable = false,
            ReimbursedStatus = ExpenseReimbursedStatus.NotBillable
        };

        var exp2 = new Expense
        {
            ProjectId = projectMicroservices.Id,
            Title = "Confluent Cloud Kafka Cluster Staging",
            Vendor = "Confluent Inc.",
            Category = ExpenseCategory.Software,
            DateUtc = now.AddDays(-20),
            GrossAmount = 350.00m,
            TaxAmount = 0.00m,
            Currency = "USD",
            ExchangeRateToBase = 1.000000m,
            GrossAmountInBaseCurrency = 350.00m,
            PaymentMethod = PaymentMethod.CreditCard,
            IsBillable = true,
            ReimbursedStatus = ExpenseReimbursedStatus.Unbilled
        };

        var exp3 = new Expense
        {
            ProjectId = projectPaymentGateway.Id,
            Title = "EU Payment Compliance Seminar & Flight",
            Vendor = "Lufthansa & EuroBanking Forum",
            Category = ExpenseCategory.Travel,
            DateUtc = now.AddDays(-15),
            GrossAmount = 520.00m,
            TaxAmount = 98.80m,
            Currency = "EUR",
            ExchangeRateToBase = 1.085000m,
            GrossAmountInBaseCurrency = 564.20m,
            PaymentMethod = PaymentMethod.CreditCard,
            IsBillable = true,
            ReimbursedStatus = ExpenseReimbursedStatus.Unbilled
        };

        context.Expenses.AddRange(exp1, exp2, exp3);
        await context.SaveChangesAsync();

        // 5. Invoices
        // Invoice 1: Paid invoice for Milestone 1 (USD)
        var inv1 = new Invoice
        {
            InvoiceNumber = "INV-202607-0001",
            ClientId = clientAcme.Id,
            ProjectId = projectMicroservices.Id,
            IssueDateUtc = now.AddDays(-45),
            DueDateUtc = now.AddDays(-15),
            TransactionCurrency = "USD",
            BaseCurrency = "USD",
            ExchangeRateToBase = 1.000000m,
            TaxRatePercentage = 0m,
            DiscountAmount = 0m,
            Status = InvoiceStatus.Paid,
            Notes = "Initial milestone deliverable invoice.",
            PaymentTerms = "Net 30 days"
        };

        var item1 = new InvoiceItem
        {
            InvoiceId = inv1.Id,
            Description = "Milestone 1: Architecture Blueprint & Schema Design",
            Quantity = 1m,
            UnitPrice = 6000.00m,
            ItemType = InvoiceItemType.Milestone,
            MilestoneId = ms1.Id
        };
        item1.RecalculateTotal();
        inv1.Items.Add(item1);
        inv1.CalculateTotals();
        inv1.RecordPayment(6000.00m); // Paid in full
        ms1.InvoiceItemId = item1.Id;
        ms1.InvoicedAtUtc = now.AddDays(-45);

        // Invoice 2: Issued (Overdue) Invoice in EUR for Nordic FinTech
        var inv2 = new Invoice
        {
            InvoiceNumber = "INV-202608-0002",
            ClientId = clientNordic.Id,
            ProjectId = projectPaymentGateway.Id,
            IssueDateUtc = now.AddDays(-35),
            DueDateUtc = now.AddDays(-5), // 5 days overdue
            TransactionCurrency = "EUR",
            BaseCurrency = "USD",
            ExchangeRateToBase = 1.085000m,
            TaxRatePercentage = 25.00m, // Swedish VAT
            DiscountAmount = 200.00m,
            Status = InvoiceStatus.Overdue,
            Notes = "Payment Gateway Stage 1 Integration.",
            PaymentTerms = "Net 30 days"
        };

        var item2 = new InvoiceItem
        {
            InvoiceId = inv2.Id,
            Description = "Milestone Deliverable: Open Banking Handshake",
            Quantity = 1m,
            UnitPrice = 8000.00m,
            ItemType = InvoiceItemType.Milestone,
            MilestoneId = ms4.Id
        };
        item2.RecalculateTotal();
        inv2.Items.Add(item2);
        inv2.CalculateTotals();
        ms4.InvoiceItemId = item2.Id;
        ms4.InvoicedAtUtc = now.AddDays(-35);

        // Invoice 3: Partially Paid Invoice in GBP for London Media
        var inv3 = new Invoice
        {
            InvoiceNumber = "INV-202608-0003",
            ClientId = clientLondon.Id,
            ProjectId = projectSeoPlatform.Id,
            IssueDateUtc = now.AddDays(-20),
            DueDateUtc = now.AddDays(10),
            TransactionCurrency = "GBP",
            BaseCurrency = "USD",
            ExchangeRateToBase = 1.295000m,
            TaxRatePercentage = 20.00m, // UK VAT
            DiscountAmount = 0m,
            Status = InvoiceStatus.PartiallyPaid,
            Notes = "Sprint 1 & 2 Consulting and Frontend CMS Setup.",
            PaymentTerms = "Net 30 days"
        };

        var item3a = new InvoiceItem
        {
            InvoiceId = inv3.Id,
            Description = "Senior Technical Advisory & Architecture - 30 hrs @ 100 GBP",
            Quantity = 30m,
            UnitPrice = 100.00m,
            ItemType = InvoiceItemType.Hourly
        };
        item3a.RecalculateTotal();

        var item3b = new InvoiceItem
        {
            InvoiceId = inv3.Id,
            Description = "GraphQL Schema Design & CDN Setup",
            Quantity = 1m,
            UnitPrice = 1500.00m,
            ItemType = InvoiceItemType.Custom
        };
        item3b.RecalculateTotal();

        inv3.Items.Add(item3a);
        inv3.Items.Add(item3b);
        inv3.CalculateTotals();
        inv3.RecordPayment(2500.00m); // Partial payment

        context.Invoices.AddRange(inv1, inv2, inv3);
        await context.SaveChangesAsync();

        // 6. Recurring Profiles (Retainers)
        var rpAcme = new RecurringProfile
        {
            ClientId = clientAcme.Id,
            ProjectId = projectMicroservices.Id,
            Title = "Monthly DevOps & Cloud Maintenance Retainer",
            Description = "Ongoing cluster observability, security patch updates, and Kafka performance tuning.",
            Schedule = BillingSchedule.Monthly,
            StartDateUtc = now.AddMonths(-1),
            EndDateUtc = now.AddMonths(11),
            NextRunDateUtc = now.AddDays(1), // Due tomorrow
            RetainerAmount = 2500.00m,
            Currency = "USD",
            PaymentDueDays = 14,
            IsActive = true
        };

        var rpIndus = new RecurringProfile
        {
            ClientId = clientIndus.Id,
            Title = "Weekly Strategic Tech Advisory Retainer",
            Description = "Portfolio company code audits and senior hiring evaluations.",
            Schedule = BillingSchedule.Weekly,
            StartDateUtc = now.AddDays(-14),
            EndDateUtc = now.AddMonths(6),
            NextRunDateUtc = now.AddDays(-1), // Already due! Background worker will pick this up.
            RetainerAmount = 150000.00m,
            Currency = "PKR",
            PaymentDueDays = 7,
            IsActive = true
        };

        context.RecurringProfiles.AddRange(rpAcme, rpIndus);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded successfully with clients, projects, multi-currency invoices, and recurring profiles!");
    }
}
