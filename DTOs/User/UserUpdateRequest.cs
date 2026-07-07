using InvoiceManagement.Api.Enum;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class UserUpdateRequest
    {
        [Required]
        public string? Username { get; set; }
    }
}
