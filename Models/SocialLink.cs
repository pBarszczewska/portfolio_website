
namespace BookingApi.Models;
public class SocialLink
{
    public int Id { get; set; }
    public string Platform { get; set; } = "";
    public string Url { get; set; }  = "";
    public string IconClass { get; set; }  = "";
    
}