using GK.Talks.Core.DomainEvents;
using GK.Talks.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GK.Talks.Tests.Integration.Api;
internal class TestNotificationHandler : INotificationHandler<SpeakerRegisteredEvent>
{
    public readonly List<SpeakerRegisteredEvent> Events = new List<SpeakerRegisteredEvent>();
    public Task Handle(SpeakerRegisteredEvent notification, CancellationToken cancellationToken)
    {
        Events.Add(notification);
        return Task.CompletedTask;
    }
}

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;
    public TestNotificationHandler NotificationHandler { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            var appDbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(AppDbContext));
            if (appDbDescriptor != null) services.Remove(appDbDescriptor);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            var existingHandlers = services.Where(d => d.ServiceType == typeof(INotificationHandler<SpeakerRegisteredEvent>)).ToList();
            foreach (var d in existingHandlers) services.Remove(d);

            services.AddSingleton<INotificationHandler<SpeakerRegisteredEvent>>(sp => NotificationHandler);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}