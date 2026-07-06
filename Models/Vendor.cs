using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.Models
{
    public class Vendor
    {

        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

    }
}
