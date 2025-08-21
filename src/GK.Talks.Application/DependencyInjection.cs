using GK.Talks.Application.Behaviours;
using GK.Talks.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GK.Talks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped<ISpeakerRegistrationService, SpeakerRegistrationService>();
        return services;
    }
}
