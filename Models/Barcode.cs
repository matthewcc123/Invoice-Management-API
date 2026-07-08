namespace InvoiceManagement.Api.Models
{
    public class Barcode
    {

        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    }
}
