using JobTracker.Application.JobApplications;
using JobTracker.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.AddScoped<JobApplicationService>();

        return services;
    }
}
