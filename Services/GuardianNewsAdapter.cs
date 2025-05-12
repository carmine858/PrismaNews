using Prisma.Models;
using PrismaNews.Services;
using System.Text.Json;

namespace Prisma.Services
{
    public class GuardianNewsAdapter : INewsService
    {
        private readonly GuardianApiService _guardianApiService;

        public GuardianNewsAdapter(GuardianApiService guardianApiService)
        {
            _guardianApiService = guardianApiService;
        }

        public async Task<NewsArticle> GetArticleByIdAsync(string id)
        {
            // L'API Guardian non fornisce direttamente un endpoint per ID
            // Workaround: recupera il contenuto integrale usando l'URL API salvato
            try
            {
                var httpClient = new HttpClient();
                var apiKey = "50a1a343-cac9-42ef-ba53-aeba70255397"; // Sarebbe meglio prenderlo dalla configurazione

                // Se l'id è un URL API completo
                var apiUrl = id.StartsWith("http") ? id : $"https://content.guardianapis.com/{id}";

                // Aggiungi parametri per ottenere dati completi
                var queryParams = "?api-key=" + apiKey + "&show-fields=headline,byline,trailText,bodyText,lastModified,thumbnail";
                var response = await httpClient.GetAsync(apiUrl + queryParams);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Guardian API error: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);
                var result = jsonDocument.RootElement.GetProperty("response").GetProperty("content");

                var fields = result.TryGetProperty("fields", out var f) ? f : default;

                return new NewsArticle
                {
                    Id = result.GetProperty("id").GetString(),
                    Title = fields.TryGetProperty("headline", out var headline) ?
                        headline.GetString() : result.GetProperty("webTitle").GetString(),
                    Section = result.GetProperty("sectionName").GetString(),
                    PublicationDate = DateTime.Parse(result.GetProperty("webPublicationDate").GetString()),
                    Url = result.GetProperty("webUrl").GetString(),
                    ImageUrl = fields.TryGetProperty("thumbnail", out var thumbnail) ?
                        thumbnail.GetString() : null,
                    Summary = fields.TryGetProperty("bodyText", out var bodyText) ?
                        bodyText.GetString() : (fields.TryGetProperty("trailText", out var trailText) ?
                        trailText.GetString() : "Nessuna descrizione disponibile."),
                    Author = fields.TryGetProperty("byline", out var byline) ?
                        byline.GetString() : "The Guardian",
                    ApiUrl = result.GetProperty("apiUrl").GetString()
                };
            }
            catch (Exception ex)
            {
                // Fallback: cercare per titolo o contenuto
                var allCategories = await _guardianApiService.GetNewsByCategoryAsync(100);
                var allArticles = allCategories.Values.SelectMany(c => c.Articles);

                // Cerca per ID o per corrispondenza parziale nell'URL
                return allArticles.FirstOrDefault(a => a.Id == id || a.Id.Contains(id) ||
                    (a.ApiUrl != null && a.ApiUrl.Contains(id)));
            }
        }

        public async Task<List<NewsArticle>> GetArticlesByCategoryAsync(string category, int count = 10)
        {
            return await _guardianApiService.GetNewsBySectionAsync(category, count);
        }

        public async Task<List<NewsArticle>> GetRelatedArticlesAsync(string articleId, int count = 3)
        {
            try
            {
                var article = await GetArticleByIdAsync(articleId);
                if (article == null) return new List<NewsArticle>();

                // Ottieni articoli della stessa categoria
                return await GetArticlesByCategoryAsync(article.Section, count);
            }
            catch
            {
                return new List<NewsArticle>();
            }
        }
    }
}