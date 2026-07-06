using InvoiceManagement.Api.Enum;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Api.DTOs
{
    public class UserUpdateRequest
    {
        public int? VendorId { get; set; }
    }
}
