namespace BestieToy.Models
{
    public class OrderDetail
    {
        public string OrderDetailId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation properties - nullable
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
