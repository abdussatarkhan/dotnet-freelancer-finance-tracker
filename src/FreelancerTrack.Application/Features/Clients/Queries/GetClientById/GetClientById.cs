using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Clients.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Clients.Queries.GetClientById;

public record GetClientByIdQuery(Guid Id) : IRequest<Result<ClientDto>>;

public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, Result<ClientDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClientByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClientDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (client == null)
        {
            return Result<ClientDto>.Failure($"Client with ID '{request.Id}' was not found.", "CLIENT_NOT_FOUND");
        }

        var dto = new ClientDto(
            client.Id,
            client.Name,
            client.Email,
            client.TaxOrVatNumber,
            client.BillingStreet,
            client.BillingCity,
            client.BillingState,
            client.BillingPostalCode,
            client.BillingCountry,
            client.DefaultCurrency,
            client.IsActive,
            client.CreatedAtUtc);

        return Result<ClientDto>.Success(dto);
    }
}
