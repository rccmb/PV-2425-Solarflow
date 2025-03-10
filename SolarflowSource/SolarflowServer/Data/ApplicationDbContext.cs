using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ViewAccount> ViewAccounts { get; set; }

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
    }

}

