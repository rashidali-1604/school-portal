using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolPortal.Application.Abstractions;
using SchoolPortal.Infrastructure.Persistence;
using SchoolPortal.Infrastructure.Persistence.Repositories;
using SchoolPortal.Infrastructure.Time;

namespace SchoolPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Persistence:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=schoolportal.db";

        services.AddDbContext<PortalDbContext>(options =>
        {
            if (string.Equals(provider, "SqlServer", System.StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
