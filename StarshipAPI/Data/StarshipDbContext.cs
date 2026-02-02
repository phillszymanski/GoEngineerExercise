using Microsoft.EntityFrameworkCore;
using StarshipAPI.Models;

public class StarshipDbContext : DbContext
{
    public StarshipDbContext(DbContextOptions<StarshipDbContext> options) : base(options)
    {
    }

    public DbSet<Starship> Starships { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Starship>().HasKey(s => s.Id);
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.Username).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
                        .Where(e => (e.Entity is Starship || e.Entity is User) && 
                                    (e.State == EntityState.Added || e.State == EntityState.Modified));

        var now = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            if (entry.Entity is Starship starship)
            {
                if (entry.State == EntityState.Added)
                {
                    starship.Created = now;
                    starship.Edited = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    starship.Edited = now;
                }
            }
            else if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added)
                {
                    user.Created = now;
                    user.Edited = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    user.Edited = now;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}