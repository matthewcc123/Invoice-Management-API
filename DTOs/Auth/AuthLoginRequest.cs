using InvoiceManagement.Api.Enum;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class AuthLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be a least 6 characters long.")]
        public string Password { get; set; } = string.Empty;


    }
}
