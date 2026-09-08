using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Application.DTOs.Contact
{
    public class ContactMessageDto
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [StringLength(200)]
        public string? Subject { get; set; }

        [Required, StringLength(3000)]
        public string Message { get; set; } = default!;
    }
}