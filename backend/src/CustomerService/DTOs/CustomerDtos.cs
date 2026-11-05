using System.ComponentModel.DataAnnotations;

namespace CustomerService.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public Guid? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Points { get; set; }
    public decimal TotalSpent { get; set; }
    public string Tier { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCustomerDto
{
    public Guid? UserId { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class UpdateCustomerDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class AddPointsRequestDto
{
    [Required]
    public string OrderId { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal OrderTotal { get; set; }

    public int? CustomPoints { get; set; }
}

public class DeductPointsRequestDto
{
    [Required]
    public string OrderId { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int PointsToDeduct { get; set; }
}

public class RollbackPointsRequestDto
{
    [Required]
    public string OrderId { get; set; } = string.Empty;

    public string Reason { get; set; } = "Order Cancelled";
}
