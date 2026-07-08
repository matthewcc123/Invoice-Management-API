using InvoiceManagement.Api.Enum;

namespace InvoiceManagement.Api.DTOs
{
    public class UserQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public SortOrder? Order { get; set; } = SortOrder.Asc;
        public string? Search { get; set; }
    }
}
