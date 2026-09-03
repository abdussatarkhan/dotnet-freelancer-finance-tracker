namespace FreelancerTrack.Application.Features.Reports.DTOs;

public record NetCashFlowReportDto(
    string BaseCurrency,
    decimal TotalPaidInvoicedRevenue,
    decimal TotalExpenses,
    decimal NetCashFlow,
    int TotalPaidInvoicesCount,
    int TotalExpensesCount);

public record AccountsReceivableAgingDto(
    string BaseCurrency,
    decimal CurrentNotOverdue,
    decimal Overdue1To30Days,
    decimal Overdue31To60Days,
    decimal Overdue61To90Days,
    decimal OverdueOver90Days,
    decimal TotalOutstandingReceivables);

public record ProjectProfitabilityItemDto(
    Guid ProjectId,
    string ProjectName,
    string ClientName,
    decimal TotalInvoicedRevenueInBase,
    decimal TotalExpensesInBase,
    decimal NetProfitInBase,
    decimal ProfitMarginPercentage);

public record ProjectProfitabilityReportDto(
    string BaseCurrency,
    IReadOnlyList<ProjectProfitabilityItemDto> Projects);
