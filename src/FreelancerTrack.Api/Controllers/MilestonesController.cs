using FreelancerTrack.Application.Features.Milestones.Commands.CreateMilestone;
using FreelancerTrack.Application.Features.Milestones.Commands.UpdateMilestoneStatus;
using FreelancerTrack.Application.Features.Milestones.DTOs;
using FreelancerTrack.Application.Features.Milestones.Queries.GetMilestones;
using FreelancerTrack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerTrack.Api.Controllers;

public class MilestonesController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(MilestoneDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMilestoneCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MilestoneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProject([FromQuery] Guid projectId, [FromQuery] MilestoneStatus? status = null)
    {
        var result = await Mediator.Send(new GetMilestonesQuery(projectId, status));
        return HandleResult(result);
    }

    public record UpdateStatusRequest(MilestoneStatus Status);

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(MilestoneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var result = await Mediator.Send(new UpdateMilestoneStatusCommand(id, request.Status));
        return HandleResult(result);
    }
}
