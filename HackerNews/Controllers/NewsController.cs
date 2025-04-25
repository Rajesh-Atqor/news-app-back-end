using HackerNews.Business.Interfaces;
using HackerNews.Business.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Controllers
{
    /// <summary>
    /// Controller responsible for handling requests related to Hacker News stories.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsController"/> class.
        /// </summary>
        /// <param name="newsService">The service for retrieving Hacker News stories.</param>
        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        /// <summary>
        /// Retrieves a paginated list of new Hacker News stories based on the provided request parameters.
        /// </summary>
        /// <param name="request">The request containing pagination and search term information.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the list of new stories, or a Bad Request response if the request is null.
        /// </returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> GetNewStories([FromBody] NewStoriesRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            return Ok(await _newsService.GetNewStories(request));
        }
    }
}

