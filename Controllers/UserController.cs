using Microsoft.AspNetCore.Mvc;
using BookingApi.Data;
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;


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
    public IActionResult Register(User user)
    {
        // check if username already exists
        if (_context.Users.Any(u => u.Username == user.Username))
        {
            return BadRequest("Username already taken");
        }

        // check if email already exists and is valid
        if (_context.Users.Any(e => e.Email == user.Email))
        {
            return BadRequest("Email already registered");
        }

        if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
        {
            return BadRequest("Valid email is required");
        }

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
    public IActionResult Login([FromBody] User user)
    {
        var found = _context.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
        if (found == null) return Unauthorized("Invalid credentials");
        return Ok(new { found.Id, found.Username,found.Email, found.IsAdmin });
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