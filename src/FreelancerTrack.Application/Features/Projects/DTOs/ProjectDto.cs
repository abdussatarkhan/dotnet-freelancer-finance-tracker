using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Application.Features.Projects.DTOs;

public record ProjectDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string Name,
    string? Description,
    string Currency,
    decimal TotalBudget,
    ProjectStatus Status,
    DateTimeOffset StartDateUtc,
    DateTimeOffset? EndDateUtc,
    DateTimeOffset CreatedAtUtc);
