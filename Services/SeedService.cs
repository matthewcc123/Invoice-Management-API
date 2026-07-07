using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Enum;
using InvoiceManagement.Api.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace InvoiceManagement.Api.Services
{
    public class SeedService
    {

        private readonly AppDbContext _context;

        public SeedService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedDataAsync()
        {
            await AddVendor(new Vendor
            {
                Name = "RM Industries",
                Email = "RM@Email.com",
                PhoneNumber = "1234567890",
            });
            await AddVendor(new Vendor
            {
                Name = "CJ Traders",
                Email = "CJ@Email.com",
                PhoneNumber = "1234567890",
            });

            await AddUser(new User
            {
                Username = "admin",
                Email = "admin@domain.com",
                Password = "admin123",
                Role = UserRole.Admin
            });

            await AddUser(new User
            {
                Username = "matthew",
                Email = "matthew@domain.com",
                Password = "matthew123",
                Role = UserRole.Vendor,
                VendorId = 1
            });
        }

        private async Task AddUser(User user)
        {
            bool userExists = await _context.Users.AnyAsync(u=> u.Email == user.Email);

            if (userExists)
            {
                Console.WriteLine("User already exists.");
                return;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            Console.WriteLine($"{user.Username} created successfully.");
        }

        private async Task AddVendor(Vendor vendor)
        {
            bool vendorExists = await _context.Vendors.AnyAsync(v => v.Name == vendor.Name);

            if (vendorExists)
            {
                Console.WriteLine("Vendor already exists.");
                return;
            }


            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            Console.WriteLine($"{vendor.Name} created successfully.");

        }

    }
}
