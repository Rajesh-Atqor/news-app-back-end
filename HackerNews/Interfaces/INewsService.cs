using HackerNews.Models.Dtos;
using HackerNews.Models.ViewModels;
using HackerNews.Models;

namespace HackerNews.Interfaces
{
    public interface INewsService
    {
        Task<PagedResult<StoryViewModel>> GetNewStories(GetNewStoriesRequestDto request);
    }
}
