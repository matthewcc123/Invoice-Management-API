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

            await AddUser("admin@domain.com", "admin123", UserRole.Admin);
            await AddUser("matthew@domain.com", "matthew123", UserRole.Vendor, 1);
        }

        private async Task AddUser(string email, string password, UserRole role, int? vendorId = null)
        {
            bool userExists = await _context.Users.AnyAsync(user => user.Email == email);

            if (userExists)
            {
                Console.WriteLine("User already exists.");
                return;
            }

            var newUser = new User
            {
                Email = email,
                Role = role,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                VendorId = vendorId,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            Console.WriteLine($"{email} created successfully.");
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
