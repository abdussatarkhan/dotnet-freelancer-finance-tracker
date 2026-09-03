using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Application.Features.RecurringProfiles.DTOs;

public record RecurringProfileDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid? ProjectId,
    string? ProjectName,
    string Title,
    string? Description,
    BillingSchedule Schedule,
    DateTimeOffset StartDateUtc,
    DateTimeOffset? EndDateUtc,
    DateTimeOffset NextRunDateUtc,
    DateTimeOffset? LastRunDateUtc,
    bool IsActive,
    decimal RetainerAmount,
    string Currency,
    int PaymentDueDays,
    DateTimeOffset CreatedAtUtc);
