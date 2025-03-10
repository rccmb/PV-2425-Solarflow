using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // MAPPING THE APPLICATION USER.
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");

            entity.Property(u => u.Fullname)
                .HasColumnName("fullname")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.Photo)
                .HasColumnName("photo")
                .HasMaxLength(255)
                .IsRequired(false);

            entity.Property(u => u.ConfirmedEmail)
                .HasColumnName("confirmed_email")
                .HasDefaultValue(false);

            entity.Property(u => u.BatteryAPI)
                .HasColumnName("battery_api")
                .HasMaxLength(255)
                .IsRequired(false);

            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("GETDATE()");
        });

        // MAPPING THE AUDIT LOG.
        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.Property(a => a.UserId).IsRequired();
            entity.Property(a => a.Action).HasMaxLength(255).IsRequired();
            entity.Property(a => a.Email).HasMaxLength(255).IsRequired();
            entity.Property(a => a.IPAddress).HasMaxLength(50);
            entity.Property(a => a.Timestamp).HasDefaultValueSql("GETDATE()");
        });
    }
}

