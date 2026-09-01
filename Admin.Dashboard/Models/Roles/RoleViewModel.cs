using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models.Roles
{
    public class RoleViewModel
    {
        [Required(ErrorMessage = "Role Name Is Required")]
        [StringLength(256, ErrorMessage = "Role Name Length Can not Be More Than 256")]
        public string Name { get; set; }
    }
}
