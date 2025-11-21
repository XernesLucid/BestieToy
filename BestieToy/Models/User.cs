using System.ComponentModel.DataAnnotations;
using System.Data;
namespace BestieToy.Models
{
    public class User
    {
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // ĐÃ SỬA: Không còn "?"

        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        [Required]
        public string RoleId { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties - nullable
        public Roles? Role { get; set; }
        public Cart? Cart { get; set; }
        public List<Order>? Orders { get; set; }
    }
}