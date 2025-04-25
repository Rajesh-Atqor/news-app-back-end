using Moq;
using Microsoft.Extensions.Caching.Memory;
using HackerNews.Business.Services;

namespace HackerNewsTest.Services
{
    public class CacheServiceTests
    {
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly CacheService _cacheService;

        public CacheServiceTests()
        {
            _mockMemoryCache = new Mock<IMemoryCache>();
            _cacheService = new CacheService(_mockMemoryCache.Object);
        }

        [Fact]
        public void SetCache_Should_Call_Set_With_Proper_Arguments()
        {
           
            var key = "test-key";
            var value = "test-value";

            var mockEntry = new Mock<ICacheEntry>();
            _mockMemoryCache
                .Setup(m => m.CreateEntry(key))
                .Returns(mockEntry.Object);

            
            _cacheService.SetCache(key, value);

            
            _mockMemoryCache.Verify(m => m.CreateEntry(key), Times.Once);
            mockEntry.VerifySet(m => m.Value = value, Times.Once);
        }

        [Fact]
        public void GetCache_Should_Return_Value_When_Exists()
        {
           
            var key = "existing-key";
            object expectedValue = "cached-data";

            _mockMemoryCache
                .Setup(m => m.TryGetValue(key, out expectedValue))
                .Returns(true);

            
            var result = _cacheService.GetCache(key);

            
            Assert.Equal(expectedValue, result);
        }


        [Fact]
        public void GetCache_Should_Return_Null_When_Not_Found()
        {
           
            var key = "missing-key";
            object value = null;

            _mockMemoryCache
                .Setup(mc => mc.TryGetValue(key, out value))
                .Returns(false);

            
            var result = _cacheService.GetCache(key);

            
            Assert.Null(result);
        }
    }

}
