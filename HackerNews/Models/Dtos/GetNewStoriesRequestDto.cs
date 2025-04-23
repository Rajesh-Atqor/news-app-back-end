namespace HackerNews.Models.Dtos
{
    public class GetNewStoriesRequestDto
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1; // Default to the first page
        public int PageSize { get; set; } = 10; // Default page size
    }
}
