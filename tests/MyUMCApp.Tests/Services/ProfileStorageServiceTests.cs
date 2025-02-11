using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using MyUMCApp.Members.API.Services;
using Xunit;

namespace MyUMCApp.Tests.Services;

public class ProfileStorageServiceTests
{
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ProfileStorageService _service;

    public ProfileStorageServiceTests()
    {
        _s3ClientMock = new Mock<IAmazonS3>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(x => x["AWS:S3:ProfilePicturesBucket"]).Returns("test-bucket");
        _configurationMock.Setup(x => x["AWS:CloudFront:Domain"]).Returns("test.cloudfront.net");

        _service = new ProfileStorageService(_s3ClientMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task UploadProfilePictureAsync_WithValidFile_ReturnsCloudFrontUrl()
    {
        // Arrange
        var fileName = "test-user-123.jpg";
        var fileContent = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
        var expectedKey = $"profiles/{fileName}";
        var expectedUrl = $"https://test.cloudfront.net/{expectedKey}";

        _s3ClientMock
            .Setup(x => x.PutObjectAsync(
                It.Is<PutObjectRequest>(r =>
                    r.BucketName == "test-bucket" &&
                    r.Key == expectedKey &&
                    r.ContentType == "image/jpeg" &&
                    r.CannedACL == S3CannedACL.Private),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        var result = await _service.UploadProfilePictureAsync(fileContent, fileName);

        // Assert
        result.Should().Be(expectedUrl);
        _s3ClientMock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UploadProfilePictureAsync_WhenS3Fails_ThrowsApplicationException()
    {
        // Arrange
        var fileName = "test.jpg";
        var fileContent = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

        _s3ClientMock
            .Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("S3 error"));

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationException>(
            () => _service.UploadProfilePictureAsync(fileContent, fileName));
    }

    [Theory]
    [InlineData("test.jpg", "image/jpeg")]
    [InlineData("test.jpeg", "image/jpeg")]
    [InlineData("test.png", "image/png")]
    [InlineData("test.unknown", "application/octet-stream")]
    public async Task UploadProfilePictureAsync_SetsCorrectContentType(string fileName, string expectedContentType)
    {
        // Arrange
        var fileContent = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

        // Act
        await _service.UploadProfilePictureAsync(fileContent, fileName);

        // Assert
        _s3ClientMock.Verify(
            x => x.PutObjectAsync(
                It.Is<PutObjectRequest>(r => r.ContentType == expectedContentType),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
} 