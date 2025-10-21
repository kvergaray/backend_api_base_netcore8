using backend_api_base_netcore8.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend_api_base_netcore8.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100);

            entity.Property(e => e.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(80);

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255);

            entity.Property(e => e.Password)
                .HasColumnName("password")
                .HasMaxLength(255);

            entity.Property(e => e.DegreeId)
                .HasColumnName("degree_id");

            entity.Property(e => e.RememberToken)
                .HasColumnName("remember_token")
                .HasMaxLength(255);

            entity.Property(e => e.Phone)
                .HasColumnName("phone");

            entity.Property(e => e.Cip)
                .HasColumnName("cip");
        });
    }
}
