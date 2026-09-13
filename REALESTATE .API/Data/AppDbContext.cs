using Microsoft.EntityFrameworkCore;
using REALESTATE_.API.models;

namespace REALESTATE_.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Property> Properties { get; set; }

        public DbSet<Owner> Owners { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Owner)
                .WithMany(o => o.Properties)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Buyer)
                .WithMany()
                .HasForeignKey(f => f.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Property)
                .WithMany()
                .HasForeignKey(f => f.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.BuyerId, f.PropertyId })
                .IsUnique();
            modelBuilder.Entity<PropertyImage>()
                .HasOne(i => i.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}