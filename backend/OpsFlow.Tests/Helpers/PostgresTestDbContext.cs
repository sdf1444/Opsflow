using Microsoft.EntityFrameworkCore;
using OpsFlow.Domain.Entities;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Tests.Helpers;

public class PostgresTestDbContext : AppDbContext
{
    public PostgresTestDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplySeedDefaults();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplySeedDefaults();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplySeedDefaults()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<User>().Where(e => e.State == EntityState.Added))
        {
            if (string.IsNullOrWhiteSpace(entry.Entity.PasswordHash))
            {
                entry.Entity.PasswordHash = "hash";
            }

            if (entry.Entity.CreatedAt == default)
            {
                entry.Entity.CreatedAt = now;
            }

            if (entry.Entity.UpdatedAt == default)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
