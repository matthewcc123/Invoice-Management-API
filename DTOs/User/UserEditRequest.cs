using InvoiceManagement.Api.Enum;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class UserEditRequest
    {
        [Required]
        public string? Username { get; set; }
    }
}
