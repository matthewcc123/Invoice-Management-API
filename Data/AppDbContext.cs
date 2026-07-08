using InvoiceManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagement.Api.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<UpdateLog> UpdateLogs => Set<UpdateLog>();
        public DbSet<InvoiceReview> InvoiceReviews => Set<InvoiceReview>();
        public DbSet<Barcode> Barcodes => Set<Barcode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Vendor One To Many Users
            modelBuilder.Entity<User>()
                .HasOne(e => e.Vendor)
                .WithMany(v => v.Users)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            //Vendor One To Many Invoices
            modelBuilder.Entity<Invoice>()
                .HasOne(e => e.Vendor)
                .WithMany(v => v.Invoices)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            //Invoice One To Many Attachments
            modelBuilder.Entity<Attachment>()
                .HasOne(e => e.Invoice)
                .WithMany(i => i.Attachments)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            //Invoice One To Many Reviews
            modelBuilder.Entity<InvoiceReview>()
                .HasOne(e => e.Invoice)
                .WithMany(r => r.Reviews)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            //Invoice One To One Barcodes
            modelBuilder.Entity<Invoice>()
                .HasOne(e => e.Barcode)
                .WithOne(b => b.Invoice)
                .HasForeignKey<Barcode>(b => b.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            //Unique constraint on Barcode Code property
            modelBuilder.Entity<Barcode>()
                .HasIndex(b => b.Code)
                .IsUnique();


            if (Database.IsSqlite())
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.GetProperties()
                        .Where(p => p.ClrType == typeof(DateTimeOffset)
                                 || p.ClrType == typeof(DateTimeOffset?));

                    foreach (var property in properties)
                    {
                        property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, string>(
                            v => v.ToString("O"), // Converts to ISO 8601 string round-trip format
                            v => DateTimeOffset.Parse(v)
                        ));
                    }
                }
            }

        }

    }
}
