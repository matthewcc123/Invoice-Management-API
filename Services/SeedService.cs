using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

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
            var email = "admin@domain.com";

            bool userExists = await _context.Users.AnyAsync(user => user.Email == email);

            if (userExists)
            {
                Console.WriteLine("Admin user already exists.");
                return;
            }

            var newUser = new User
            {
                Email = email,
                Role = Enum.UserRole.Admin,
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123")
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            Console.WriteLine("Admin user created successfully.");
        }
    }
}
