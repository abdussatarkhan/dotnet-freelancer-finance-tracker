using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.RecurringProfiles.DTOs;
using FreelancerTrack.Domain.Entities;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.RecurringProfiles.Commands.CreateRecurringProfile;

public record CreateRecurringProfileCommand(
    Guid ClientId,
    Guid? ProjectId,
    string Title,
    string? Description,
    BillingSchedule Schedule,
    DateTimeOffset StartDateUtc,
    DateTimeOffset? EndDateUtc,
    decimal RetainerAmount,
    string Currency,
    int PaymentDueDays = 14) : IRequest<Result<RecurringProfileDto>>;

public class CreateRecurringProfileCommandValidator : AbstractValidator<CreateRecurringProfileCommand>
{
    public CreateRecurringProfileCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Schedule).IsInEnum();
        RuleFor(x => x.StartDateUtc).NotEmpty();
        RuleFor(x => x.RetainerAmount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.PaymentDueDays).InclusiveBetween(1, 90);
    }
}

public class CreateRecurringProfileCommandHandler : IRequestHandler<CreateRecurringProfileCommand, Result<RecurringProfileDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateRecurringProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RecurringProfileDto>> Handle(CreateRecurringProfileCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);
        if (client == null)
        {
            return Result<RecurringProfileDto>.Failure($"Client with ID '{request.ClientId}' was not found.", "CLIENT_NOT_FOUND");
        }

        var profile = new RecurringProfile
        {
            ClientId = request.ClientId,
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            Schedule = request.Schedule,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc,
            NextRunDateUtc = request.StartDateUtc,
            RetainerAmount = request.RetainerAmount,
            Currency = request.Currency.ToUpperInvariant(),
            PaymentDueDays = request.PaymentDueDays,
            IsActive = true
        };

        _context.RecurringProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new RecurringProfileDto(
            profile.Id,
            profile.ClientId,
            client.Name,
            profile.ProjectId,
            null,
            profile.Title,
            profile.Description,
            profile.Schedule,
            profile.StartDateUtc,
            profile.EndDateUtc,
            profile.NextRunDateUtc,
            profile.LastRunDateUtc,
            profile.IsActive,
            profile.RetainerAmount,
            profile.Currency,
            profile.PaymentDueDays,
            profile.CreatedAtUtc);

        return Result<RecurringProfileDto>.Success(dto);
    }
}
