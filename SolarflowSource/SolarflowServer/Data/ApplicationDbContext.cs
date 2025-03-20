using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Battery> Batteries { get; set; }

    public DbSet<Forecast> Forecasts { get; set; }
    public DbSet<ViewAccount> ViewAccounts { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // MAPPING THE APPLICATION USER.
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");

            entity.Property(u => u.Fullname).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Photo).HasMaxLength(255).IsRequired(false);
            entity.Property(u => u.ConfirmedEmail).HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(u => u.Battery)
                .WithOne(b => b.User)
                .HasForeignKey<Battery>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.ViewAccount) 
                .WithOne(v => v.User)
                .HasForeignKey<ViewAccount>(v => v.UserId) 
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MAPPING THE VIEW ACCOUNT.
        builder.Entity<ViewAccount>(entity =>
        {
            entity.ToTable("ViewAccounts");

            entity.HasOne(v => v.User) 
                .WithOne(u => u.ViewAccount)
                .HasForeignKey<ViewAccount>(v => v.UserId)
                .IsRequired();
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

        // MAPPING THE BATTERY.
        builder.Entity<Battery>(entity =>
        {
            entity.ToTable("Batteries");

            entity.HasKey(b => b.ID);

            entity.Property(b => b.ChargeLevel).HasDefaultValue(0);
            entity.Property(b => b.ChargingSource).HasDefaultValue("");
            entity.Property(b => b.BatteryMode).HasDefaultValue("");
            entity.Property(b => b.MinimalTreshold).HasDefaultValue(0);
            entity.Property(b => b.MaximumTreshold).HasDefaultValue(100);
            entity.Property(b => b.SpendingStartTime).HasDefaultValue("00:00");
            entity.Property(b => b.SpendingEndTime).HasDefaultValue("09:00");
            entity.Property(b => b.LastUpdate).HasDefaultValue("");

            entity.HasOne(b => b.User)
                  .WithOne(u => u.Battery)
                  .HasForeignKey<Battery>(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Forecast>(entity =>
        {
            entity.ToTable("Forecasts");
            entity.HasKey(f => f.ID);
            entity.Property(f => f.ForecastDate).IsRequired();
            entity.Property(f => f.SolarHoursExpected).IsRequired();
            entity.Property(f => f.WeatherCondition).HasMaxLength(100).IsRequired();
            entity.HasOne<Battery>()
                .WithMany()  
                .HasForeignKey(f => f.BatteryID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

