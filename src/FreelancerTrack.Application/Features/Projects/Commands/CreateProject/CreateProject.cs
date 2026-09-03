using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Projects.DTOs;
using FreelancerTrack.Domain.Entities;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    Guid ClientId,
    string Name,
    string? Description,
    string Currency,
    decimal TotalBudget,
    DateTimeOffset StartDateUtc,
    DateTimeOffset? EndDateUtc) : IRequest<Result<ProjectDto>>;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.TotalBudget).GreaterThan(0);
        RuleFor(x => x.StartDateUtc).NotEmpty();
        RuleFor(x => x.EndDateUtc)
            .GreaterThan(x => x.StartDateUtc)
            .When(x => x.EndDateUtc.HasValue)
            .WithMessage("EndDate must be after StartDate.");
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);

        if (client == null)
        {
            return Result<ProjectDto>.Failure($"Client with ID '{request.ClientId}' was not found.", "CLIENT_NOT_FOUND");
        }

        var project = new Project
        {
            ClientId = request.ClientId,
            Name = request.Name,
            Description = request.Description,
            Currency = request.Currency.ToUpperInvariant(),
            TotalBudget = request.TotalBudget,
            Status = ProjectStatus.Active,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ProjectDto(
            project.Id,
            project.ClientId,
            client.Name,
            project.Name,
            project.Description,
            project.Currency,
            project.TotalBudget,
            project.Status,
            project.StartDateUtc,
            project.EndDateUtc,
            project.CreatedAtUtc);

        return Result<ProjectDto>.Success(dto);
    }
}
