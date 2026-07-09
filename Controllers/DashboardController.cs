using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Enum;
using InvoiceManagement.Api.Extensions;
using InvoiceManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace InvoiceManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {

        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("invoice-summary")]
        [Authorize]
        public async Task<IActionResult> GetStats()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You are not authorized."
                }));
            }

            var vendorId = user.VendorId;

            var invoiceQuery = _context.Invoices.AsQueryable();
            invoiceQuery = invoiceQuery.ApplyFilter(nameof(Invoice.VendorId), User.IsInRole("Admin") ? null : vendorId);

            int totalInvoices = await invoiceQuery.CountAsync();
            int draftCount = await invoiceQuery.CountAsync(i => i.Status == InvoiceStatus.Draft);
            int pendingCount = await invoiceQuery.CountAsync(i => i.Status == InvoiceStatus.Pending);
            int approveCount = await invoiceQuery.CountAsync(i => i.Status == InvoiceStatus.Approved);
            int rejectCount = await invoiceQuery.CountAsync(i => i.Status == InvoiceStatus.Rejected);
            int processCount = await invoiceQuery.CountAsync(i => i.Status == InvoiceStatus.OnProcess);
            int paidCount = await invoiceQuery.CountAsync(i => i.Status == InvoiceStatus.Paid);
            int receivedCount = await invoiceQuery.CountAsync(i => i.DeliveryStatus == InvoiceDeliveryStatus.Received);

            decimal totalAmount = await invoiceQuery.SumAsync(i => i.TotalAmount);
            decimal totalOutstanding =  await invoiceQuery.Where(i => i.Status != InvoiceStatus.Paid).SumAsync(i => i.TotalAmount);

            var summary = new InvoiceSummaryResponse
            {
                TotalInvoices = totalInvoices,
                Draft = draftCount,
                Pending = pendingCount,
                Approve = approveCount,
                Reject = rejectCount,
                OnProcess = processCount,
                Paid = paidCount,
                Received = receivedCount,
                TotalAmount = totalAmount,
                TotalOutstandingAmount = totalOutstanding
            };

            return Ok(new ApiResponse<InvoiceSummaryResponse>
            {
                Success = true,
                Message = "Statistic retrived successfully.",
                Data = summary

            });

        }

        private async Task<User?> GetCurrentUserAsync()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _context.Users.FindAsync(userId);
        }


    }
}
