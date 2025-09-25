using MyUMCApp.API.Models;

namespace MyUMCApp.API.Services;

public interface IStoreService
{
    // Product management
    Task<Product> CreateProductAsync(Product product);
    Task<ProductVariant> AddProductVariantAsync(ProductVariant variant);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync();
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, string? category = null);

    // Cart management
    Task<Cart> CreateCartAsync(string userId);
    Task AddToCartAsync(Guid cartId, CartItem item);

    // Order management
    Task<Order> CreateOrderAsync(Guid cartId, string shippingAddress, string billingAddress, PaymentMethod paymentMethod);
    Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
}