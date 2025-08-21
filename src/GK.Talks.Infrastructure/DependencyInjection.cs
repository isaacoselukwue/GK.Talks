using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.Interfaces;
using GK.Talks.Infrastructure.Configuration;
using GK.Talks.Infrastructure.Data;
using GK.Talks.Infrastructure.Data.Interceptors;
using GK.Talks.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GK.Talks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RegistrationRulesOptions>(configuration.GetSection("RegistrationRules"));

        services.AddTransient<ISpeakerQualificationStrategy, ExperienceQualificationStrategy>();
        services.AddTransient<ISpeakerQualificationStrategy, HasBlogQualificationStrategy>();
        services.AddTransient<ISpeakerQualificationStrategy, CertificationsQualificationStrategy>();
        services.AddTransient<ISpeakerQualificationStrategy, EmployerQualificationStrategy>();
        services.AddTransient<ISpeakerQualificationStrategy, EmailAndBrowserQualificationStrategy>();

        services.AddSingleton<IRegistrationFeeCalculator, RegistrationFeeCalculator>();

        services.AddScoped<AuditingInterceptor>();

        services.AddSingleton(sp =>
        {
            SqliteConnection conn = new(configuration.GetConnectionString("Default"));
            conn.Open();
            return conn;
        });

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var sqliteConn = sp.GetRequiredService<SqliteConnection>();
            var interceptor = sp.GetRequiredService<AuditingInterceptor>();
            options.UseSqlite(sqliteConn);
            options.AddInterceptors(interceptor);
        });

        services.AddScoped<IRepository<Speaker>, SpeakerRepository>();

        services.AddSingleton<IBrowserDetector, BrowserDetector>();

        return services;
    }
}
