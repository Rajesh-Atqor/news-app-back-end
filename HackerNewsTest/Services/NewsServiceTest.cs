using HackerNews.Constants;
using HackerNews.Interfaces;
using HackerNews.Models;
using HackerNews.Models.Dtos;
using HackerNews.Models.ViewModels;
using HackerNews.Services;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;

namespace HackerNewsTest.Services
{
    public class NewsServiceTests
    {
        private readonly Mock<ICacheService> _mockCache;
        private HttpClient _httpClient;
        private readonly NewsService _newsService;

        public NewsServiceTests()
        {
            _mockCache = new Mock<ICacheService>();

            var appConfig = Options.Create(new AppConfig
            {
                HackerNewsApiBaseUrl = "https://hacker-news.firebaseio.com/v0"
            });

            var handler = new MockHttpMessageHandler();

            handler.When("https://hacker-news.firebaseio.com/v0/newstories.json")
                .Respond("application/json", "[1,2]"); // Respond with JSON

            handler.When("https://hacker-news.firebaseio.com/v0/item/1.json")
              .Respond("application/json", "{ \"id\": 1, \"title\": \"Story 1\", \"url\": \"https://tech.deriv.com/tracing-perl-memory-leaks-with-devel-mat-part-1/\" }");

            handler.When("https://hacker-news.firebaseio.com/v0/item/2.json")
              .Respond("application/json", "{ \"id\": 2, \"title\": \"Story 2\", \"url\": \"https://tech.deriv.com/tracing-perl-memory-leaks-with-devel-mat-part-1/\" }");

            _httpClient = new HttpClient(handler);

            _newsService = new NewsService(appConfig, _mockCache.Object, _httpClient);
        }

        private StoryViewModel CreateStory(int id, string title) => new StoryViewModel { Id = id, Title = title };

        

        [Fact]
        public async Task GetNewStories_FiltersBySearchTerm()
        {
            var stories = new List<StoryViewModel>
            {
                CreateStory(1, "C# is awesome"),
                CreateStory(2, "Learn Python")
            };

            _mockCache.Setup(c => c.GetCache(CommonConstants.NewStoriesCacheKey)).Returns(stories);

            var result = await _newsService.GetNewStories(new GetNewStoriesRequestDto
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "C#"
            });

            Assert.Single(result.Items);
            Assert.Equal("C# is awesome", result.Items.First().Title);
        }

        [Fact]
        public async Task GetNewStories_PaginatesCorrectly()
        {
            var stories = Enumerable.Range(1, 20)
                .Select(i => CreateStory(i, $"Story {i}"))
                .ToList();

            _mockCache.Setup(c => c.GetCache(CommonConstants.NewStoriesCacheKey)).Returns(stories);

            var result = await _newsService.GetNewStories(new GetNewStoriesRequestDto
            {
                PageNumber = 2,
                PageSize = 5
            });

            Assert.Equal(5, result.Items.Count);
            Assert.Equal("Story 6", result.Items.First().Title);
            Assert.Equal(20, result.TotalCount);
        }

        [Fact]
        public async Task GetNewStories_ReturnsAll_WhenSearchTermIsEmpty()
        {
            var stories = new List<StoryViewModel>
            {
                CreateStory(1, "C# Guide"),
                CreateStory(2, "Java Basics")
            };

            _mockCache.Setup(c => c.GetCache(CommonConstants.NewStoriesCacheKey)).Returns(stories);

            var result = await _newsService.GetNewStories(new GetNewStoriesRequestDto
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = ""
            });

            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
        }
        [Fact]
        public async Task GetNewStories_UsesCache_WhenCacheIsNotEmpty()
        {
            var stories = new List<StoryViewModel>
            {
                CreateStory(1, "Cached Story 1"),
                CreateStory(2, "Cached Story 2")
            };

            _mockCache.Setup(c => c.GetCache(CommonConstants.NewStoriesCacheKey)).Returns(stories);

            var result = await _newsService.GetNewStories(new GetNewStoriesRequestDto
            {
                PageNumber = 1,
                PageSize = 10
            });

            _mockCache.Verify(c => c.GetCache(CommonConstants.NewStoriesCacheKey), Times.Once);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("Cached Story 1", result.Items.First().Title);
        }

        [Fact]
        public async Task GetNewStories_FetchesFromHttpClient_WhenCacheIsEmpty()
        {
            _mockCache.Setup(c => c.GetCache(CommonConstants.NewStoriesCacheKey)).Returns(null);

            var result = await _newsService.GetNewStories(new GetNewStoriesRequestDto
            {
                PageNumber = 1,
                PageSize = 10
            });

            _mockCache.Verify(c => c.GetCache(CommonConstants.NewStoriesCacheKey), Times.Once);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("Story 1", result.Items.First().Title);
        }
    }
}
