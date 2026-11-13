using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.DB.Data;
using RaceDay.DB.Models;
using static RaceDay.DB.Models.Entities;
   
namespace RaceDay.DB.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict(new { message = "Email address is already registered." });

            var role = await _context.Roles.FindAsync(dto.RoleID);
            if (role == null) return BadRequest(new { message = "Invalid RoleID specified." });

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                RoleID = dto.RoleID,
                PhoneNumber = dto.PhoneNumber
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "User registered successfully.", userId = user.UserID });
        }

        /// <summary>Authenticates user credentials and establishes server session.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Unauthorized(new { message = "Invalid email or password." });

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid email or password." });

            // Store user session state
            HttpContext.Session.SetInt32("UserId", user.UserID);
            HttpContext.Session.SetString("UserRole", user.Role.RoleName);

            return Ok(new { message = "Login successful.", userId = user.UserID, role = user.Role.RoleName });
        }

        /// <summary>Logs out the current session.</summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }
    }

    public record RegisterDto(string FirstName, string LastName, string Email, string Password, int RoleID, string? PhoneNumber);
    public record LoginDto(string Email, string Password);
}
    

