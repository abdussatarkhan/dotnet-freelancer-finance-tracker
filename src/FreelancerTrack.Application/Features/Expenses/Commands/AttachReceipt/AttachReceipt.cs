using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Expenses.DTOs;
using FreelancerTrack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Expenses.Commands.AttachReceipt;

public record AttachReceiptCommand(
    Guid ExpenseId,
    Stream FileStream,
    string OriginalFileName,
    string MimeType) : IRequest<Result<ReceiptAttachmentDto>>;

public class AttachReceiptCommandValidator : AbstractValidator<AttachReceiptCommand>
{
    public AttachReceiptCommandValidator()
    {
        RuleFor(x => x.ExpenseId).NotEmpty();
        RuleFor(x => x.OriginalFileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.MimeType).NotEmpty().MaximumLength(100);
    }
}

public class AttachReceiptCommandHandler : IRequestHandler<AttachReceiptCommand, Result<ReceiptAttachmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public AttachReceiptCommandHandler(IApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<ReceiptAttachmentDto>> Handle(AttachReceiptCommand request, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == request.ExpenseId, cancellationToken);

        if (expense == null)
        {
            return Result<ReceiptAttachmentDto>.Failure($"Expense with ID '{request.ExpenseId}' was not found.", "EXPENSE_NOT_FOUND");
        }

        var (storedFileName, storagePath, hash, sizeBytes) = await _fileStorageService.SaveFileAsync(
            request.FileStream,
            request.OriginalFileName,
            request.MimeType,
            cancellationToken);

        var attachment = new ReceiptAttachment
        {
            ExpenseId = request.ExpenseId,
            StoredFileName = storedFileName,
            OriginalFileName = request.OriginalFileName,
            MimeType = request.MimeType,
            FileSizeBytes = sizeBytes,
            StoragePath = storagePath,
            Sha256Hash = hash,
            UploadedAtUtc = DateTimeOffset.UtcNow
        };

        _context.ReceiptAttachments.Add(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ReceiptAttachmentDto(
            attachment.Id,
            attachment.ExpenseId,
            attachment.OriginalFileName,
            attachment.MimeType,
            attachment.FileSizeBytes,
            attachment.StoragePath,
            attachment.Sha256Hash,
            attachment.UploadedAtUtc);

        return Result<ReceiptAttachmentDto>.Success(dto);
    }
}
