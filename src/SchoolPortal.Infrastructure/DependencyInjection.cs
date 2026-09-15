using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolPortal.Application.Interfaces;
using SchoolPortal.Application.Repositories;
using SchoolPortal.Infrastructure.Context;
using SchoolPortal.Infrastructure.Repositories;
using SchoolPortal.Infrastructure.Services;

namespace SchoolPortal.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration["Persistence:Provider"] ?? "Sqlite";
            var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=schoolportal.db";

            services.AddDbContext<AppDbContext>(options =>
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

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddSingleton<IClock, SystemClock>();

            return services;
        }
    }
}
