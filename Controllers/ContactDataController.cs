using BookingApi.Models;
using Microsoft.AspNetCore.Mvc;
using BookingApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialLinksController(IConfiguration configuration) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;

    [HttpGet]
    public IActionResult GetContactData()
    {
        var email = _configuration["EmailSettings:SenderEmail"];
        
        var contactData = new ContactData
        {
            Email = email,
            SocialLinks = new List<SocialLink>
            {
                new SocialLink 
                { 
                    Id = 1, 
                    Platform = "LinkedIn", 
                    Url = "https://linkedin.com/in/paulina-barszczewska-938875131",
                    IconClass = "fab fa-linkedin"
                },
                new SocialLink 
                { 
                    Id = 2, 
                    Platform = "GitHub", 
                    Url = "https://github.com/pBarszczewska",
                    IconClass = "fab fa-github"
                }
            }
        };
        
        return Ok(contactData);
    }
}