namespace InventoryService.Models;

public class ImportReceipt
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Confirmed";
    public string Note { get; set; } = string.Empty;

    public ICollection<ImportReceiptDetail> Details { get; set; } = new List<ImportReceiptDetail>();
}

public class ImportReceiptDetail
{
    public int Id { get; set; }
    public int ImportReceiptId { get; set; }
    public ImportReceipt? ImportReceipt { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
}
