using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.Models;
using SolarflowServer.Models.Enums;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Battery> Batteries { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Forecast> Forecasts { get; set; }
    public DbSet<ViewAccount> ViewAccounts { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<Suggestion> Suggestions { get; set; }
    public DbSet<EnergyRecord> EnergyRecords { get; set; }


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
            entity.Property(a => a.Brief).HasMaxLength(255).IsRequired();
            entity.Property(a => a.IPAddress).HasMaxLength(50);
            entity.Property(a => a.Timestamp).HasDefaultValueSql("GETDATE()");

        });

        // MAPPING THE BATTERY.
        builder.Entity<Battery>(entity =>
        {
            entity.ToTable("Batteries");

            entity.HasKey(b => b.Id);

            // Basic properties with default values
            entity.Property(b => b.Capacity).HasDefaultValue(0.0);
            entity.Property(b => b.CapacityMax).HasDefaultValue(10.0);
            entity.Property(b => b.ChargeRate).HasDefaultValue(5.0);
            entity.Property(b => b.DischargeRate).HasDefaultValue(7.0);

            entity.Property(b => b.ChargeMode).HasDefaultValue(BatteryMode.Normal);
            entity.Property(b => b.ChargeSource).HasDefaultValue(BatterySource.All);
            entity.Property(b => b.ThresholdMin).HasDefaultValue(0);
            entity.Property(b => b.ThresholdMax).HasDefaultValue(100);
            entity.Property(b => b.ChargeGridStartTime).HasColumnType("time")
                .HasDefaultValue(new TimeSpan(0, 0, 0)); // default: 00:00
            entity.Property(b => b.ChargeGridEndTime).HasColumnType("time")
                .HasDefaultValue(new TimeSpan(9, 0, 0)); // default: 09:00

            entity.Property(b => b.LastUpdate).HasDefaultValueSql("getutcdate()");

            entity.HasOne(b => b.User)
                .WithOne(u => u.Battery)
                .HasForeignKey<Battery>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MAPPING THE Forecast.
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

        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<Suggestion>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Title).IsRequired().HasMaxLength(100);

            entity.Property(s => s.Description).IsRequired().HasMaxLength(300);

            entity.Property(s => s.Type).IsRequired();

            entity.Property(s => s.Status).IsRequired();

            entity.Property(s => s.TimeSent).IsRequired();

            entity.HasOne(s => s.Battery)
                .WithMany()
                .HasForeignKey(s => s.BatteryId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // MAPPING THE ENERGY RECORD.
        builder.Entity<EnergyRecord>(entity =>
        {
            entity.ToTable("EnergyRecords");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.House).IsRequired();
            entity.Property(e => e.Grid).IsRequired();
            entity.Property(e => e.Solar).IsRequired();
            entity.Property(e => e.Battery).IsRequired();

            entity.HasOne(er => er.ApplicationUser)
                .WithMany(au => au.EnergyRecords)
                .HasForeignKey(er => er.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}