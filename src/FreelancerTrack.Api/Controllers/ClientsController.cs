using FreelancerTrack.Application.Features.Clients.Commands.CreateClient;
using FreelancerTrack.Application.Features.Clients.DTOs;
using FreelancerTrack.Application.Features.Clients.Queries.GetClientById;
using FreelancerTrack.Application.Features.Clients.Queries.GetClients;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerTrack.Api.Controllers;

public class ClientsController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.IsSuccess ? result.Value.Id : Guid.Empty });
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? onlyActive = null)
    {
        var result = await Mediator.Send(new GetClientsQuery(onlyActive));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetClientByIdQuery(id));
        return HandleResult(result);
    }
}
