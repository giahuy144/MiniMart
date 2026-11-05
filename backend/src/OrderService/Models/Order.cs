namespace OrderService.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = "Khách vãng lai";
    public string CustomerPhone { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "System";
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PromotionCode { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled, Failed
    public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Refunded
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class OrderDetail
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}

public class Payment
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Banking, Card
    public decimal Amount { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string Status { get; set; } = "Success"; // Success, Failed
    public DateTime PaymentTime { get; set; } = DateTime.UtcNow;
}
