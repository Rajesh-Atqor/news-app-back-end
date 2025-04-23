using HackerNews.Interfaces;
using HackerNews.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {

        private readonly INewsService _newsService;
        public NewsController(INewsService newsService) 
        {
            _newsService = newsService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetNewStories([FromBody] GetNewStoriesRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }
            return Ok(await _newsService.GetNewStories(request));
        }
    }
}
