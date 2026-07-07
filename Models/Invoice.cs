
using InvoiceManagement.Api.Enum;

namespace InvoiceManagement.Api.Models
{
    public class Invoice
    {

        public int Id { get; set; }
        public string? InvoiceNumber { get; set; } = string.Empty;
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }

        //Historical tracking
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public ICollection<UpdateLog> UpdateLogs { get; set; } = new List<UpdateLog>();

        //Vendor relationship
        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        //Financial
        public decimal? SubTotal { get; set; } = 0;
        public decimal? TaxAmount { get; set; } = 0;
        public decimal? TotalAmount { get; set; } = 0;

        //File attachments
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        //Status
        public InvoiceStatus Status { get; set; } = InvoiceStatus.ToReview;

    }
}
