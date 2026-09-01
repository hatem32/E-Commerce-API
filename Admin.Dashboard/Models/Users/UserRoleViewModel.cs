using AdminDashboard.Models.Roles;

namespace AdminDashboard.Models.Users
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public List<UpdatedRoleViewModel> Roles { get; set; }
    }
}
