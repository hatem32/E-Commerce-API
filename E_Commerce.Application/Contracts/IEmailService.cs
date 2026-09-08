namespace E_Commerce.Application.Contracts
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, string? replyToEmail = null, CancellationToken ct = default);
    }
}