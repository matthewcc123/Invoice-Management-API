using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.Models
{
    public class UpdateLog
    {
        public int Id { get; set; }
        [Required]
        public string EntityType { get; set; } = string.Empty;
        [Required]
        public int EntityId { get; set; }
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;
        public string Description { get; set; } = string.Empty;



    }
}
