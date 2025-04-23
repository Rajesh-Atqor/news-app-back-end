using HackerNews.Constants;
using HackerNews.Interfaces;
using HackerNews.Models;
using HackerNews.Models.Dtos;
using HackerNews.Models.ViewModels;
using Microsoft.Extensions.Options;

namespace HackerNews.Services
{
    public class NewsService : INewsService
    {
        private readonly AppConfig _appConfig;
        private readonly ICacheService _cache;
        private readonly HttpClient _httpClient;

        public NewsService(IOptions<AppConfig> appConfig, ICacheService cache, HttpClient httpClient)
        {
            _appConfig = appConfig.Value;
            _cache = cache;
            _httpClient = httpClient;
        }

        public async Task<PagedResult<StoryViewModel>> GetNewStories(GetNewStoriesRequestDto request)
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

        private async Task<List<StoryViewModel>> FetchAndCacheNewStories()
        {
            var apiUrl = $"{_appConfig.HackerNewsApiBaseUrl}/newstories.json";
            var newStoryIds = await _httpClient.GetFromJsonAsync<List<int>>(apiUrl);

            if (newStoryIds == null || !newStoryIds.Any())
                return new List<StoryViewModel>();

            // Fetch stories in parallel
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
