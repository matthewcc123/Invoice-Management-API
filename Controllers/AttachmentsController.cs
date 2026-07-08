using AutoMapper;
using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvoiceManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AttachmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public AttachmentsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("{name}")]
        [Authorize]
        public async Task<IActionResult> Preview(string name)
        {
            var attachment = await _context.Attachments.Include(a => a.Invoice).FirstOrDefaultAsync(a => a.FileName == name);

            if (attachment == null)
            {
                return NotFound(new ApiResponse<InvoiceResponse>
                {
                    Success = false,
                    Message = "Attachment not found."
                });
            }

            //Check if User have rights to access invoice for the given vendor
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || (!User.IsInRole("Admin") && user.VendorId != attachment.Invoice!.VendorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You do not have permission to access an attachment from this invoice."
                }));
            }

            //Create Directory
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads",  attachment.FileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new ApiResponse<InvoiceResponse>
                {
                    Success = false,
                    Message = "Attachment not found."
                });
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileExtension = Path.GetExtension(attachment.FileName);

            return File(stream, $"application/{fileExtension}", attachment.OriginalName);

        }

        [HttpDelete("delete/{name}")]
        [Authorize]
        public async Task<IActionResult> Delete(string name)
        {
            var attachment = await _context.Attachments.Include(a => a.Invoice).FirstOrDefaultAsync(a => a.FileName == name);

            if (attachment == null)
            {
                return NotFound(new ApiResponse<InvoiceResponse>
                {
                    Success = false,
                    Message = "Attachment not found."
                });
            }

            //Check if User have rights to access invoice for the given vendor
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || (!User.IsInRole("Admin") && user.VendorId != attachment.Invoice!.VendorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You do not have permission to access an attachment from this invoice."
                }));
            }

            //Create Directory
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", attachment.FileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new ApiResponse<InvoiceResponse>
                {
                    Success = false,
                    Message = "Attachment not found."
                });
            }

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();

            System.IO.File.Delete(filePath);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Attachment uploaded successfully."
            });


        }

    }
}
