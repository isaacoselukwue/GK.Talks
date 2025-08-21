using GK.Talks.Core.Aggregates.SpeakerAggregate;
using Microsoft.EntityFrameworkCore;

namespace GK.Talks.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    public DbSet<Speaker> Speakers { get; set; }
    public DbSet<Session> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Speaker>(b =>
        {
            b.OwnsOne(s => s.Name);
            b.OwnsOne(s => s.Email);
            b.HasMany(s => s.Sessions).WithOne().IsRequired();
            b.OwnsOne(s => s.Browser, bb =>
            {
                bb.Property(b => b.Name)
                  .HasConversion<string>()
                  .HasColumnName("BrowserName");

                bb.Property(b => b.MajorVersion)
                  .HasColumnName("BrowserMajorVersion");
            });
        });
    }
}