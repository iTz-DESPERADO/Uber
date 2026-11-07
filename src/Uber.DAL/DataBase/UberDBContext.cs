using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Uber.DAL.Entities;
namespace Uber.DAL.DataBase
{
    public class UberDBContext : IdentityDbContext<ApplicationUser>
    {



        public UberDBContext(DbContextOptions<UberDBContext> options) : base(options)
        {

        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Server=.;Database=;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true");
        //}

        public DbSet<Admin> Admins { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Ride> Rides { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        //public DbSet<Wallet> Wallets { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This is CRUCIAL to configure Identity tables' keys
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Driver)
                .WithMany(d => d.Rides)
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.User)
                .WithMany(u => u.Rides)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
