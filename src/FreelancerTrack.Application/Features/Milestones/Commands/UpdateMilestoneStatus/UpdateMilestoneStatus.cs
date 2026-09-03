using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Milestones.DTOs;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Milestones.Commands.UpdateMilestoneStatus;

public record UpdateMilestoneStatusCommand(Guid MilestoneId, MilestoneStatus NewStatus) : IRequest<Result<MilestoneDto>>;

public class UpdateMilestoneStatusCommandValidator : AbstractValidator<UpdateMilestoneStatusCommand>
{
    public UpdateMilestoneStatusCommandValidator()
    {
        RuleFor(x => x.MilestoneId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

public class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, Result<MilestoneDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateMilestoneStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MilestoneDto>> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

        if (milestone == null)
        {
            return Result<MilestoneDto>.Failure($"Milestone with ID '{request.MilestoneId}' was not found.", "MILESTONE_NOT_FOUND");
        }

        switch (request.NewStatus)
        {
            case MilestoneStatus.Completed:
                milestone.MarkCompleted();
                break;
            case MilestoneStatus.InProgress:
                milestone.Status = MilestoneStatus.InProgress;
                break;
            case MilestoneStatus.Pending:
                milestone.Status = MilestoneStatus.Pending;
                break;
            default:
                milestone.Status = request.NewStatus;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MilestoneDto(
            milestone.Id,
            milestone.ProjectId,
            milestone.Project.Name,
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
