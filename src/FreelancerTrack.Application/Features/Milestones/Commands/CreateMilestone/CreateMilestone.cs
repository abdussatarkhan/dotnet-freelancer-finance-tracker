using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Milestones.DTOs;
using FreelancerTrack.Domain.Entities;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Milestones.Commands.CreateMilestone;

public record CreateMilestoneCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    DateTimeOffset DeadlineUtc,
    decimal Amount) : IRequest<Result<MilestoneDto>>;

public class CreateMilestoneCommandValidator : AbstractValidator<CreateMilestoneCommand>
{
    public CreateMilestoneCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DeadlineUtc).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class CreateMilestoneCommandHandler : IRequestHandler<CreateMilestoneCommand, Result<MilestoneDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateMilestoneCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MilestoneDto>> Handle(CreateMilestoneCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            return Result<MilestoneDto>.Failure($"Project with ID '{request.ProjectId}' was not found.", "PROJECT_NOT_FOUND");
        }

        var milestone = new Milestone
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            DeadlineUtc = request.DeadlineUtc,
            Amount = request.Amount,
            Status = MilestoneStatus.Pending
        };

        _context.Milestones.Add(milestone);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MilestoneDto(
            milestone.Id,
            milestone.ProjectId,
            project.Name,
            milestone.Title,
            milestone.Description,
            milestone.DeadlineUtc,
            milestone.Amount,
            milestone.Status,
            milestone.InvoiceItemId,
            milestone.InvoicedAtUtc,
            milestone.CreatedAtUtc);

        return Result<MilestoneDto>.Success(dto);
    }
}
