using System.Collections.Generic;
using System.Threading.Tasks;
using Prisma.Models;

namespace Prisma.Services
{
    public interface INewsService
    {
        Task<NewsArticle> GetArticleByIdAsync(string id);
        Task<List<NewsArticle>> GetArticlesByCategoryAsync(string category, int count = 12);
        Task<List<NewsArticle>> GetRelatedArticlesAsync(string articleId, int count = 3);
    }
}