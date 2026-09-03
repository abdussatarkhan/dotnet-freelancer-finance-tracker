using FreelancerTrack.Application.Features.Invoices.Commands.CreateDraftInvoice;
using FreelancerTrack.Application.Features.Invoices.Commands.IssueInvoice;
using FreelancerTrack.Application.Features.Invoices.Commands.RecordPayment;
using FreelancerTrack.Application.Features.Invoices.Commands.VoidInvoice;
using FreelancerTrack.Application.Features.Invoices.DTOs;
using FreelancerTrack.Application.Features.Invoices.Queries.GetInvoiceById;
using FreelancerTrack.Application.Features.Invoices.Queries.GetInvoices;
using FreelancerTrack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerTrack.Api.Controllers;

public class InvoicesController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDraft([FromBody] CreateDraftInvoiceCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.IsSuccess ? result.Value.Id : Guid.Empty });
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? clientId = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] DateTimeOffset? fromDate = null,
        [FromQuery] DateTimeOffset? toDate = null)
    {
        var result = await Mediator.Send(new GetInvoicesQuery(clientId, projectId, status, fromDate, toDate));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetInvoiceByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/issue")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Issue(Guid id)
    {
        var result = await Mediator.Send(new IssueInvoiceCommand(id));
        return HandleResult(result);
    }

    public record RecordPaymentRequest(decimal Amount);

    [HttpPost("{id:guid}/payments")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordPaymentRequest request)
    {
        var result = await Mediator.Send(new RecordInvoicePaymentCommand(id, request.Amount));
        return HandleResult(result);
    }

    public record VoidInvoiceRequest(string Reason);

    [HttpPost("{id:guid}/void")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidInvoiceRequest request)
    {
        var result = await Mediator.Send(new VoidInvoiceCommand(id, request.Reason));
        return HandleResult(result);
    }
}
