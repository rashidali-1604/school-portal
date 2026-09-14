using System;
using SchoolPortal.Application.Abstractions;

namespace SchoolPortal.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
