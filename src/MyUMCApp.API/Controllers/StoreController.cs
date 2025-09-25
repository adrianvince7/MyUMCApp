using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyUMCApp.API.Models;
using MyUMCApp.API.Services;
using System.Security.Claims;

namespace MyUMCApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ILogger<StoreController> _logger;

    public StoreController(IStoreService storeService, ILogger<StoreController> logger)
    {
        _storeService = storeService;
        _logger = logger;
    }

    [HttpPost("products")]
    [Authorize(Roles = "Administrator,ChurchLeader")]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        try
        {
            var createdProduct = await _storeService.CreateProductAsync(product);
            return CreatedAtAction(nameof(SearchProducts), new { searchTerm = createdProduct.Name }, createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    [HttpPost("products/{productId:guid}/variants")]
    [Authorize(Roles = "Administrator,ChurchLeader")]
    public async Task<ActionResult<ProductVariant>> AddProductVariant(Guid productId, ProductVariant variant)
    {
        if (productId != variant.ProductId)
        {
            return BadRequest("Product ID mismatch");
        }

        try
        {
            var createdVariant = await _storeService.AddProductVariantAsync(variant);
            return Ok(createdVariant);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding variant to product with ID: {ProductId}", productId);
            return StatusCode(500, "An error occurred while adding the product variant");
        }
    }

    [HttpGet("products/featured")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Product>>> GetFeaturedProducts()
    {
        try
        {
            var products = await _storeService.GetFeaturedProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving featured products");
            return StatusCode(500, "An error occurred while retrieving featured products");
        }
    }

    [HttpGet("products/search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string searchTerm, [FromQuery] string? category = null)
    {
        try
        {
            var products = await _storeService.SearchProductsAsync(searchTerm, category);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with term: {SearchTerm}", SanitizeForLog(searchTerm ?? ""));
            return StatusCode(500, "An error occurred while searching products");
        }
    }

    [HttpPost("carts")]
    public async Task<ActionResult<Cart>> CreateCart()
    {
        try
        {
            var userId = GetCurrentUserId();
            var cart = await _storeService.CreateCartAsync(userId);
            return CreatedAtAction(nameof(CreateCart), new { id = cart.Id }, cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cart");
            return StatusCode(500, "An error occurred while creating the cart");
        }
    }

    [HttpPost("carts/{cartId:guid}/items")]
    public async Task<IActionResult> AddToCart(Guid cartId, CartItem item)
    {
        if (cartId != item.CartId)
        {
            return BadRequest("Cart ID mismatch");
        }

        try
        {
            await _storeService.AddToCartAsync(cartId, item);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart with ID: {CartId}", cartId);
            return StatusCode(500, "An error occurred while adding the item to cart");
        }
    }

    [HttpPost("orders")]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _storeService.CreateOrderAsync(
                request.CartId,
                request.ShippingAddress,
                request.BillingAddress,
                request.PaymentMethod);

            return CreatedAtAction(nameof(CreateOrder), new { id = order.Id }, order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order from cart: {CartId}", request.CartId);
            return StatusCode(500, "An error occurred while creating the order");
        }
    }

    [HttpPut("orders/{orderId:guid}/status")]
    [Authorize(Roles = "Administrator,ChurchLeader")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            await _storeService.UpdateOrderStatusAsync(orderId, request.Status);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for order: {OrderId}", orderId);
            return StatusCode(500, "An error occurred while updating the order status");
        }
    }

    private string GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new InvalidOperationException("User ID claim not found");
        }
        return userIdClaim;
    }

    private static string SanitizeForLog(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "[empty]";
        
        // Remove potential log injection characters
        return input.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').Trim();
    }
}

public class CreateOrderRequest
{
    public Guid CartId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}