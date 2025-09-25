namespace MyUMCApp.API.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Images { get; set; } = string.Empty; // JSON string for simplicity
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
    public List<ProductVariant> Variants { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Attributes { get; set; } = string.Empty; // JSON string for simplicity
}

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class Cart
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public List<CartItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int Quantity { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}