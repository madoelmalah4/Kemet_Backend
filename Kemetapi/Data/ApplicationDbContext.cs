using Microsoft.EntityFrameworkCore;
using Kemet_api.Models;

namespace Kemet_api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Day> Days { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
            });

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TripType).HasConversion<string>();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.HasMany(t => t.Days)
                      .WithOne(d => d.Trip)
                      .HasForeignKey(d => d.TripId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
