using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Clients.DTOs;
using FreelancerTrack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Clients.Commands.CreateClient;

public record CreateClientCommand(
    string Name,
    string Email,
    string? TaxOrVatNumber,
    string BillingStreet,
    string BillingCity,
    string BillingState,
    string BillingPostalCode,
    string BillingCountry,
    string DefaultCurrency) : IRequest<Result<ClientDto>>;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.BillingStreet).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BillingCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BillingState).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BillingPostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BillingCountry).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DefaultCurrency).NotEmpty().Length(3);
    }
}

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<ClientDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateClientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClientDto>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        bool emailExists = await _context.Clients.AnyAsync(c => c.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            return Result<ClientDto>.Failure($"A client with email '{request.Email}' already exists.", "CLIENT_EMAIL_EXISTS");
        }

        var client = new Client
        {
            Name = request.Name,
            Email = request.Email,
            TaxOrVatNumber = request.TaxOrVatNumber,
            BillingStreet = request.BillingStreet,
            BillingCity = request.BillingCity,
            BillingState = request.BillingState,
            BillingPostalCode = request.BillingPostalCode,
            BillingCountry = request.BillingCountry,
            DefaultCurrency = request.DefaultCurrency.ToUpperInvariant(),
            IsActive = true
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);

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
