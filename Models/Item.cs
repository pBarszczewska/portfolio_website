namespace BookingApi.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsAvailable { get; set; } = true;
}