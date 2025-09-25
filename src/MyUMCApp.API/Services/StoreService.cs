using Microsoft.EntityFrameworkCore;
using MyUMCApp.API.Data;
using MyUMCApp.API.Models;

namespace MyUMCApp.API.Services;

public class StoreService : IStoreService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StoreService> _logger;

    public StoreService(ApplicationDbContext context, ILogger<StoreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.IsAvailable = true;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new product with ID: {ProductId}", product.Id);
        return product;
    }

    public async Task<ProductVariant> AddProductVariantAsync(ProductVariant variant)
    {
        var product = await _context.Products.FindAsync(variant.ProductId);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {variant.ProductId} not found");
        }

        variant.Id = Guid.NewGuid();
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added variant {VariantId} to product {ProductId}", variant.Id, variant.ProductId);
        return variant;
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Variants)
            .Where(p => p.IsFeatured && p.IsAvailable)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, string? category = null)
    {
        var query = _context.Products
            .Include(p => p.Variants)
            .Where(p => p.IsAvailable);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || 
                                   p.Description.Contains(searchTerm) ||
                                   p.SKU.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Cart> CreateCartAsync(string userId)
    {
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new cart {CartId} for user {UserId}", cart.Id, userId);
        return cart;
    }

    public async Task AddToCartAsync(Guid cartId, CartItem item)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId);

        if (cart == null)
        {
            throw new KeyNotFoundException($"Cart with ID {cartId} not found");
        }

        var product = await _context.Products.FindAsync(item.ProductId);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {item.ProductId} not found");
        }

        if (!product.IsAvailable)
        {
            throw new InvalidOperationException("Product is not available");
        }

        // Check if product variant exists (if specified)
        if (item.ProductVariantId.HasValue)
        {
            var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId.Value);
            if (variant == null)
            {
                throw new KeyNotFoundException($"Product variant with ID {item.ProductVariantId} not found");
            }

            if (variant.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock for variant");
            }
        }
        else if (product.StockQuantity < item.Quantity)
        {
            throw new InvalidOperationException("Insufficient stock for product");
        }

        // Check if item already exists in cart
        var existingItem = cart.Items.FirstOrDefault(i => 
            i.ProductId == item.ProductId && 
            i.ProductVariantId == item.ProductVariantId);

        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            item.Id = Guid.NewGuid();
            item.CartId = cartId;
            _context.CartItems.Add(item);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added {Quantity} of product {ProductId} to cart {CartId}", 
            item.Quantity, item.ProductId, cartId);
    }

    public async Task<Order> CreateOrderAsync(Guid cartId, string shippingAddress, string billingAddress, PaymentMethod paymentMethod)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.Id == cartId);

        if (cart == null)
        {
            throw new KeyNotFoundException($"Cart with ID {cartId} not found");
        }

        if (!cart.Items.Any())
        {
            throw new InvalidOperationException("Cart is empty");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = await GenerateOrderNumberAsync(),
            UserId = cart.UserId,
            Status = OrderStatus.Pending,
            PaymentMethod = paymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        decimal subtotal = 0;
        foreach (var cartItem in cart.Items)
        {
            var unitPrice = cartItem.ProductVariant?.Price ?? cartItem.Product!.Price;
            var total = unitPrice * cartItem.Quantity;

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                ProductVariantId = cartItem.ProductVariantId,
                Quantity = cartItem.Quantity,
                UnitPrice = unitPrice,
                Total = total
            };

            order.Items.Add(orderItem);
            subtotal += total;
        }

        order.SubTotal = subtotal;
        order.ShippingCost = CalculateShippingCost(subtotal);
        order.Tax = CalculateTax(subtotal);
        order.Total = order.SubTotal + order.ShippingCost + order.Tax;

        _context.Orders.Add(order);

        // Clear the cart
        _context.CartItems.RemoveRange(cart.Items);
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created order {OrderNumber} from cart {CartId} with total {Total}", 
            order.OrderNumber, cartId, order.Total);

        return order;
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found");
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated order {OrderId} status to {Status}", orderId, status);
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.Orders.CountAsync(o => o.OrderNumber.StartsWith($"ORD-{date}"));
        return $"ORD-{date}-{(count + 1):D4}";
    }

    private static decimal CalculateShippingCost(decimal subtotal)
    {
        // Simple shipping calculation - free shipping over $50
        return subtotal >= 50 ? 0 : 5.00m;
    }

    private static decimal CalculateTax(decimal subtotal)
    {
        // Simple tax calculation - 10% tax
        return subtotal * 0.10m;
    }
}