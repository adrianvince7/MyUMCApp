using Microsoft.Extensions.Logging;
using MyUMCApp.Shared.Data;
using MyUMCApp.Shared.Models;

namespace MyUMCApp.Store.API.Services;

public interface IStoreService
{
    Task<Product> CreateProductAsync(Product product);
    Task<ProductVariant> AddProductVariantAsync(ProductVariant variant);
    Task<Cart> CreateCartAsync(Guid userId);
    Task AddToCartAsync(Guid cartId, CartItem item);
    Task<Order> CreateOrderAsync(Guid cartId, string shippingAddress, string billingAddress, PaymentMethod paymentMethod);
    Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync();
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, string? category = null);
}

public class StoreService : IStoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StoreService> _logger;

    public StoreService(IUnitOfWork unitOfWork, ILogger<StoreService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        try
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            
            var createdProduct = await _unitOfWork.Repository<Product>().AddAsync(product);
            _logger.LogInformation("Created new product with ID: {ProductId}", createdProduct.Id);
            
            return createdProduct;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    public async Task<ProductVariant> AddProductVariantAsync(ProductVariant variant)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(variant.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {variant.ProductId} not found");
            }

            var createdVariant = await _unitOfWork.Repository<ProductVariant>().AddAsync(variant);
            
            product.Variants.Add(createdVariant);
            await _unitOfWork.Repository<Product>().UpdateAsync(product);
            
            _logger.LogInformation("Added variant with ID: {VariantId} to product: {ProductId}", 
                createdVariant.Id, variant.ProductId);
            
            return createdVariant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding variant to product: {ProductId}", variant.ProductId);
            throw;
        }
    }

    public async Task<Cart> CreateCartAsync(Guid userId)
    {
        try
        {
            var cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var createdCart = await _unitOfWork.Repository<Cart>().AddAsync(cart);
            _logger.LogInformation("Created new cart with ID: {CartId} for user: {UserId}", 
                createdCart.Id, userId);
            
            return createdCart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cart for user: {UserId}", userId);
            throw;
        }
    }

    public async Task AddToCartAsync(Guid cartId, CartItem item)
    {
        try
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(cartId);
            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart with ID {cartId} not found");
            }

            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {item.ProductId} not found");
            }

            if (item.ProductVariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
                if (variant == null)
                {
                    throw new KeyNotFoundException($"Product variant with ID {item.ProductVariantId} not found");
                }

                if (variant.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for variant {variant.Name}");
                }
            }
            else
            {
                if (product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
                }
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
                cart.Items.Add(item);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Cart>().UpdateAsync(cart);
            
            _logger.LogInformation("Added item to cart: {CartId}", cartId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart: {CartId}", cartId);
            throw;
        }
    }

    public async Task<Order> CreateOrderAsync(Guid cartId, string shippingAddress, string billingAddress, PaymentMethod paymentMethod)
    {
        try
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(cartId);
            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart with ID {cartId} not found");
            }

            if (!cart.Items.Any())
            {
                throw new InvalidOperationException("Cannot create order with empty cart");
            }

            var order = new Order
            {
                UserId = cart.UserId,
                OrderNumber = GenerateOrderNumber(),
                Status = OrderStatus.Pending,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            decimal subTotal = 0;
            foreach (var cartItem in cart.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(cartItem.ProductId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {cartItem.ProductId} not found");
                }

                decimal unitPrice = cartItem.ProductVariantId.HasValue
                    ? product.Variants.First(v => v.Id == cartItem.ProductVariantId).Price
                    : product.Price;

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductVariantId = cartItem.ProductVariantId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    Total = unitPrice * cartItem.Quantity
                };

                order.Items.Add(orderItem);
                subTotal += orderItem.Total;
            }

            order.SubTotal = subTotal;
            order.Tax = subTotal * 0.15m; // 15% tax
            order.ShippingCost = subTotal > 1000 ? 0 : 50; // Free shipping over $1000
            order.Total = order.SubTotal + order.Tax + order.ShippingCost;

            var createdOrder = await _unitOfWork.Repository<Order>().AddAsync(order);

            // Clear the cart
            await _unitOfWork.Repository<Cart>().DeleteAsync(cart);

            _logger.LogInformation("Created order with ID: {OrderId} for user: {UserId}", 
                createdOrder.Id, cart.UserId);

            return createdOrder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order from cart: {CartId}", cartId);
            throw;
        }
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        try
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found");
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Order>().UpdateAsync(order);
            _logger.LogInformation("Updated order status to {Status} for order: {OrderId}", 
                status, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for order: {OrderId}", orderId);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync()
    {
        try
        {
            var products = await _unitOfWork.Repository<Product>()
                .FindAsync(p => p.IsAvailable && p.IsFeatured);
            
            return products.OrderByDescending(p => p.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving featured products");
            throw;
        }
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, string? category = null)
    {
        try
        {
            var products = await _unitOfWork.Repository<Product>()
                .FindAsync(p => p.IsAvailable && 
                               (string.IsNullOrEmpty(category) || p.Category == category) &&
                               (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)));
            
            return products.OrderByDescending(p => p.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8)}".ToUpper();
    }
} 