namespace E_Commerce.Application.Common
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = default!;
        public int SmtpPort { get; set; }
        public bool UseSsl { get; set; } = true;

        // The mailbox that logs in and actually sends the message.
        public string SenderEmail { get; set; } = default!;
        public string SenderPassword { get; set; } = default!;
        public string SenderName { get; set; } = "E-Shop";

        // Where "Contact Us" messages get delivered - your store's inbox.
        // Defaults to SenderEmail if left blank.
        public string? RecipientEmail { get; set; }
    }
}