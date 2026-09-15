using Microsoft.Extensions.DependencyInjection;
using SchoolPortal.Application.Services;

namespace SchoolPortal.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            return services;
        }
    }
}
