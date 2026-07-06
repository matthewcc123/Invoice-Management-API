namespace InvoiceManagement.Api.DTOs
{
    public class VendorRequest
    {

        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

    }
}
