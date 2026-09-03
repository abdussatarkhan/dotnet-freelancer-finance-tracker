using FreelancerTrack.Application.Common.Interfaces;

namespace FreelancerTrack.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
