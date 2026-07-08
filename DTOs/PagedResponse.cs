namespace InvoiceManagement.Api.DTOs
{
    public class PagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPrevPage => PageNumber > 1;
        public ICollection<T> Data { get; set; } = [];

        public PagedResponse(ICollection<T> items, int pageNumber, int pageSize, int totalRecords)
        {
            Data = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        }

    }
}
