using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Expenses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Expenses.Queries.GetExpenseById;

public record GetExpenseByIdQuery(Guid Id) : IRequest<Result<ExpenseDto>>;

public class GetExpenseByIdQueryHandler : IRequestHandler<GetExpenseByIdQuery, Result<ExpenseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExpenseByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExpenseDto>> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses
            .Include(e => e.Project)
            .Include(e => e.Receipts)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (expense == null)
        {
            return Result<ExpenseDto>.Failure($"Expense with ID '{request.Id}' was not found.", "EXPENSE_NOT_FOUND");
        }

        var dto = new ExpenseDto(
            expense.Id,
            expense.ProjectId,
            expense.Project?.Name,
            expense.Title,
            expense.Vendor,
            expense.Category,
            expense.DateUtc,
            expense.GrossAmount,
            expense.TaxAmount,
            expense.Currency,
            expense.ExchangeRateToBase,
            expense.GrossAmountInBaseCurrency,
            expense.PaymentMethod,
            expense.IsBillable,
            expense.ReimbursedStatus,
            expense.InvoiceItemId,
            expense.InvoicedAtUtc,
            expense.Receipts.Select(r => new ReceiptAttachmentDto(
                r.Id,
                r.ExpenseId,
                r.OriginalFileName,
                r.MimeType,
                r.FileSizeBytes,
                r.StoragePath,
                r.Sha256Hash,
                r.UploadedAtUtc)).ToList(),
            expense.CreatedAtUtc);

        return Result<ExpenseDto>.Success(dto);
    }
}
