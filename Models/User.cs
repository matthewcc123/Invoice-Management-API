using InvoiceManagement.Api.Enum;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Vendor;

        //EF
        public int? VendorId { get; set; }
        public Vendor? Vendor { get; set; }

    }
}
