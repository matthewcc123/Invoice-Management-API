using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManagement.Api.DTOs
{
    public class InvoiceSummaryResponse
    {
        public int TotalInvoices { get; set; }
        public int Draft { get; set; }
        public int Pending { get; set; }
        public int Approve { get; set; }
        public int Reject { get; set; }
        public int OnProcess { get; set; }
        public int Paid { get; set; }
        public int Received { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal TotalOutstandingAmount { get; set; }

    }
}
