using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MyUMCApp.Client.Services;
using Xunit;

namespace MyUMCApp.Tests.Services;

public class ProfilePictureServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly ProfilePictureService _service;

    public ProfilePictureServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _configurationMock.Setup(x => x["API:BaseUrl"]).Returns("https://api.example.com");

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _service = new ProfilePictureService(httpClient, _configurationMock.Object);
    }

    [Fact]
    public async Task UploadProfilePictureAsync_WithValidImage_ReturnsUrl()
    {
        // Arrange
        var fileName = "test.jpg";
        var fileContent = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
        var expectedUrl = "https://cdn.example.com/profiles/test.jpg";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"{{\"url\": \"{expectedUrl}\"}}")
            });

        // Act
        var result = await _service.UploadProfilePictureAsync(fileContent, fileName);

        // Assert
        result.Should().Be(expectedUrl);
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Post && 
                req.RequestUri!.ToString() == "https://api.example.com/api/profile/upload-picture"),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task UploadProfilePictureAsync_WhenUploadFails_ReturnsNull()
    {
        // Arrange
        var fileName = "test.jpg";
        var fileContent = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var result = await _service.UploadProfilePictureAsync(fileContent, fileName);

        // Assert
        result.Should().BeNull();
    }
} 