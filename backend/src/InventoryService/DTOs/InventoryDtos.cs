using System.ComponentModel.DataAnnotations;

namespace InventoryService.DTOs;

public class StockDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinStockLevel { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class DeductStockItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class DeductStockRequestDto
{
    [Required]
    public string OrderId { get; set; } = string.Empty;

    [Required]
    public List<DeductStockItemDto> Items { get; set; } = new();

    public string PerformedBy { get; set; } = "OrderService";
}

public class RollbackStockRequestDto
{
    [Required]
    public string OrderId { get; set; } = string.Empty;

    [Required]
    public List<DeductStockItemDto> Items { get; set; } = new();

    public string Reason { get; set; } = "Order Cancelled / Flow Failed";
}

public class CreateImportReceiptDetailDto
{
    [Required]
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ImportPrice { get; set; }
}

public class CreateImportReceiptDto
{
    public int? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "Thủ Kho";
    public string Note { get; set; } = string.Empty;

    [Required]
    public List<CreateImportReceiptDetailDto> Items { get; set; } = new();
}

public class ImportReceiptDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public List<ImportReceiptDetailDto> Details { get; set; } = new();
}

public class ImportReceiptDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
}
