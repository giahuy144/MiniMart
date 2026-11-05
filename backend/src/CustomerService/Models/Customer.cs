namespace CustomerService.Models;

public class Customer
{
    public int Id { get; set; }
    public Guid? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Points { get; set; } = 0;
    public decimal TotalSpent { get; set; } = 0;
    public string Tier { get; set; } = "Thường"; // Thường, Bạc, Vàng, Kim Cương
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();

    public void UpdateTier()
    {
        if (TotalSpent >= 10000000)
            Tier = "Kim Cương";
        else if (TotalSpent >= 5000000)
            Tier = "Vàng";
        else if (TotalSpent >= 1000000)
            Tier = "Bạc";
        else
            Tier = "Thường";
    }
}

public class PointHistory
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int PointsChanged { get; set; } // + or -
    public int PointsBalanceAfter { get; set; }
    public string TransactionType { get; set; } = "Earn"; // Earn, Redeem, Rollback
    public string ReferenceId { get; set; } = string.Empty; // OrderId
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
