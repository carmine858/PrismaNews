using HtmlAgilityPack;
using PrismaNews.model;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrismaNews.services
{
    public interface IArticleAnalysisService
    {
        Task<ArticleAnalysis> AnalyzeArticleAsync(Article article, string category = null);
        Task<string> DetectCategoryAsync(Article article);
        Task<string> DetectBiasAsync(Article article, string category);
        Task<string> IdentifyRhetoricalTechniquesAsync(Article article, string category);
        Task<string> GenerateAlternativePerspectivesAsync(Article article, string category);
        Task<string> IdentifyOmissionsAsync(Article article, string category);
        Task<string> AnalyzeScientificReferencesAsync(Article article);
        Task<string> IdentifyCommercialInterestsAsync(Article article);
        Task<string> GenerateCounterArgumentsAsync(Article article, string category);
    }
}


