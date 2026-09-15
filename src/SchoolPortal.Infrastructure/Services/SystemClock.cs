using System;
using SchoolPortal.Application.Interfaces;

namespace SchoolPortal.Infrastructure.Services
{
    public class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
