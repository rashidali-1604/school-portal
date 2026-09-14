using Microsoft.Extensions.DependencyInjection;
using SchoolPortal.Application.Users.Commands;
using SchoolPortal.Application.Users.Queries;

namespace SchoolPortal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SearchUsersHandler>();
        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<AdjustWalletHandler>();
        return services;
    }
}
