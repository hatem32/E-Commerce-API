using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Contact;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace E_Commerce.API.Controllers
{
    // Public endpoint - anyone visiting the site can send a message, no login needed.
    public class ContactController : ApiBaseController
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IEmailService emailService, IOptions<EmailSettings> emailOptions, ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _emailSettings = emailOptions.Value;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Send(ContactMessageDto dto, CancellationToken ct)
        {
            var recipient = string.IsNullOrWhiteSpace(_emailSettings.RecipientEmail)
                ? _emailSettings.SenderEmail
                : _emailSettings.RecipientEmail;

            var subject = string.IsNullOrWhiteSpace(dto.Subject)
                ? $"New contact message from {dto.Name}"
                : $"[Contact] {dto.Subject}";

            var body = $"""
                <h3>New message from the Contact page</h3>
                <p><strong>Name:</strong> {WebUtility.HtmlEncode(dto.Name)}</p>
                <p><strong>Email:</strong> {WebUtility.HtmlEncode(dto.Email)}</p>
                <p><strong>Message:</strong></p>
                <p>{WebUtility.HtmlEncode(dto.Message).Replace("\n", "<br/>")}</p>
                """;

            try
            {
                await _emailService.SendAsync(recipient, subject, body, replyToEmail: dto.Email, ct);
                return Ok();
            }
            catch (Exception ex)
            {
                // TEMP: logging the full exception so we can see exactly why SMTP is failing.
                // Safe to remove/quiet down once email sending is confirmed working.
                _logger.LogError(ex, "Failed to send contact email");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Could not send your message right now. Please try again later or email us directly.");
            }
        }
    }
}