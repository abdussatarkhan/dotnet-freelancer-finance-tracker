using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Application.Features.Milestones.DTOs;

public record MilestoneDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Title,
    string? Description,
    DateTimeOffset DeadlineUtc,
    decimal Amount,
    MilestoneStatus Status,
    Guid? InvoiceItemId,
    DateTimeOffset? InvoicedAtUtc,
    DateTimeOffset CreatedAtUtc);
