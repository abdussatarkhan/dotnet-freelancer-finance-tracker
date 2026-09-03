using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Projects.DTOs;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Projects.Queries.GetProjects;

public record GetProjectsQuery(Guid? ClientId = null, ProjectStatus? Status = null) : IRequest<Result<IReadOnlyList<ProjectDto>>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, Result<IReadOnlyList<ProjectDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ProjectDto>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Projects.Include(p => p.Client).AsNoTracking();

        if (request.ClientId.HasValue)
        {
            query = query.Where(p => p.ClientId == request.ClientId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        var projects = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new ProjectDto(
                p.Id,
                p.ClientId,
                p.Client.Name,
                p.Name,
                p.Description,
                p.Currency,
                p.TotalBudget,
                p.Status,
                p.StartDateUtc,
                p.EndDateUtc,
                p.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ProjectDto>>.Success(projects);
    }
}
