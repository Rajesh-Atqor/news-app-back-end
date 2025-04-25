using HackerNews.Business.Constants;
using HackerNews.Business.Interfaces;
using HackerNews.Business.Models;
using HackerNews.Business.Models.Dtos;
using HackerNews.Business.Models.ViewModels;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
namespace HackerNews.Business.Services
{
    /// <summary>
    /// Service responsible for fetching, filtering, and caching Hacker News stories.
    /// </summary>
    public class NewsService : INewsService
    {
        private readonly AppConfig _appConfig;
        private readonly ICacheService _cache;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsService"/> class.
        /// </summary>
        /// <param name="appConfig">Configuration settings including API base URLs.</param>
        /// <param name="cache">Cache service for storing and retrieving data.</param>
        /// <param name="httpClient">HTTP client used to call the Hacker News API.</param>
        public NewsService(IOptions<AppConfig> appConfig, ICacheService cache, HttpClient httpClient)
        {
            _appConfig = appConfig.Value;
            _cache = cache;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieves a paginated and optionally filtered list of new Hacker News stories.
        /// </summary>
        /// <param name="request">The pagination and search term request.</param>
        /// <returns>A <see cref="PagedResult{T}"/> containing the stories and total count.</returns>
        public async Task<PagedResult<StoryViewModel>> GetNewStories(NewStoriesRequestDto request)
        {
            var cachedData = _cache.GetCache(CommonConstants.NewStoriesCacheKey);

            if (cachedData == null)
            {
                cachedData = await FetchAndCacheNewStories();
            }

            var newStories = cachedData as List<StoryViewModel>;

            if (newStories == null || !newStories.Any())
                return new PagedResult<StoryViewModel> { Items = new List<StoryViewModel>(), TotalCount = 0 };

            var filteredStories = string.IsNullOrEmpty(request.SearchTerm)
                ? newStories
                : newStories.Where(s => s.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

            var pagedStories = filteredStories
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<StoryViewModel>
            {
                Items = pagedStories,
                TotalCount = filteredStories.Count
            };
        }

        /// <summary>
        /// Fetches the latest new stories from the Hacker News API and caches them.
        /// </summary>
        /// <returns>A list of <see cref="StoryViewModel"/> representing the new stories.</returns>
        private async Task<List<StoryViewModel>> FetchAndCacheNewStories()
        {
            var apiUrl = $"{_appConfig.HackerNewsApiBaseUrl}/newstories.json";
            var newStoryIds = await _httpClient.GetFromJsonAsync<List<int>>(apiUrl);

            if (newStoryIds == null || !newStoryIds.Any())
                return new List<StoryViewModel>();

            // Fetch stories in parallel for performance
            var storyTasks = newStoryIds.Take(200).Select(id =>
            {
                var storyUrl = $"{_appConfig.HackerNewsApiBaseUrl}/item/{id}.json";
                return _httpClient.GetFromJsonAsync<StoryViewModel>(storyUrl);
            });

            var newStories = (await Task.WhenAll(storyTasks)).Where(s => s != null).ToList();

            _cache.SetCache(CommonConstants.NewStoriesCacheKey, newStories);
            return newStories;
        }
    }
}
