using Microsoft.AspNetCore.Mvc;
using BookingApi.Data;
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly BookingContext _context;

    public UserController(BookingContext context)
    {
        _context = context;
    }


    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
    {

        // check if username already exists
        if (_context.Users.Any(u => u.Username == dto.Username))
        return BadRequest("Username already taken");

        // check if email already exists and is valid
        if (_context.Users.Any(e => e.Email == dto.Email))
        return BadRequest("Email already registered");

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
        return BadRequest("Valid email is required");

         var user = new User
        {
            Username = dto.Username,
            Email = dto.Email
        };
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(user);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll()
    {
        return await _context.Users.ToListAsync();
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);

        if (user == null)
        return Unauthorized("Invalid credentials");

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password
        );

        if (result == PasswordVerificationResult.Failed)
        return Unauthorized("Invalid credentials");
        
        return Ok(new { user.Id, user.Username,user.Email, user.IsAdmin });
    }

    [HttpGet("userBookings")]
    public IActionResult GetUserBookings(string username)
    {
        var bookings = _context.Bookings
            .Where(b => b.UserName.ToLower() == username.ToLower())
            .Select(b => new { b.Id, b.ItemName, b.StartDate })
            .ToList();

        if (!bookings.Any())
            return Ok($"User '{username}' has no bookings.");

        return Ok(bookings);
    }
    
    [HttpDelete("delete")]
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound("Please select an user.");
        _context.Users.Remove(user);
        _context.SaveChanges();
        return Ok($"Item '{user.Username}' deleted.");
    }
}