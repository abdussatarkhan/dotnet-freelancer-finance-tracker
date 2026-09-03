using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.RecurringProfiles.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.RecurringProfiles.Queries.GetRecurringProfiles;

public record GetRecurringProfilesQuery(Guid? ClientId = null, bool? OnlyActive = null) : IRequest<Result<IReadOnlyList<RecurringProfileDto>>>;

public class GetRecurringProfilesQueryHandler : IRequestHandler<GetRecurringProfilesQuery, Result<IReadOnlyList<RecurringProfileDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetRecurringProfilesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<RecurringProfileDto>>> Handle(GetRecurringProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.RecurringProfiles
            .Include(rp => rp.Client)
            .Include(rp => rp.Project)
            .AsNoTracking();

        if (request.ClientId.HasValue)
        {
            query = query.Where(rp => rp.ClientId == request.ClientId.Value);
        }

        if (request.OnlyActive.HasValue)
        {
            query = query.Where(rp => rp.IsActive == request.OnlyActive.Value);
        }

        var profiles = await query
            .OrderBy(rp => rp.NextRunDateUtc)
            .Select(rp => new RecurringProfileDto(
                rp.Id,
                rp.ClientId,
                rp.Client.Name,
                rp.ProjectId,
                rp.Project != null ? rp.Project.Name : null,
                rp.Title,
                rp.Description,
                rp.Schedule,
                rp.StartDateUtc,
                rp.EndDateUtc,
                rp.NextRunDateUtc,
                rp.LastRunDateUtc,
                rp.IsActive,
                rp.RetainerAmount,
                rp.Currency,
                rp.PaymentDueDays,
                rp.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<RecurringProfileDto>>.Success(profiles);
    }
}
