using FreelancerTrack.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.ErrorCode != null && result.ErrorCode.Contains("NOT_FOUND", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = result.Error,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }

        if (result.ErrorCode != null && result.ErrorCode.Contains("CONFLICT", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = result.Error,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }

        return BadRequest(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = result.Error,
            Extensions = { ["errorCode"] = result.ErrorCode }
        });
    }

    protected ActionResult HandleCreatedResult<T>(Result<T> result, string actionName, object routeValues)
    {
        if (result.IsSuccess)
        {
            return CreatedAtAction(actionName, routeValues, result.Value);
        }

        return HandleResult(result);
    }
}
