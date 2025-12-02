namespace BookingApi.Models;

public class ContactData

{
    public required string Email { get; set; }
    public required List<SocialLink> SocialLinks { get; set; }
}