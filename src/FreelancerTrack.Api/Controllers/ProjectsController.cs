using FreelancerTrack.Application.Features.Projects.Commands.CreateProject;
using FreelancerTrack.Application.Features.Projects.DTOs;
using FreelancerTrack.Application.Features.Projects.Queries.GetProjectById;
using FreelancerTrack.Application.Features.Projects.Queries.GetProjects;
using FreelancerTrack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerTrack.Api.Controllers;

public class ProjectsController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.IsSuccess ? result.Value.Id : Guid.Empty });
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? clientId = null, [FromQuery] ProjectStatus? status = null)
    {
        var result = await Mediator.Send(new GetProjectsQuery(clientId, status));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetProjectByIdQuery(id));
        return HandleResult(result);
    }
}
