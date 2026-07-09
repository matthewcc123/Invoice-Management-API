using AutoMapper;
using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Extensions;
using InvoiceManagement.Api.Migrations;
using InvoiceManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace InvoiceManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> GetAll([FromQuery] UserQuery query)
        {

            var userQuery = _context.Users.AsQueryable();
            int totalRecords = _context.Users.Count();

            userQuery = userQuery.ApplySearch(query.Search, x => x.Username, x => x.Email);
            userQuery = userQuery.ApplySort(query.SortBy ?? nameof(Vendor.Id), query.Order);
            userQuery = userQuery.ApplyPagination(query.PageNumber, query.PageSize);

            var users = await userQuery.ToListAsync();
            var usersDto = _mapper.Map<List<UserResponse>>(users);

            var pagedUsers = new PagedResponse<UserResponse>(usersDto, query.PageNumber, query.PageSize, totalRecords);

            return Ok(new ApiResponse<PagedResponse<UserResponse>>
            { 
                Success = true,
                Message = "Users retrieved successfully.",
                Data = pagedUsers
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userId != id && !User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = $"You're not authorized to access this user."
                });
            }

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

        [HttpPut("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, UserEditRequest userEditRequest)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userId != id && !User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = $"You're not authorized to edit this user."
                });
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"User id {id} not found."
                });
            }

            var updatedUser = _mapper.Map(userEditRequest, user);
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

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, UserDeleteRequest userDeleteRequest)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userId != id && !User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = $"You're not authorized to delete this user."
                });
            }

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
