using InvoiceManagement.Api.Enum;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class AuthChangePasswordRequest
    {
        [Required]
        [MinLength(6, ErrorMessage = "Password must be a least 6 characters long.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = string.Empty;
        [Required]
        [MinLength(6, ErrorMessage = "Password must be a least 6 characters long.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
        [Required]
        [Compare(nameof(NewPassword))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
