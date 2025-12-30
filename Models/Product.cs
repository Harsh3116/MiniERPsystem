namespace MiniERPsystem.Models
{
    public class Product
    {
        public int Id { get; set; }
        public bool IsLowStock => StockQuantity <= 5;
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}