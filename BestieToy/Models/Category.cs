namespace BestieToy.Models
{
    public class Category
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PetType { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation property - nullable
        public List<Product>? Products { get; set; }
    }
}
