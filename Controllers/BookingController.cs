using Microsoft.AspNetCore.Mvc;
using BookingApi.Data;
using BookingApi.Models;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly BookingContext _context;
    private readonly EmailService _emailService;

    public BookingController(BookingContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }


    [HttpPost]
    public async Task<IActionResult> Book(
        [FromQuery] string startDate,
        [FromQuery] int? durationHours = null,
        [FromQuery] bool wholeDay = false,
        [FromQuery] string? username = null,
        [FromQuery] string? email = null,
        [FromQuery] string? itemname = null)
    {
        if (string.IsNullOrWhiteSpace(username)) return BadRequest("Username is required");
        if (string.IsNullOrWhiteSpace(itemname)) return BadRequest("Item name is required");

        // Look up the user
        var user = _context.Users.FirstOrDefault(u => u.Username.ToLower().Trim() == username.ToLower().Trim());

        // this cannot be used in Postgres
        // var user = _context.Users.FirstOrDefault(u => u.Username.Trim().Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
        if (user == null) return NotFound($"User '{username}' not found");
        if (email == null) return NotFound($"Email '{email}' not found");

        // look up for item
        var item = _context.Items.FirstOrDefault(i => i.Name.ToLower().Trim() == itemname.ToLower().Trim());
        if (item == null) return NotFound($"Item '{itemname}' not found");
        if (!item.IsAvailable) return BadRequest($"Item '{itemname}' already booked");

        DateTime startLocal = DateTime.Parse(startDate);
        DateTime startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, TimeZoneInfo.Local);
        // Parse and convert to UTC
        //var startUtc = DateTime.SpecifyKind(startDateStr, DateTimeKind.Local).ToUniversalTime();

        // Determine end time
        DateTime endUtc;
        if (wholeDay)
        {
            // Book for the rest of the day (till 23:59)
            endUtc = new DateTime(startUtc.Year, startUtc.Month, startUtc.Day, 23, 59, 59, DateTimeKind.Utc);

        }
        else
        {
            // Default or custom duration in hours (minimum 1h)
            var hours = durationHours.HasValue && durationHours.Value > 0 ? durationHours.Value : 1;
            endUtc = startUtc.AddHours(hours);
        }

        if (startUtc == default || endUtc == default)
        {
            return BadRequest("Invalid start or end date.");
        }


        // Check for overlapping bookings
        var now = DateTime.UtcNow;

        if (_context.Bookings.Any(b => b.ItemId == item.Id && b.StartDate < endUtc && startUtc < b.EndDate && b.EndDate > now))
        {
            return BadRequest($"Item '{item.Name}' is already booked for that time range.");
        }


        var booking = new Booking
        {
            //Id = AppData.Bookings.Count + 1,
            // EF will auto-generate Id if you set it as primary key with identity
            UserId = user.Id,
            ItemId = item.Id,
            UserName = user.Username,
            ItemName = item.Name,
            Email = email,
            StartDate = startUtc,
            EndDate = endUtc,
            DateNow = DateTime.UtcNow
        };

        // Save to DB
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

         // send confirmation email
        await _emailService.SendBookingConfirmationAsync(email, user.Username, item.Name, startUtc, endUtc);

        return Ok(booking);
    }

    [HttpGet("user/{username}")]
    public IActionResult GetUserBookings(string username)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        if (user == null) return NotFound();

        var bookings = _context.Bookings
            .Where(b => b.UserId == user.Id)
            .Select(b => new { b.Id, b.ItemName, b.StartDate })
            .ToList();

        return Ok(bookings);
    }


    [HttpDelete("cancel")]
    public IActionResult Cancel(int? id = null, string? username = null)
    {
        if (id == null && string.IsNullOrWhiteSpace(username))
        {
            return BadRequest("You must provide either a booking Id or a username.");
        }

        Booking? booking = null;

        if (id.HasValue)
        {
            booking = _context.Bookings.FirstOrDefault(b => b.Id == id.Value);
            if (booking == null) return NotFound($"Booking with id {id} not found");
        }
        else if (!string.IsNullOrWhiteSpace(username))
        {
            var bookings = _context.Bookings.Where(b => b.UserName.ToLower() == username.ToLower()).ToList();
            if (!bookings.Any()) return NotFound($"No bookings found for user '{username}'");
            if (bookings.Count > 1)
            {
                // Return list of bookings to choose from
                return Ok(new
                {
                    Message = $"User '{username}' has multiple bookings. Please specify the booking ID to cancel.",
                    Bookings = bookings.Select(b => new
                    {
                        b.Id,
                        b.ItemName,
                        b.StartDate
                    })
                });
            }
            booking = bookings.First();
        }

        if (booking == null) return NotFound("Booking not found");

        var item = _context.Items.FirstOrDefault(i => i.Id == booking.ItemId);
        if (item != null) item.IsAvailable = true;

        _context.Bookings.Remove(booking);
        _context.SaveChanges();

        return Ok($"Booking cancelled successfully (Id: {booking.Id}, User: {booking.UserName})");
    }
        
    [HttpPut("change")]
    public IActionResult ChangeBooking(int bookingId, string itemname, string startDateStr, double? durationHours, bool? wholeDay)
{
    var booking = _context.Bookings.FirstOrDefault(b => b.Id == bookingId);
    if (booking == null) return NotFound("Booking not found.");

    var item = _context.Items.FirstOrDefault(i => i.Name == itemname);
    if (item == null) return NotFound("Item not found.");

    var startUtc = DateTime.Parse(startDateStr).ToUniversalTime();
    var duration = wholeDay == true ? TimeSpan.FromHours(24) :
                   TimeSpan.FromHours(durationHours ?? 1);
    var endUtc = startUtc.Add(duration);

    // overlap check
    bool overlap = _context.Bookings.Any(b =>
        b.ItemId == item.Id &&
        b.Id != booking.Id && // Exclude itself
        b.StartDate < endUtc &&
        startUtc < b.EndDate
    );

    if (overlap)
        return BadRequest($"Item '{item.Name}' is already booked for that time range.");

    // Update booking
    booking.ItemId = item.Id;
    booking.ItemName = item.Name;
    booking.StartDate = startUtc;
    booking.EndDate = endUtc;
    _context.SaveChanges();

    return Ok("Booking updated.");
}


    [HttpGet]
    public IActionResult GetBookings() => Ok(_context.Bookings);
}