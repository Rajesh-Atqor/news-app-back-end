using HackerNews.Controllers;
using HackerNews.Interfaces;
using HackerNews.Models;
using HackerNews.Models.Dtos;
using HackerNews.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HackerNewsTest.Controllers
{
    public class NewsControllerTest
    {
        private readonly Mock<INewsService> _mockService;
        private readonly NewsController _controller;

        public NewsControllerTest()
        {
            _mockService = new Mock<INewsService>();
            _controller = new NewsController(_mockService.Object);
        }

        [Fact]
        public async Task GetNewStories_ReturnsOkResult_WithValidData()
        {
            
            var request = new GetNewStoriesRequestDto
            {
                SearchTerm = "test",
                PageNumber = 1,
                PageSize = 10
            };

            var pagedResult = new PagedResult<StoryViewModel>
            {
                Items = new List<StoryViewModel>
                   {
                       new StoryViewModel { Id = 1, Title = "Test Story", Url = "http://test.com" }
                   },
                TotalCount = 1
            };

            _mockService.Setup(s => s.GetNewStories(request)).ReturnsAsync(pagedResult);

            
            var result = await _controller.GetNewStories(request);

             
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResult<StoryViewModel>>(okResult.Value);
            Assert.Single(returnValue.Items);
            Assert.Equal(1, returnValue.TotalCount);
        }

        [Fact]
        public async Task GetNewStories_ReturnsBadRequest_WhenRequestIsInvalid()
        {
            
            GetNewStoriesRequestDto request = null;

            _controller.ModelState.AddModelError("SearchTerm", "Required");

            
            var result = await _controller.GetNewStories(request);

             
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetNewStories_ReturnsEmptyResult_WhenNoStoriesFound()
        {
            
            var request = new GetNewStoriesRequestDto
            {
                SearchTerm = "nonexistent",
                PageNumber = 1,
                PageSize = 10
            };

            var pagedResult = new PagedResult<StoryViewModel>
            {
                Items = new List<StoryViewModel>(),
                TotalCount = 0
            };

            _mockService.Setup(s => s.GetNewStories(request)).ReturnsAsync(pagedResult);

            
            var result = await _controller.GetNewStories(request);

             
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResult<StoryViewModel>>(okResult.Value);
            Assert.Empty(returnValue.Items);
            Assert.Equal(0, returnValue.TotalCount);
        }
    }
}
