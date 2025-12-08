namespace BookingApi.Models
{
    public class MailjetSettings
    {
        public required string MailjetApiKey { get; set; }
        public required string MailjetApiSecret { get; set; }
        public required string SenderEmail { get; set; }
        public required string SenderName { get; set; }
    }
}
