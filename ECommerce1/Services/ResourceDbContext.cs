using ECommerce1.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce1.Services
{
    public class ResourceDbContext : DbContext
    {
        public ResourceDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductPhoto> ProductPhotos { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<City>(e =>
            {
                e.Property(e => e.Name)
                .HasColumnType("nvarchar(256)")
                .HasMaxLength(256);

                e.Property(e => e.Id)
                .IsRequired();

                e.HasKey(e => e.Id);

                e.HasMany(c => c.Profiles)
                .WithOne(p => p.City)
                .OnDelete(DeleteBehavior.SetNull);

                e.HasIndex(e => e.Name).IsUnique();
            });

            builder.Entity<Category>(e =>
            {
                e.Property(e => e.Name)
                .HasColumnType("nvarchar(256)")
                .HasMaxLength(256)
                .IsRequired();

                e.Property(e => e.Id)
                .IsRequired();

                e.HasKey(e => e.Id);

                e.Property(e => e.AllowProducts)
                .HasDefaultValue(false).IsRequired();

                e.HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .IsRequired();

                e.HasIndex(e => e.Name).IsUnique();
            });

            builder.Entity<ProductPhoto>(e =>
            {
                e.Property(e => e.Id)
                .IsRequired();

                e.HasKey(e => e.Id);

                e.Property(e => e.Url)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

                e.HasOne(pp => pp.Product)
                .WithMany(p => p.ProductPhotos)
                .IsRequired();
            });

            builder.Entity<Profile>(e =>
            {
                e.Property(e => e.Id)
                .IsRequired();

                e.HasKey(e => e.Id);

                e.Property(e => e.AuthId)
                .IsRequired();

                e.Property(e => e.Username)
                .HasColumnType("nvarchar(32)")
                .HasMaxLength(32)
                .IsRequired();

                e.HasIndex(e => e.Username).IsUnique();

                e.Property(e => e.FirstName)
                .HasColumnType("nvarchar(64)")
                .HasMaxLength(64)
                .IsRequired();

                e.Property(e => e.LastName)
                .HasColumnType("nvarchar(64)")
                .HasMaxLength(64)
                .IsRequired();

                e.Property(e => e.PhoneNumber)
                .HasColumnType("nvarchar(15)")
                .HasMaxLength(15)
                .IsRequired();

                e.HasIndex(e => e.PhoneNumber).IsUnique();

                e.HasOne(p => p.City)
                .WithMany(c => c.Profiles);

                e.Property(e => e.Email)
                .HasColumnType("nvarchar(320)")
                .HasMaxLength(320)
                .IsRequired();

                e.HasIndex(e => e.Email).IsUnique();

                e.HasMany(prof => prof.Products)
                .WithOne(prod => prod.User)
                .IsRequired();
            });

            builder.Entity<Product>(e =>
            {
                e.Property(e => e.Id)
                .IsRequired();

                e.HasKey(e => e.Id);

                e.Property(e => e.CreationTime)
                
                .HasDefaultValueSql("getdate()");

                e.Property(e => e.Name)
                .HasColumnType("nvarchar(128)")
                .HasMaxLength(128)
                .IsRequired();

                e.Property(e => e.Description)
                .HasColumnType("nvarchar(max)");

                e.Property(e => e.Price)
                .HasColumnType("money")
                .IsRequired();

                e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .IsRequired();

                e.HasOne(p => p.User)
                .WithMany(u => u.Products)
                .IsRequired();

                e.HasMany(p => p.ProductPhotos)
                .WithOne(pp => pp.Product)
                .IsRequired();
            });
        }
    }
}
