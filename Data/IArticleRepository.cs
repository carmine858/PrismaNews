using PrismaNews.model;

namespace PrismaNews.Data
{
    public interface IArticleRepository
    {
        Task<Article> GetByIdAsync(int id);
        Task<List<Article>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<List<Article>> GetByCategoryAsync(string category, int page = 1, int pageSize = 10);
        Task<int> AddAsync(Article article);
        Task UpdateAsync(Article article);
        Task<ArticleAnalysis> GetAnalysisForArticleAsync(int articleId);
        Task<int> AddAnalysisAsync(ArticleAnalysis analysis);
    }
}
