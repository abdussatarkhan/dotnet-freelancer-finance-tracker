using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Features.Expenses.Commands.AttachReceipt;
using FreelancerTrack.Application.Features.Expenses.Commands.LogExpense;
using FreelancerTrack.Application.Features.Expenses.DTOs;
using FreelancerTrack.Application.Features.Expenses.Queries.GetExpenseById;
using FreelancerTrack.Application.Features.Expenses.Queries.GetExpenses;
using FreelancerTrack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Api.Controllers;

public class ExpensesController : ApiControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationDbContext _context;

    public ExpensesController(IFileStorageService fileStorageService, IApplicationDbContext context)
    {
        _fileStorageService = fileStorageService;
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogExpense([FromBody] LogExpenseCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.IsSuccess ? result.Value.Id : Guid.Empty });
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExpenseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? projectId = null,
        [FromQuery] ExpenseCategory? category = null,
        [FromQuery] bool? isBillable = null,
        [FromQuery] ExpenseReimbursedStatus? reimbursedStatus = null,
        [FromQuery] DateTimeOffset? fromDate = null,
        [FromQuery] DateTimeOffset? toDate = null)
    {
        var result = await Mediator.Send(new GetExpensesQuery(projectId, category, isBillable, reimbursedStatus, fromDate, toDate));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetExpenseByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/receipts")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ReceiptAttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadReceipt(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid File",
                Detail = "No file was uploaded or the file was empty."
            });
        }

        await using var stream = file.OpenReadStream();
        var command = new AttachReceiptCommand(id, stream, file.FileName, file.ContentType);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpGet("{id:guid}/receipts/{receiptId:guid}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadReceipt(Guid id, Guid receiptId)
    {
        var attachment = await _context.ReceiptAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(ra => ra.Id == receiptId && ra.ExpenseId == id);

        if (attachment == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Receipt Not Found",
                Detail = $"Receipt attachment '{receiptId}' was not found for expense '{id}'."
            });
        }

        var fileResult = await _fileStorageService.GetFileAsync(attachment.StoragePath);
        if (fileResult == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "File Not Found on Disk",
                Detail = "The receipt physical file could not be located on storage."
            });
        }

        return File(fileResult.Value.ContentStream, attachment.MimeType, attachment.OriginalFileName);
    }
}
