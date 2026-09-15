using System;

namespace SchoolPortal.Application.Interfaces
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
