using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Projects.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(Guid Id) : IRequest<Result<ProjectDto>>;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Client)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
        {
            return Result<ProjectDto>.Failure($"Project with ID '{request.Id}' was not found.", "PROJECT_NOT_FOUND");
        }

        var dto = new ProjectDto(
            project.Id,
            project.ClientId,
            project.Client.Name,
            project.Name,
            project.Description,
            project.Currency,
            project.TotalBudget,
            project.Status,
            project.StartDateUtc,
            project.EndDateUtc,
            project.CreatedAtUtc);

        return Result<ProjectDto>.Success(dto);
    }
}
