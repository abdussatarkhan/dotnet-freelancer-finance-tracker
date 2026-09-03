namespace FreelancerTrack.Application.Features.Clients.DTOs;

public record ClientDto(
    Guid Id,
    string Name,
    string Email,
    string? TaxOrVatNumber,
    string BillingStreet,
    string BillingCity,
    string BillingState,
    string BillingPostalCode,
    string BillingCountry,
    string DefaultCurrency,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
