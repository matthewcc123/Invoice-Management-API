using InvoiceManagement.Api.Enum;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class AuthUpdateRoleRequest
    {
        [Required]
        public UserRole Role { get; set; } = UserRole.Vendor;
    }
}
