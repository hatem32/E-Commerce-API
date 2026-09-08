using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Common
{
    public sealed class IdentityUserResult
    {
        public IdentityUserResult(string id, string? email, string? userName, string displayName)
        {
            Id = id;
            Email = email;
            UserName = userName;
            DisplayName = displayName;
        }

        public string Id { get; set; } = default!;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string DisplayName { get; set; } = default!;
        public bool EmailConfirmed { get; set; }
    }
}