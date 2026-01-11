using System.Text.Json;
using FluentAssertions;
using KYC.Service.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KYC.Service.Tests.Caching;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<CacheService<TestModel>>> _loggerMock;
    private readonly CacheService<TestModel> _sut;

    public CacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CacheService<TestModel>>>();
        _sut = new CacheService<TestModel>(_cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrAddAsync_WhenKeyExistsInCache_ShouldReturnCachedValue()
    {
        // Arrange
        const string key = "test-key";
        
        var expectedValue = new TestModel { Name = "Test name" };
        var envelope = new { Payload = expectedValue };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(envelope);
        var fullKey = $"{typeof(TestModel).FullName}:{key}";

        _cacheMock.Setup(x => x.GetAsync(fullKey, CancellationToken.None))
            .ReturnsAsync(bytes);

        // Act
        var result = await _sut.GetOrAddAsync(key, () => Task.FromResult(new TestModel()));

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test name");
        _cacheMock.Verify(x => x.GetAsync(fullKey, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetOrAddAsync_WhenCacheIsEmpty_ShouldCallFactoryAndStoreValue()
    {
        // Arrange
        const string key = "new-key";
        
        var newValue = new TestModel { Name = "Test name" };
        var fullKey = $"{typeof(TestModel).FullName}:{key}";

        _cacheMock.Setup(x => x.GetAsync(fullKey, CancellationToken.None))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetOrAddAsync(key, () => Task.FromResult(newValue));

        // Assert
        result.Should().BeEquivalentTo(newValue);
        _cacheMock.Verify(x => x.SetAsync(
                fullKey,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task GetOrAddAsync_WhenForceRefreshIsTrue_ShouldIgnoreCacheAndCallFactory()
    {
        // Arrange
        const string key = "refresh-key";
        
        var factoryValue = new TestModel { Name = "Test name" };

        // Act
        var result = await _sut.GetOrAddAsync(key, () => Task.FromResult(factoryValue), forceRefresh: true);

        // Assert
        result.Name.Should().Be("Test name");
        _cacheMock.Verify(x => x.GetAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task GetOrAddAsync_WhenDeserializationFails_ShouldLogWarningRemoveKeyAndCallFactory()
    {
        // Arrange
        const string key = "corrupt-key";
        
        var fullKey = $"{typeof(TestModel).FullName}:{key}";
        var corruptBytes = "{ invalid json }"u8.ToArray();
        var newValue = new TestModel { Name = "Test name" };

        _cacheMock.Setup(x => x.GetAsync(fullKey, CancellationToken.None))
            .ReturnsAsync(corruptBytes);

        // Act
        var result = await _sut.GetOrAddAsync(key, () => Task.FromResult(newValue));

        // Assert
        result.Name.Should().Be("Test name");
        _cacheMock.Verify(x => x.RemoveAsync(fullKey, CancellationToken.None), Times.Exactly(2));
        _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to deserialize")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }
    
    public class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }
}