using InvoiceManagement.Api.Enum;
using InvoiceManagement.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class InvoiceCreateRequest
    {
        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        [Required]
        public int VendorId { get; set; }

        //Financial
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

    }
}
