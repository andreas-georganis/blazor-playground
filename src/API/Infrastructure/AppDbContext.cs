using BlazorPlayground.API.Model;
using Microsoft.EntityFrameworkCore;

namespace BlazorPlayground.API.Infrastructure;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Customer>(c =>
        {
            c.ToTable("Customers");

            c.HasKey(x => x.Id);

            c.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .ValueGeneratedOnAdd();

            c.Property(x => x.CompanyName)
                .HasMaxLength(100);

            c.Property(x => x.ContactName)
                .HasMaxLength(100);

            c.Property(x => x.Address)
                .HasMaxLength(200);

            c.Property(x => x.City)
                .HasMaxLength(100);

            c.Property(x => x.Region)
                .HasMaxLength(50);

            c.Property(x => x.PostalCode)
                .HasMaxLength(20);

            c.Property(x => x.Country)
                .HasMaxLength(50);

            c.Property(x => x.Phone)
                .HasMaxLength(50);

            // Optional indexes
            c.HasIndex(x => x.CompanyName);
            c.HasIndex(x => x.Country);
        });
    }
}