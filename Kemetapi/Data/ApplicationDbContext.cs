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
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<DayActivity> DayActivities { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }

        public DbSet<VirtualTour> VirtualTours { get; set; }

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
                entity.Property(e => e.TravelCompanions).HasConversion<string>();
                entity.Property(e => e.TravelStyle).HasConversion<string>();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                // EF Core 8+ handles List<string> automatically as JSON collections
                
                entity.HasMany(t => t.Days)
                      .WithOne(d => d.Trip)
                      .HasForeignKey(d => d.TripId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Destination>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EstimatedPrice).HasColumnType("decimal(18,2)");
                
                // One-to-One relationship with VirtualTour
                entity.HasOne(e => e.VirtualTour)
                      .WithOne(v => v.Destination)
                      .HasForeignKey<VirtualTour>(v => v.Destination_id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DayActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActivityType).HasConversion<string>();
                entity.HasOne(da => da.Day)
                      .WithMany(d => d.DayActivities)
                      .HasForeignKey(da => da.DayId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(da => da.Destination)
                      .WithMany()
                      .HasForeignKey(da => da.DestinationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(uf => new { uf.UserId, uf.DestinationId });

                entity.HasOne(uf => uf.User)
                      .WithMany(u => u.Favorites)
                      .HasForeignKey(uf => uf.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uf => uf.Destination)
                      .WithMany() // No collection in Destination, unidirectional from User perspective is fine, or update Destination too? 
                                  // User didn't ask to see who favorited a destination, just that user has favorites.
                      .HasForeignKey(uf => uf.DestinationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
