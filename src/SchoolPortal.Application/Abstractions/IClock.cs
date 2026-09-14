using System;

namespace SchoolPortal.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
