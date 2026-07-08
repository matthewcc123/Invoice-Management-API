using AutoMapper;
using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Enum;
using InvoiceManagement.Api.Models;
using InvoiceManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvoiceManagement.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly BarcodeService _barcodeService;
        private readonly long MaxFileSize = 5 * 1024;
        public InvoicesController(AppDbContext context, IMapper mapper, BarcodeService barcodeService)
        {
            _context = context;
            _mapper = mapper;
            _barcodeService = barcodeService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetInvoices()
        {
            // This will filter only the users that belong to the same vendor as the authenticated user, unless the user is an Admin.

            var invoices = await _context.Invoices.Include(i => i.Barcode).ToListAsync();
            var invoicesResponse = _mapper.Map<List<InvoiceResponse>>(invoices);

            return Ok(new ApiResponse<List<InvoiceResponse>>
            {
                Success = true,
                Message = "Invoices retrieved successfully.",
                Data = invoicesResponse
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _context.Invoices.Include(i => i.Attachments).Include(i => i.Reviews).Include(i => i.Barcode).FirstAsync(i => i.Id == id);
            if (invoice == null)
            {
                return NotFound(new ApiResponse<InvoiceResponse>
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }

            //Check if User have rights to access invoice for the given vendor
            var user = await GetCurrentUserAsync();
            if (user == null || !HasVendorAccess(user, invoice.VendorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You are not authorized to view an invoice for this vendor."
                }));
            }

            var logs = await _context.UpdateLogs
                .Where(log => log.EntityType == nameof(Invoice) && log.EntityId == invoice.Id)
                .OrderBy(log => log.Id)
                .ToListAsync();

            invoice.UpdateLogs = logs;

            var invoiceResponse = _mapper.Map<InvoiceDetailResponse>(invoice);
            return Ok(new ApiResponse<InvoiceDetailResponse>
            {
                Success = true,
                Message = "Invoice retrieved successfully.",
                Data = invoiceResponse
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateInvoice(InvoiceCreateRequest invoiceRequest)
        {
            //Check Vendor Exists
            if (invoiceRequest.VendorId != 0)
            {
                var vendorExists = await _context.Vendors.AnyAsync(v => v.Id == invoiceRequest.VendorId);
                if (!vendorExists)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Vendor not found."
                    });
                }
            }

            //Check if User have rights to make invoice for the given vendor
            var user = await GetCurrentUserAsync();
            if (user == null || !HasVendorAccess(user, invoiceRequest.VendorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You are not authorized to create an invoice for this vendor."
                }));
            }


            var invoice = _mapper.Map<Invoice>(invoiceRequest);

            //Set CreatedAt to current UTC time
            invoice.CreatedAt = DateTimeOffset.UtcNow;

            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();

            var createdInvoiceResponse = _mapper.Map<InvoiceResponse>(invoice);
            return CreatedAtAction(nameof(GetInvoice), new { id = createdInvoiceResponse.Id }, new ApiResponse<InvoiceResponse>
            {
                Success = true,
                Message = "Invoice created successfully.",
                Data = createdInvoiceResponse
            });

        }

        [HttpPut("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, InvoiceEditRequest invoiceRequest)
        {
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }

            if (invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.Rejected)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Only draft or rejected invoice can be edited"
                });
            }

            //Check if User have rights to make invoice for the given vendor
            var user = await GetCurrentUserAsync();
            if (user == null || !HasVendorAccess(user, invoice.VendorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You are not authorized to edit this invoice."
                }));
            }


            //Add UpdateLog
            var now = DateTimeOffset.UtcNow;

            var updateLog = new UpdateLog
            {
                EntityType = nameof(Invoice),
                EntityId = invoice.Id,
                DateTime = now,
                Description = $"Invoice updated by {user.Username}"
            };

            //Apply
            _mapper.Map(invoiceRequest, invoice);
            _context.Entry(invoice).State = EntityState.Modified;
            await _context.UpdateLogs.AddAsync(updateLog);
            await _context.SaveChangesAsync();

            var updatedInvoiceResponse = _mapper.Map<InvoiceResponse>(invoice);
            return Ok(new ApiResponse<InvoiceResponse>
            {
                Success = true,
                Message = "Invoice updated successfully.",
                Data = updatedInvoiceResponse
            });
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }

            //Check if User have rights to delete invoice for the given vendor
            var user = await GetCurrentUserAsync();
            if (user == null || !HasVendorAccess(user, invoice.VendorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You are not authorized to delete this invoice."
                }));
            }


            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Invoice deleted successfully."
            });
        }

        [HttpPost("{id}/upload-attachment")]
        [Authorize]
        public async Task<IActionResult> Upload(int id, IFormFile file)
        {

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }

            //Check if User have rights to delete attachment for the given invoice
            var user = await GetCurrentUserAsync();
            bool isNotDraft = invoice.Status != InvoiceStatus.Draft || invoice.Status != InvoiceStatus.Rejected;
            if (user == null || !HasVendorAccess(user, invoice.VendorId) || (!User.IsInRole("Admin") && isNotDraft))
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You are not authorized to upload attachment to this invoice."
                }));
            }


            if (file == null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "File is required."
                });
            }

            bool isPdf = file.ContentType == "application/pdf";
            bool isFileSizeValid = file.Length <= MaxFileSize;

            //Check File Requirement
            if (!isPdf || isFileSizeValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = !isPdf ? "Only PDF files are allowed." : "File size exceeds the maximum limit."
                });
            }
            
            //Create Directory
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            //Generate FileName and Path
            var fileExtension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadFolder, storedFileName);

            if (System.IO.File.Exists(filePath))
            {
                return Conflict(new ApiResponse
                {
                    Success = false,
                    Message = "File already exists."
                });
            }

            //Copy
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //Attach File Path to Invoice
            var attachment = new Attachment
            {
                InvoiceId = invoice.Id,
                FileName = storedFileName,
                OriginalName = file.FileName,
                UploadedAt = DateTimeOffset.UtcNow,
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();


            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Attachment uploaded successfully."
            });
        }

        [HttpPost("{id}/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitToReview(int id)
        {

            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }

            var user = await GetCurrentUserAsync();
            if (user == null || !HasVendorAccess(user, invoice.VendorId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, (new ApiResponse
                {
                    Success = false,
                    Message = "You are not authorized to submit this invoice."
                }));
            }

            bool canSubmit = invoice.Status == InvoiceStatus.Draft || invoice.Status == InvoiceStatus.Rejected;

            if (!canSubmit)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Only draft or rejected invoices can be submited."
                });
            }


            //Add UpdateLog
            var now = DateTimeOffset.UtcNow;
            var updateLog = new UpdateLog
            {
                EntityType = nameof(Invoice),
                EntityId = invoice.Id,
                DateTime = now,
                Description = $"Invoice submited by {user.Username}"
            };

            //Apply
            invoice.Status = InvoiceStatus.Pending;
            _context.Entry(invoice).State = EntityState.Modified;
            await _context.UpdateLogs.AddAsync(updateLog);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"Invoice submited successfully."
            });
        }

        [HttpPost("{id}/review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, InvoiceReviewRequest invoiceReviewRequest)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string username = User.FindFirst(ClaimTypes.Name)!.Value;

            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }

            if (invoice.Status != InvoiceStatus.Pending)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Only pending invoices can be reviewed."
                });
            }

            //Create Review
            var now = DateTimeOffset.UtcNow;

            InvoiceReview review = new InvoiceReview
            {
                InvoiceId = invoice.Id,
                Action = invoiceReviewRequest.Action,
                Remarks = invoiceReviewRequest.Remarks,
                ReviewDate = now,
                ReviewedBy = username,
                ReviewedByUserId = userId
            };


            //Add UpdateLog
            var updateLog = new UpdateLog
            {
                EntityType = nameof(Invoice),
                EntityId = invoice.Id,
                DateTime = now,
                Description = $"Invoice {invoiceReviewRequest.Action} by {username}"
            };

            //Change Invoice Status
            if (!invoiceReviewRequest.IsDeliveryRequired)
            {
                invoice.DeliveryStatus = InvoiceDeliveryStatus.Received;
                invoice.ReceivedAt = now;
            }

            invoice.DeliveryStatus = invoiceReviewRequest.IsDeliveryRequired ? InvoiceDeliveryStatus.Pending : InvoiceDeliveryStatus.Received;
            invoice.Status = invoiceReviewRequest.Action == ReviewAction.Approved ? InvoiceStatus.Approved : InvoiceStatus.Rejected;

            //Update DB
            Barcode? barcode = null;

            if (invoiceReviewRequest.Action == ReviewAction.Approved)
            {
                var prefix = "INV" + DateTime.UtcNow.ToString("MMyyyy");
                barcode = await _barcodeService.GenerateBarcodeAsync(invoice.Id, prefix);
                await _context.Barcodes.AddAsync(barcode);
            }

            await _context.InvoiceReviews.AddAsync(review);
            await _context.UpdateLogs.AddAsync(updateLog);

            _context.Entry(invoice).State = EntityState.Modified;

            //Apply Changes
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
                {
                    Success = false,
                    Message = "Failed to generate a unique barcode. Please try again."
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = invoiceReviewRequest.Action == ReviewAction.Approved ? $"Invoice approved successfully." : $"Invoice rejected successfully."
            });
        }

        [HttpPost("{id}/receive")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsReceived(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string username = User.FindFirst(ClaimTypes.Name)!.Value;

            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }


            if (invoice.Status != InvoiceStatus.Approved && invoice.DeliveryStatus != InvoiceDeliveryStatus.Pending)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Only approved invoice that have pending delivery can be marked as received."
                });
            }

            if (invoice.DeliveryStatus == InvoiceDeliveryStatus.Received)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invoice already received."
                });
            }

            //Create Review
            var now = DateTimeOffset.UtcNow;

            //Add UpdateLog
            var updateLog = new UpdateLog
            {
                EntityType = nameof(Invoice),
                EntityId = invoice.Id,
                DateTime = now,
                Description = $"Invoice received by {username}"
            };

            //Change Invoice Status
            invoice.ReceivedAt = now;
            invoice.DeliveryStatus = InvoiceDeliveryStatus.Received;

            //Update DB
            await _context.UpdateLogs.AddAsync(updateLog);
            _context.Entry(invoice).State = EntityState.Modified;

            //Apply Changes
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Invoice marked as received."
            });
        }

        [HttpPost("{id}/process")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsOnProcess(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string username = User.FindFirst(ClaimTypes.Name)!.Value;

            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }


            if (invoice.Status != InvoiceStatus.Approved || invoice.DeliveryStatus != InvoiceDeliveryStatus.Received)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Only approved invoice and received can be marked as on process."
                });
            }

            //Create Review
            var now = DateTimeOffset.UtcNow;

            //Add UpdateLog
            var updateLog = new UpdateLog
            {
                EntityType = nameof(Invoice),
                EntityId = invoice.Id,
                DateTime = now,
                Description = $"Invoice updated to on process by {username}"
            };


            //Update DB
            invoice.Status = InvoiceStatus.OnProcess;
            await _context.UpdateLogs.AddAsync(updateLog);
            _context.Entry(invoice).State = EntityState.Modified;

            //Apply Changes
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Invoice marked as on process."
            });
        }

        [HttpPost("{id}/paid")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string username = User.FindFirst(ClaimTypes.Name)!.Value;

            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }


            if (invoice.Status != InvoiceStatus.OnProcess)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Only on process invoice can be marked as paid."
                });
            }

            //Create Review
            var now = DateTimeOffset.UtcNow;

            //Add UpdateLog
            var updateLog = new UpdateLog
            {
                EntityType = nameof(Invoice),
                EntityId = invoice.Id,
                DateTime = now,
                Description = $"Invoice updated to on paid by {username}"
            };


            //Update DB
            invoice.PaidAt = now;
            invoice.Status = InvoiceStatus.Paid;
            await _context.UpdateLogs.AddAsync(updateLog);
            _context.Entry(invoice).State = EntityState.Modified;

            //Apply Changes
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Invoice marked as paid."
            });
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
