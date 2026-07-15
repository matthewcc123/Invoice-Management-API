namespace InvoiceManagement.Api.DTOs
{
    public class TokenResult
    {
        public string Token { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
    }
}
