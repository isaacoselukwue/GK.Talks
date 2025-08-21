using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
namespace GK.Talks.Infrastructure.Data.Interceptors;
public class AuditingInterceptor(ILogger<AuditingInterceptor> logger) : SaveChangesInterceptor
{
    private readonly ILogger<AuditingInterceptor> _logger = logger;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var entries = eventData.Context?.ChangeTracker.Entries();
        if (entries is not null)
            foreach (var entry in entries.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                _logger.LogInformation("AUDIT: Entity {EntityName} was {State}.", entry.Entity.GetType().Name, entry.State);
            }
        return base.SavingChanges(eventData, result);
    }
}