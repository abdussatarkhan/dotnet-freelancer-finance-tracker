using FreelancerTrack.Application.Features.RecurringProfiles.Commands.CreateRecurringProfile;
using FreelancerTrack.Application.Features.RecurringProfiles.Commands.ProcessRecurringProfiles;
using FreelancerTrack.Application.Features.RecurringProfiles.DTOs;
using FreelancerTrack.Application.Features.RecurringProfiles.Queries.GetRecurringProfiles;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerTrack.Api.Controllers;

public class RecurringProfilesController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RecurringProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRecurringProfileCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecurringProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? clientId = null, [FromQuery] bool? onlyActive = null)
    {
        var result = await Mediator.Send(new GetRecurringProfilesQuery(clientId, onlyActive));
        return HandleResult(result);
    }

    [HttpPost("trigger-run")]
    [ProducesResponseType(typeof(ProcessRecurringProfilesResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> TriggerRun()
    {
        var result = await Mediator.Send(new ProcessRecurringProfilesCommand());
        return HandleResult(result);
    }
}
