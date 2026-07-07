using InvoiceManagement.Api.Enum;
using InvoiceManagement.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class InvoiceReviewResponse
    {
        public int Id { get; set; }
        public ReviewAction Action { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string ReviewedBy { get; set; } = string.Empty;
        public DateTimeOffset ReviewDate { get; set; }

    }
}
