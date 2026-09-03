namespace FreelancerTrack.Domain.Enums;

public enum ProjectStatus
{
    Active = 0,
    Completed = 1,
    Archived = 2
}

public enum MilestoneStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Billed = 3,
    Paid = 4
}

public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Overdue = 4,
    Void = 5
}

public enum InvoiceItemType
{
    Milestone = 0,
    Hourly = 1,
    ExpenseRecharge = 2,
    Retainer = 3,
    Custom = 4
}

public enum BillingSchedule
{
    Weekly = 0,
    Monthly = 1,
    Quarterly = 2,
    Yearly = 3
}

public enum ExpenseCategory
{
    Software = 0,
    Hardware = 1,
    Travel = 2,
    Subcontractor = 3,
    Office = 4
}

public enum PaymentMethod
{
    CreditCard = 0,
    BankTransfer = 1,
    Cash = 2,
    PayPal = 3,
    Crypto = 4
}

public enum ExpenseReimbursedStatus
{
    NotBillable = 0,
    Unbilled = 1,
    Reimbursed = 2
}
