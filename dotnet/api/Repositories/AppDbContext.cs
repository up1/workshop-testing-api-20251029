using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            
            entity.Property(e => e.FullName)
                .HasColumnName("full_name")
                .IsRequired();
            
            entity.Property(e => e.Username)
                .HasColumnName("username")
                .IsRequired();
            
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .IsRequired();
            
            entity.Property(e => e.Phone)
                .HasColumnName("phone")
                .IsRequired();
            
            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();
            
            entity.Property(e => e.Dob)
                .HasColumnName("dob")
                .IsRequired();
            
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .IsRequired()
                .HasDefaultValue("pending_verification");
            
            entity.Property(e => e.AcceptTerms)
                .HasColumnName("accept_terms")
                .IsRequired()
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
            
            entity.Property(e => e.VerifiedAt)
                .HasColumnName("verified_at");

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Phone).IsUnique();
        });
    }
}
