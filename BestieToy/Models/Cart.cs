namespace BestieToy.Models
{
    public class Cart
    {
        public string CartId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties - nullable
        public User? User { get; set; }
        public List<CartItem>? CartItems { get; set; }
    }
}
