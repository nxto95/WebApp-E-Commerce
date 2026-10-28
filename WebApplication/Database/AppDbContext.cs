
using Microsoft.EntityFrameworkCore;
using WebApplication.Models;
using WebApplication.Models.Users;

namespace WebApplication.Database;

public class AppDbContext : DbContext
{

    public DbSet<UsersEntity> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            var connectionString =
                 config.GetConnectionString("WebAppDatabase")
                 ?? throw new InvalidOperationException(
                     "Missing connection string: WebAppDatabase");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditingAndSoftDelete();
        return base.SaveChanges();
    }
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditingAndSoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditingAndSoftDelete();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditingAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditingAndSoftDelete()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is IAuditable auditableEntry)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditableEntry.CreatedAt = now;
                        auditableEntry.UpdatedAt = null;
                        break;
                    case EntityState.Modified:
                        auditableEntry.UpdatedAt = now;
                        entry.Property(e => e.CreatedAt).IsModified = false;
                        break;
                }
            }
            if (entry.Entity is ISoftDelete deletedEntry)
            {
                switch (entry.State)
                {
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        deletedEntry.DeletedAt = now;
                        deletedEntry.IsDeleted = true;
                        break;

                }
            }
        }
    }


}
