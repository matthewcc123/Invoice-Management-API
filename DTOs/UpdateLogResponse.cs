namespace InvoiceManagement.Api.DTOs
{
    public class UpdateLogResponse
    {
        public DateTimeOffset DateTime { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
