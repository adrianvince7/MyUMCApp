using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyUMCApp.Shared.Data;
using MyUMCApp.Shared.Models;
using MyUMCApp.Store.API.Services;
using Xunit;

namespace MyUMCApp.Tests.Services;

public class StoreServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<StoreService>> _loggerMock;
    private readonly StoreService _service;

    public StoreServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<StoreService>>();
        _service = new StoreService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateProductAsync_WithValidData_ReturnsProduct()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            StockQuantity = 10,
            SKU = "TEST-001",
            Category = "Books"
        };

        var productRepoMock = new Mock<IRepository<Product>>();
        productRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(productRepoMock.Object);

        // Act
        var result = await _service.CreateProductAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AddProductVariantAsync_WithValidData_ReturnsVariant()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var variant = new ProductVariant
        {
            ProductId = productId,
            Name = "Test Variant",
            SKU = "TEST-001-VAR",
            Price = 89.99m,
            StockQuantity = 5
        };

        var product = new Product
        {
            Id = productId,
            Variants = new List<ProductVariant>()
        };

        var productRepoMock = new Mock<IRepository<Product>>();
        productRepoMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        var variantRepoMock = new Mock<IRepository<ProductVariant>>();
        variantRepoMock
            .Setup(x => x.AddAsync(It.IsAny<ProductVariant>()))
            .ReturnsAsync(variant);

        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(productRepoMock.Object);
        _unitOfWorkMock
            .Setup(x => x.Repository<ProductVariant>())
            .Returns(variantRepoMock.Object);

        // Act
        var result = await _service.AddProductVariantAsync(variant);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(variant);
        productRepoMock.Verify(x => x.UpdateAsync(It.Is<Product>(p => 
            p.Id == productId && 
            p.Variants.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task CreateCartAsync_WithValidData_ReturnsCart()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart
        {
            UserId = userId,
            Items = new List<CartItem>()
        };

        var cartRepoMock = new Mock<IRepository<Cart>>();
        cartRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Cart>()))
            .ReturnsAsync(cart);

        _unitOfWorkMock
            .Setup(x => x.Repository<Cart>())
            .Returns(cartRepoMock.Object);

        // Act
        var result = await _service.CreateCartAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AddToCartAsync_WithValidData_AddsItemToCart()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = cartId,
            Items = new List<CartItem>()
        };

        var product = new Product
        {
            Id = productId,
            Price = 99.99m,
            StockQuantity = 10
        };

        var cartItem = new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = 2
        };

        var cartRepoMock = new Mock<IRepository<Cart>>();
        cartRepoMock
            .Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);

        var productRepoMock = new Mock<IRepository<Product>>();
        productRepoMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.Repository<Cart>())
            .Returns(cartRepoMock.Object);
        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(productRepoMock.Object);

        // Act
        await _service.AddToCartAsync(cartId, cartItem);

        // Assert
        cartRepoMock.Verify(x => x.UpdateAsync(It.Is<Cart>(c => 
            c.Id == cartId && 
            c.Items.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidData_ReturnsOrder()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = cartId,
            UserId = userId,
            Items = new List<CartItem>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    Product = new Product { Price = 99.99m }
                }
            }
        };

        var cartRepoMock = new Mock<IRepository<Cart>>();
        cartRepoMock
            .Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);

        var orderRepoMock = new Mock<IRepository<Order>>();
        orderRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order order) => order);

        _unitOfWorkMock
            .Setup(x => x.Repository<Cart>())
            .Returns(cartRepoMock.Object);
        _unitOfWorkMock
            .Setup(x => x.Repository<Order>())
            .Returns(orderRepoMock.Object);

        // Act
        var result = await _service.CreateOrderAsync(cartId, "123 Test St", "123 Test St", PaymentMethod.EcoCash);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Status.Should().Be(OrderStatus.Pending);
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(199.98m);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidData_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Pending
        };

        var orderRepoMock = new Mock<IRepository<Order>>();
        orderRepoMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.Repository<Order>())
            .Returns(orderRepoMock.Object);

        // Act
        await _service.UpdateOrderStatusAsync(orderId, OrderStatus.Processing);

        // Assert
        orderRepoMock.Verify(x => x.UpdateAsync(It.Is<Order>(o => 
            o.Id == orderId && 
            o.Status == OrderStatus.Processing)), Times.Once);
    }

    [Fact]
    public async Task GetFeaturedProductsAsync_ReturnsProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { IsFeatured = true, Name = "Featured 1" },
            new() { IsFeatured = true, Name = "Featured 2" }
        };

        var productRepoMock = new Mock<IRepository<Product>>();
        productRepoMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(productRepoMock.Object);

        // Act
        var result = await _service.GetFeaturedProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchProductsAsync_ReturnsMatchingProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Name = "Test Book 1", Category = "Books" },
            new() { Name = "Test Book 2", Category = "Books" }
        };

        var productRepoMock = new Mock<IRepository<Product>>();
        productRepoMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(productRepoMock.Object);

        // Act
        var result = await _service.SearchProductsAsync("Book", "Books");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }
} 