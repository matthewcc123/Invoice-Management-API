using InvoiceManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagement.Api.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Vendor One To Many Users
            modelBuilder.Entity<User>()
                .HasOne(e => e.Vendor)
                .WithMany(v => v.Users)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);


        }

    }
}
