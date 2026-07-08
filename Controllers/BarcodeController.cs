using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Models;
using InvoiceManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace InvoiceManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BarcodeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BarcodeService _barcodeService;

        public BarcodeController(AppDbContext context, BarcodeService barcodeService)
        {
            _context = context;
            _barcodeService = barcodeService;
        }

        [HttpGet("{invoiceId}")]
        [Authorize]
        public async Task<IActionResult> Preview(int invoiceId)
        {
            var invoice = await _context.Invoices.Include(i => i.Barcode).FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null || invoice.Barcode == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = invoice == null ? "Invoice not found." : invoice.Barcode == null ? "Barcode not found." : "Invoice and Barcode not found."
                });
            }

            var user = await GetCurrentUserAsync();
            if (user == null || !HasVendorAccess(user, invoice.VendorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You are not authorized to view this invoice barcode."
                }));
            }

            var bytes = _barcodeService.GenerateBarcodeImage(invoice.Barcode.Code);

            return File(bytes, "image/png", invoice.Barcode.Code);
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _context.Users.FindAsync(userId);
        }

        private bool HasVendorAccess(User user, int vendorId)
        {
            return User.IsInRole("Admin") || user.VendorId == vendorId;
        }
    }
}
