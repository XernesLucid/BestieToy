namespace BestieToy.Models
{
    public class Order
    {
        public string OrderId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }

        // Navigation properties - nullable
        public User? User { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
