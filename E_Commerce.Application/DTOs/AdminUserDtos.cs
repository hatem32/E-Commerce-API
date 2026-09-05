namespace E_Commerce.API.DTOs
{
    public class AdminUserDto
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class AdminRoleDto
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
    }

    public class CreateRoleDto
    {
        public string Name { get; set; } = default!;
    }

    // Full desired set of role names for the user - the endpoint adds/removes to match this.
    public class UpdateUserRolesDto
    {
        public IList<string> Roles { get; set; } = new List<string>();
    }
}