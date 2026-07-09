using AutoMapper;
using BCrypt.Net;
using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.DTOs;
using InvoiceManagement.Api.Models;
using InvoiceManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace InvoiceManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, IMapper mapper, JwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AuthRegisterRequest registerRequest)
        {
            bool userExists = await _context.Users.AnyAsync(user => user.Email == registerRequest.Email);

            if (userExists)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "User already registered."
                });
            }

            var newUser = _mapper.Map(registerRequest, new User());
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
            newUser.Role = Enum.UserRole.Vendor;

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var userResponse = _mapper.Map<UserResponse>(newUser);

            return Ok(new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "Registration successful.",
                Data = userResponse
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthLoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == loginRequest.Email);

            if (user == null)
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid email or password."
                });
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password);

            if (!passwordValid)
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid email or password."
                });
            }


            return Ok(new ApiResponse<AuthLoginResponse>
            {
                Success = true,
                Message = "Login successful.",
                Data = new AuthLoginResponse
                {
                    //Generate Token
                    Token = _jwtService.GenerateToken(user),
                }
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Logout successful."
            });
        }

        [HttpPut("change-password/{id}")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, AuthChangePasswordRequest changePasswordRequest)
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

            bool passwordValid = BCrypt.Net.BCrypt.Verify(changePasswordRequest.OldPassword, user.Password);

            if (!passwordValid)
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid password."
                });
            }

            var updatedUser = user;
            updatedUser.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);
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

            return Ok(new ApiResponse<AuthLoginResponse>
            {
                Success = true,
                Message = "Password updated successfully.",
                Data = new AuthLoginResponse
                {
                    //Generate Token
                    Token = _jwtService.GenerateToken(user),
                }
            });


        }

        [HttpPut("update-role/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateRole(int id, AuthUpdateRoleRequest updateRoleRequest)
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

            var updatedUser = _mapper.Map(updateRoleRequest, user);
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
                Message = $"Role updated successfully.",
                Data = userResponse
            });
        }


    }
}
