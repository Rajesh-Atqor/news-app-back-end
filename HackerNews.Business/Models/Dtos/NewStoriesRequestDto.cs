namespace HackerNews.Business.Models.Dtos
{
    public class NewStoriesRequestDto
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1; // Default to the first page
        public int PageSize { get; set; } = 10; // Default page size
    }
}
