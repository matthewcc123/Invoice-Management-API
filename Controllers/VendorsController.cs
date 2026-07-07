using AutoMapper;
using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null)
            {
                return NotFound(new ApiResponse<VendorResponse>
                {
                    Success = false,
                    Message = "Vendor not found."
                });
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(VendorRequest vendor)
        {
            var newVendor = _mapper.Map<Vendor>(vendor);

            _context.Vendors.Add(newVendor);
            await _context.SaveChangesAsync();

            var vendorResponse = _mapper.Map<VendorResponse>(newVendor);
            return CreatedAtAction(nameof(Get), new { id = newVendor.Id }, new ApiResponse<VendorResponse>
            {
                Success = true,
                Message = "Vendor created successfully.",
                Data = vendorResponse
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, VendorRequest vendorRequest)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"Vendor with ID {id} not found."
                });
            }

            var updatedVendor = _mapper.Map(vendorRequest, vendor);
            _context.Entry(updatedVendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Vendors.Any(v => v.Id == id))
                {
                    return NotFound(new ApiResponse
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

            var vendorResponse = _mapper.Map<VendorResponse>(updatedVendor);

            return Ok(new ApiResponse<VendorResponse>
            {
                Success = true,
                Message = "Vendor updated successfully.",
                Data = vendorResponse
            });
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"Vendor with ID {id} not found."
                });
            }

            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Vendor deleted successfully."
            });
        }

    }
}
