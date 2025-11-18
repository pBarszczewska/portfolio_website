namespace BookingApi.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = "unknown";
    public string Email { get; set; } = "unknown";
    public int ItemId { get; set; }
    public string ItemName { get; set; } = "unknown";
    public DateTime DateNow { get; set; }
    public DateTime StartDate { get; set; } 
    public DateTime EndDate { get; set; }   

}