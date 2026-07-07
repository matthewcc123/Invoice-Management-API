namespace InvoiceManagement.Api.DTOs
{
    public class FileResponse
    {
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public byte[]? Data { get; set; }
    }
}
