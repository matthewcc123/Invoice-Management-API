
using InvoiceManagement.Api.Enum;

namespace InvoiceManagement.Api.Models
{
    public class Attachment
    {

        public int Id { get; set; }
        public string FilePath { get; set; } = string.Empty;

        //Invoice Relationship
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

    }
}
