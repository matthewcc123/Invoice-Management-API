using AutoMapper;
using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagement.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VendorsController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public VendorsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vendors = await _context.Vendors.ToListAsync();
            var vendorDtos = _mapper.Map<List<VendorResponse>>(vendors);
            return Ok(new ApiResponse<List<VendorResponse>>
            {
                Success = true,
                Message = "Vendors retrieved successfully.",
                Data = vendorDtos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null)
            {
                return NotFound();
            }

            var vendorDto = _mapper.Map<VendorResponse>(vendor);
            return Ok(new ApiResponse<VendorResponse>
            {
                Success = true,
                Message = "Vendor retrieved successfully.",
                Data = vendorDto
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(VendorRequest vendor)
        {
            var newVendor = _mapper.Map<Vendor>(vendor);

            _context.Vendors.Add(newVendor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newVendor.Id }, new ApiResponse<Vendor>
            {
                Success = true,
                Message = "Vendor created successfully.",
                Data = newVendor
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, VendorRequest vendorDto)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null)
            {
                return BadRequest(new ApiResponse<Vendor>
                {
                    Success = false,
                    Message = $"Vendor with ID {id} not found."
                });
            }

            var updatedVendor = _mapper.Map(vendorDto, vendor);
            _context.Entry(updatedVendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Vendors.Any(v => v.Id == id))
                {
                    return NotFound(new ApiResponse<Vendor>
                    {
                        Success = false,
                        Message = $"Vendor with ID {id} not found."
                    });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new ApiResponse<Vendor>
            {
                Success = true,
                Message = "Vendor updated successfully.",
                Data = updatedVendor
            });
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }

            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<Vendor>
            {
                Success = true,
                Message = "Vendor deleted successfully."
            });
        }

    }
}
