using InvoiceManagement.Api.Enum;

namespace InvoiceManagement.Api.Models
{
    public class InvoiceReview
    {

        public int Id { get; set; }
        public ReviewAction Action { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string ReviewedBy { get; set; } = string.Empty;
        public int ReviewedByUserId { get; set; }
        public DateTimeOffset ReviewDate { get; set; }

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

    }
}
