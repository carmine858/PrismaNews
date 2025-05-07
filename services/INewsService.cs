using PrismaNews.model;

namespace PrismaNews.services
{
    public interface INewsService
    {
        Task<List<Article>> GetLatestNewsAsync(string category = null, int count = 10);
        Task<Article> GetArticleByUrlAsync(string url);
        Task<Article> CreateArticleFromTextAsync(string title, string content, string source = "User Submitted");
    }
}
