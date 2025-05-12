using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Prisma.Models;
using PrismaNews.Services;

namespace Prisma.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly GuardianApiService _guardianApiService;

        public IndexModel(ILogger<IndexModel> logger, GuardianApiService guardianApiService)
        {
            _logger = logger;
            _guardianApiService = guardianApiService;
        }

        public Dictionary<string, NewsCategoryGroup> NewsByCategory { get; private set; } = new Dictionary<string, NewsCategoryGroup>();
        public List<NewsArticle> FeaturedNews { get; private set; } = new List<NewsArticle>();
        public List<NewsArticle> TrendingNews { get; private set; } = new List<NewsArticle>();
        public bool HasError { get; private set; } = false;
        public string ErrorMessage { get; private set; } = string.Empty;
        public bool IsLoading { get; private set; } = true;
        public string ActiveCategoryTab { get; private set; } = "all";

        public async Task OnGetAsync(string categoryTab = "all")
        {
            IsLoading = true;
            ActiveCategoryTab = categoryTab;

            try
            {
                // Load data in parallel
                var featuredNewsTask = _guardianApiService.GetFeaturedNewsAsync(4);
                var trendingNewsTask = _guardianApiService.GetTrendingNewsAsync(7);
                var newsByCategoryTask = _guardianApiService.GetNewsByCategoryAsync(18);

                await Task.WhenAll(featuredNewsTask, trendingNewsTask, newsByCategoryTask);

                FeaturedNews = featuredNewsTask.Result;
                TrendingNews = trendingNewsTask.Result;
                NewsByCategory = newsByCategoryTask.Result;

                // Create a "Latest" category with a mix of news from different sections
                if (NewsByCategory.Any())
                {
                    var latestGroup = new NewsCategoryGroup("latest");
                    latestGroup.CategoryName = "Le Ultime";
                    latestGroup.Description = "Le notizie più recenti da tutte le categorie";
                    latestGroup.IconClass = "bi-lightning";
                    latestGroup.ThemeColor = "#6610f2";
                    latestGroup.GradientClass = "category-latest";

                    var latestNews = NewsByCategory.Values
                        .SelectMany(g => g.Articles)
                        .OrderByDescending(n => n.PublicationDate)
                        .Take(8)
                        .ToList();

                    latestGroup.Articles = latestNews;

                    if (!NewsByCategory.ContainsKey("latest"))
                    {
                        NewsByCategory.Add("latest", latestGroup);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving news for the homepage");
                HasError = true;
                ErrorMessage = "Si è verificato un errore nel caricamento delle notizie. Riprova più tardi.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}