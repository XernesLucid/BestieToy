namespace BestieToy.Models
{
    public class CartItem
    {
        public string CartItemId { get; set; } = string.Empty;
        public string CartId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Navigation properties - nullable
        public Cart? Cart { get; set; }
        public Product? Product { get; set; }
    }
}
