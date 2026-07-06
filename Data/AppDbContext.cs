using InvoiceManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagement.Api.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Vendor> Vendors => Set<Vendor>();

    }
}
