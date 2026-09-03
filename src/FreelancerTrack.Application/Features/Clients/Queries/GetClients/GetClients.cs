using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Clients.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Clients.Queries.GetClients;

public record GetClientsQuery(bool? OnlyActive = null) : IRequest<Result<IReadOnlyList<ClientDto>>>;

public class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, Result<IReadOnlyList<ClientDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetClientsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ClientDto>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Clients.AsNoTracking();

        if (request.OnlyActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.OnlyActive.Value);
        }

        var clients = await query
            .OrderBy(c => c.Name)
            .Select(c => new ClientDto(
                c.Id,
                c.Name,
                c.Email,
                c.TaxOrVatNumber,
                c.BillingStreet,
                c.BillingCity,
                c.BillingState,
                c.BillingPostalCode,
                c.BillingCountry,
                c.DefaultCurrency,
                c.IsActive,
                c.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ClientDto>>.Success(clients);
    }
}
