using System;
using System.Linq;
using System.Threading.Tasks;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Infrastructure.Context
{
    public static class AppDbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;

            var seeds = new[]
            {
                User.Register("Amelia", "Nguyen", "amelia.nguyen@example.com", UserRole.Parent, now, 125.50m),
                User.Register("Rahul", "Menon", "rahul.menon@example.com", UserRole.Parent, now, 40.00m),
                User.Register("Sofia", "Tanaka", "sofia.tanaka@example.com", UserRole.Parent, now, 0.00m),
                User.Register("Liam", "OConnor", "liam.oconnor@example.com", UserRole.Parent, now, 78.20m),
                User.Register("Priya", "Kapoor", "priya.kapoor@example.com", UserRole.Staff, now, 0.00m),
                User.Register("Ethan", "Wong", "ethan.wong@example.com", UserRole.Student, now, 15.00m),
                User.Register("Isabella", "Rivera", "isabella.rivera@example.com", UserRole.Parent, now, 220.00m),
                User.Register("Noah", "Fernandez", "noah.fernandez@example.com", UserRole.Administrator, now, 0.00m)
            };

            context.Users.AddRange(seeds);
            await context.SaveChangesAsync();
        }
    }
}
