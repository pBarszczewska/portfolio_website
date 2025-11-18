using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailService
{
    private readonly string? _apiKey;
    private readonly string? _senderEmail;
    private readonly string? _senderName;

    public EmailService(IConfiguration config)
    {
        _apiKey = config["EmailSettings:SendGridApiKey"];
        _senderEmail = config["EmailSettings:SenderEmail"];
        _senderName = config["EmailSettings:SenderName"];
    }

    public async Task SendBookingConfirmationAsync(string toEmail, string username, string itemName, DateTime startDate, DateTime endDate)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_senderEmail, _senderName);
        var to = new EmailAddress(toEmail, username);
        var subject = "Paulina Barszczewska - Meeting Confirmation";

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

        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
        var response = await client.SendEmailAsync(msg);

        Console.WriteLine($"SendGrid response: {response.StatusCode}");
        var responseBody = await response.Body.ReadAsStringAsync();
        Console.WriteLine($"SendGrid body: {responseBody}");

    }
}
