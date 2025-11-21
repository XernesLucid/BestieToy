namespace BestieToy.Models
{
    public class Roles
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation property - nullable
        public List<User>? Users { get; set; }
    }
}
