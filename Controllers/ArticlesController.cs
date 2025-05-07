using Microsoft.AspNetCore.Mvc;
using PrismaNews.model;
using PrismaNews.services;
using PrismaNews.Data;

namespace PrismaNews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly IArticleAnalysisService _analysisService;
        private readonly PrismaNewsDbContext _context;

        public ArticlesController(
            INewsService newsService,
            IArticleAnalysisService analysisService,
            PrismaNewsDbContext context)
        {
            _newsService = newsService;
            _analysisService = analysisService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestNews(string category = null, int count = 10)
        {
            var articles = await _newsService.GetLatestNewsAsync(category, count);
            return Ok(articles);
        }

        [HttpPost("analyze/url")]
        public async Task<IActionResult> AnalyzeFromUrl([FromBody] UrlAnalysisRequest request)
        {
            var article = await _newsService.GetArticleByUrlAsync(request.Url);
            var category = string.IsNullOrEmpty(request.Category) ?
                await _analysisService.DetectCategoryAsync(article) :
                request.Category;

            // Save article to database
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            // Perform analysis
            var analysis = await _analysisService.AnalyzeArticleAsync(article, category);

            // Save analysis to database
            _context.ArticleAnalyses.Add(analysis);
            await _context.SaveChangesAsync();

            return Ok(new AnalysisResponse
            {
                Article = article,
                Analysis = analysis
            });
        }

        [HttpPost("analyze/text")]
        public async Task<IActionResult> AnalyzeFromText([FromBody] TextAnalysisRequest request)
        {
            var article = await _newsService.CreateArticleFromTextAsync(
                request.Title,
                request.Content,
                request.Source ?? "User Submitted");

            var category = string.IsNullOrEmpty(request.Category) ?
                await _analysisService.DetectCategoryAsync(article) :
                request.Category;

            // Save article to database
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            // Perform analysis
            var analysis = await _analysisService.AnalyzeArticleAsync(article, category);

            // Save analysis to database
            _context.ArticleAnalyses.Add(analysis);
            await _context.SaveChangesAsync();

            return Ok(new AnalysisResponse
            {
                Article = article,
                Analysis = analysis
            });
        }

        public class UrlAnalysisRequest
        {
            public string Url { get; set; }
            public string Category { get; set; }
        }

        public class TextAnalysisRequest
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public string Source { get; set; }
            public string Category { get; set; }
        }

        public class AnalysisResponse
        {
            public Article Article { get; set; }
            public ArticleAnalysis Analysis { get; set; }
        }
    }
}
