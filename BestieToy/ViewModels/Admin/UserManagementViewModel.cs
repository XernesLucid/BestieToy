using System.Data;
using BestieToy.Models;

namespace BestieToy.ViewModels.Admin
{
    public class UserManagementViewModel
    {
        public IEnumerable<User> Users { get; set; } = new List<User>();
        public IEnumerable<Roles> Roles { get; set; } = new List<Roles>();

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalUsers { get; set; } = 0;
        public int PageSize { get; set; } = 20;

        // Filter
        public string? SearchKeyword { get; set; }
        public string? RoleId { get; set; }
        public bool? IsActive { get; set; }

        // Stats
        public int TotalAdmins { get; set; }
        public int TotalStaff { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalActiveUsers { get; set; }
    }
}