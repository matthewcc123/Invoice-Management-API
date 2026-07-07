namespace InvoiceManagement.Api.DTOs
{
    public class AttachmentResponse
    {

        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    }
}
