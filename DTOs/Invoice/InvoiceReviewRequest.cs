using InvoiceManagement.Api.Enum;
using InvoiceManagement.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class InvoiceReviewRequest
    {
        [Required]
        public string Remarks { get; set; } = string.Empty;
        [Required]
        public ReviewAction Action { get; set; } = ReviewAction.Approved;
        [Required]
        public bool IsDeliveryRequired { get; set; } = false;

    }
}
