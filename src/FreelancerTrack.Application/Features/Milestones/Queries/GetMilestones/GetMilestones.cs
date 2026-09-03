using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Milestones.DTOs;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Milestones.Queries.GetMilestones;

public record GetMilestonesQuery(Guid ProjectId, MilestoneStatus? Status = null) : IRequest<Result<IReadOnlyList<MilestoneDto>>>;

public class GetMilestonesQueryHandler : IRequestHandler<GetMilestonesQuery, Result<IReadOnlyList<MilestoneDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMilestonesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<MilestoneDto>>> Handle(GetMilestonesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Milestones
            .Include(m => m.Project)
            .Where(m => m.ProjectId == request.ProjectId)
            .AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(m => m.Status == request.Status.Value);
        }

        var milestones = await query
            .OrderBy(m => m.DeadlineUtc)
            .Select(m => new MilestoneDto(
                m.Id,
                m.ProjectId,
                m.Project.Name,
                m.Title,
                m.Description,
                m.DeadlineUtc,
                m.Amount,
                m.Status,
                m.InvoiceItemId,
                m.InvoicedAtUtc,
                m.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MilestoneDto>>.Success(milestones);
    }
}
