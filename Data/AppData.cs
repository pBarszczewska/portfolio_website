using BookingApi.Models;

namespace BookingApi.Data;

public static class AppData
{
    public static List<User> Users { get; set; } = new List<User>();
    public static List<Item> Items { get; set; } = new List<Item>();
    public static List<Booking> Bookings = new();
}