using AutoMapper;
using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;

namespace InvoiceManagement.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UsersController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            var usersDto = _mapper.Map<List<UserResponse>>(users);

            return Ok(new ApiResponse<List<UserResponse>>
            { 
                Success = true,
                Message = "Users retrieved successfully.",
                Data = usersDto
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"User id {id} not found."
                });
            }

            var userDto = _mapper.Map<UserResponse>(user);
            return Ok(new ApiResponse<UserResponse>
            {
                Success = true,
                Message = $"User id {id} retrieved successfully.",
                Data = userDto
            });
        }

        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, UserUpdateRequest userUpdateRequest)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"User id {id} not found."
                });
            }

            var updatedUser = _mapper.Map(userUpdateRequest, user);
            _context.Entry(updatedUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(u => u.Id == id))
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = $"User with ID {id} not found."
                    });
                }
                else
                {
                    throw;
                }
            }

            var userResponse = _mapper.Map<UserResponse>(updatedUser);
            return Ok(new ApiResponse<UserResponse>
            {
                Success = true,
                Message = $"User updated successfully.",
                Data = userResponse
            });
        }

        [HttpPost("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, UserDeleteRequest userDeleteRequest)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"User id {id} not found."
                });
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(userDeleteRequest.Password, user.Password);

            if (!passwordValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"Invalid password."
                });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"User id {id} deleted successfully."
            });
        }

    }
}
