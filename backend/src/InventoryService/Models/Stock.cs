namespace InventoryService.Models;

public class Stock
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinStockLevel { get; set; } = 10;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public bool IsLowStock => Quantity <= MinStockLevel;
}
