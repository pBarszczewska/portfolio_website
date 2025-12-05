using Microsoft.AspNetCore.Mvc;
using BookingApi.Data;
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemController : ControllerBase
{
    private readonly BookingContext _context;

    public ItemController(BookingContext context)
    {
        _context = context;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Item>>> GetAll()
    {
        return await _context.Items.ToListAsync();
    }


    [HttpPost("add")]
    public async Task<ActionResult<Item>> Create(Item item)
    {
        if (string.IsNullOrWhiteSpace(item.Name)) return BadRequest("Item name required.");
        // prevent duplicates
        if (_context.Items.Any(i => i.Name == item.Name)) return BadRequest("An item with that name already exists.");
        
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = item.Id }, item);
    }

    [HttpDelete("delete")]
    public IActionResult DeleteItem(int id)
    {
        var item = _context.Items.Find(id);
        if (item == null) return NotFound("Please select an item.");
        _context.Items.Remove(item);
        _context.SaveChanges();
        return Ok($"Item '{item.Name}' deleted.");
    }


    [HttpPut("update")]
    public IActionResult UpdateItem(int id, string name)
    {
        var item = _context.Items.FirstOrDefault(i => i.Id == id);
        if (item == null) return NotFound("Item not found.");

        item.Name = name;
        _context.SaveChanges();

        return Ok("Item updated successfully.");
    }


}