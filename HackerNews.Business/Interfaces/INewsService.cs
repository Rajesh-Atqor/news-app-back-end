using HackerNews.Business.Models;
using HackerNews.Business.Models.Dtos;
using HackerNews.Business.Models.ViewModels;

namespace HackerNews.Business.Interfaces
{
    public interface INewsService
    {
        Task<PagedResult<StoryViewModel>> GetNewStories(NewStoriesRequestDto request);
    }
}
