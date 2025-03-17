using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


    public DbSet<ViewAccount> ViewAccounts { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<Battery> Batteries { get; set; }


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

            entity.HasOne(u => u.ViewAccount) 
                .WithOne(v => v.User)
                .HasForeignKey<ViewAccount>(v => v.UserId) 
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("GETDATE()");
        });


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

            entity.Property(b => b.UserId)
            .HasColumnName("user_id")
            .IsRequired();

            entity.HasIndex(b => b.UserId)
            .IsUnique();

            entity.Property(b => b.ApiKey)
            .HasColumnName("api_key")
            .HasDefaultValue("")
            .IsRequired();

            entity.HasIndex(b => b.ApiKey)
            .IsUnique();

            entity.Property(b => b.ChargeLevel)
            .HasColumnName("charge_level")
            .HasDefaultValue(0);

            entity.Property(b => b.ChargingMode)
            .HasColumnName("charging_mode")
            .HasDefaultValue("");

            entity.Property(b => b.EmergencyMode)
            .HasColumnName("emergency_mode")
            .HasDefaultValue(false);

            entity.Property(b => b.AutoOptimization)
            .HasColumnName("auto_optimization")
            .HasDefaultValue(false);

            entity.Property(b => b.LastUpdate)
            .HasColumnName("last_update")
            .HasDefaultValue("");
        });
    }

}

