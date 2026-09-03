using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Expenses.DTOs;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Expenses.Queries.GetExpenses;

public record GetExpensesQuery(
    Guid? ProjectId = null,
    ExpenseCategory? Category = null,
    bool? IsBillable = null,
    ExpenseReimbursedStatus? ReimbursedStatus = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null) : IRequest<Result<IReadOnlyList<ExpenseDto>>>;

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, Result<IReadOnlyList<ExpenseDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetExpensesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ExpenseDto>>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Expenses
            .Include(e => e.Project)
            .Include(e => e.Receipts)
            .AsNoTracking();

        if (request.ProjectId.HasValue)
        {
            query = query.Where(e => e.ProjectId == request.ProjectId.Value);
        }

        if (request.Category.HasValue)
        {
            query = query.Where(e => e.Category == request.Category.Value);
        }

        if (request.IsBillable.HasValue)
        {
            query = query.Where(e => e.IsBillable == request.IsBillable.Value);
        }

        if (request.ReimbursedStatus.HasValue)
        {
            query = query.Where(e => e.ReimbursedStatus == request.ReimbursedStatus.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(e => e.DateUtc >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(e => e.DateUtc <= request.ToDate.Value);
        }

        var expenses = await query
            .OrderByDescending(e => e.DateUtc)
            .Select(e => new ExpenseDto(
                e.Id,
                e.ProjectId,
                e.Project != null ? e.Project.Name : null,
                e.Title,
                e.Vendor,
                e.Category,
                e.DateUtc,
                e.GrossAmount,
                e.TaxAmount,
                e.Currency,
                e.ExchangeRateToBase,
                e.GrossAmountInBaseCurrency,
                e.PaymentMethod,
                e.IsBillable,
                e.ReimbursedStatus,
                e.InvoiceItemId,
                e.InvoicedAtUtc,
                e.Receipts.Select(r => new ReceiptAttachmentDto(
                    r.Id,
                    r.ExpenseId,
                    r.OriginalFileName,
                    r.MimeType,
                    r.FileSizeBytes,
                    r.StoragePath,
                    r.Sha256Hash,
                    r.UploadedAtUtc)).ToList(),
                e.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ExpenseDto>>.Success(expenses);
    }
}
