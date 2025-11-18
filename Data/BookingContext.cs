using Microsoft.EntityFrameworkCore;
using BookingApi.Models;

namespace BookingApi.Data
{
    public class BookingContext : DbContext
    {
        public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }

        public DbSet<Item> Items { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Booking> Bookings { get; set; }
    }
    
}