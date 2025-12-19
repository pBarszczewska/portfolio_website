using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;


public class EmailService
{
    private readonly string? _apiKey;
    private readonly string? _apiSecret;
    private readonly string? _senderEmail;
    private readonly string? _senderName;

    public EmailService(IConfiguration config)
    {
        _apiKey = config["EmailSettings:ApiKey"];
        _apiSecret = config["EmailSettings:ApiSecret"];
        _senderEmail = config["EmailSettings:SenderEmail"];
        _senderName = config["EmailSettings:SenderName"];
    }

    public async Task SendBookingConfirmationAsync(string toEmail, string username, string itemName, DateTime startDate, DateTime endDate)
    {
        var client = new MailjetClient(_apiKey, _apiSecret);

        // Generate Outlook calendar link
        var outlookUrl =
        "https://outlook.live.com/calendar/0/deeplink/compose?" +
        $"path=/calendar/action/compose&rru=addevent" +
        $"&subject={Uri.EscapeDataString($"Booking: {itemName}")}" +
        $"&body={Uri.EscapeDataString("Your booking was confirmed!")}" +
        $"&startdt={startDate.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}" +
        $"&enddt={endDate.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}" +
        $"&location={Uri.EscapeDataString("Online / Teams meeting")}";


        var htmlContent = $@"
            <h2>Hello {username},</h2>
            <p>Thank you for organising a meeting with me.</p>
            <p>Your booking for <strong>{itemName}</strong> was confirmed!</p>
            <p>Please save the following dates:</p>
            <p>From: {startDate}<br/>To: {endDate}</p>
            <p><a href='{outlookUrl}' target='_blank'>📅 Add to Outlook / Teams Calendar</a></p>
            <p>I am looking forward to speak with you!</p>
            <p> </p>
            <p>Regards,</p>
            <p><strong>Paulina Barszczewska</strong></p>
        ";

        var request = new MailjetRequest
        {
            Resource = Send.Resource
        }
        .Property("FromEmail", _senderEmail)
        .Property("FromName", _senderName)
        .Property("Subject", "Meeting Confirmation")
        .Property("Html-part", htmlContent)
        .Property("Recipients", new JArray {
            new JObject {
                { "Email", toEmail },
                { "Name", username }
            },
            new JObject {
                {"Email", _senderEmail}, // email to sender
                {"Name", _senderName}
            }
        });

        MailjetResponse response = await client.PostAsync(request);

        Console.WriteLine($"Mailjet response: {response.StatusCode}");
        Console.WriteLine($"Mailjet body: {response.GetData()}");
    }
}
