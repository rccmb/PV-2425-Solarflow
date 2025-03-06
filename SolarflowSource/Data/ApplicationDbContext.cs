using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // MAPPING THE APPLICATION USER.
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");

            entity.Property(u => u.Name)
                .HasColumnName("name")
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
    }
}

