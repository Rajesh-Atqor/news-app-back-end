namespace HackerNews.Business.Interfaces
{
    public interface ICacheService
    {
        void SetCache(string key, object value);
        object GetCache(string key);
    }
}
