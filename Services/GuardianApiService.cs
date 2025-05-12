using System.Text.Json;
using Prisma.Models;


namespace PrismaNews.Services
{
    public class GuardianApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GuardianApiService> _logger;
        private readonly string _apiKey;

        public GuardianApiService(HttpClient httpClient, IConfiguration configuration, ILogger<GuardianApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GuardianApiKey"] ?? "50a1a343-cac9-42ef-ba53-aeba70255397";
            _httpClient.BaseAddress = new Uri("https://content.guardianapis.com/");
        }

        public async Task<Dictionary<string, NewsCategoryGroup>> GetNewsByCategoryAsync(int articlesPerCategory = 12)
        {
            var categories = new string[] {
                "politics", "environment", "world", "technology",
                "business", "science", "society", "opinion"
            };

            var categoryGroups = new Dictionary<string, NewsCategoryGroup>();

            foreach (var category in categories)
            {
                try
                {
                    var articles = await GetNewsBySectionAsync(category, articlesPerCategory);
                    if (articles.Any())
                    {
                        var group = new NewsCategoryGroup(category);
                        group.Articles = articles;
                        categoryGroups.Add(category, group);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching {Category} news", category);
                }
            }

            return categoryGroups;
        }

        public async Task<List<NewsArticle>> GetNewsBySectionAsync(string section, int count = 8)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "api-key", _apiKey },
                    { "section", section },
                    { "show-fields", "headline,byline,trailText,lastModified,thumbnail" },
                    { "show-tags", "keyword" },
                    { "page-size", count.ToString() }
                };

                var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                var response = await _httpClient.GetAsync($"search?{queryString}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Guardian API returned status code: {StatusCode}", response.StatusCode);
                    return new List<NewsArticle>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var guardianResponse = JsonSerializer.Deserialize<GuardianNewsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (guardianResponse?.Response?.Results == null)
                {
                    _logger.LogWarning("Guardian API returned null or empty results");
                    return new List<NewsArticle>();
                }

                return guardianResponse.Response.Results
                    .Select(r => new NewsArticle
                    {
                        Id = r.Id,
                        Title = r.Fields?.Headline ?? r.WebTitle,
                        Section = r.SectionName,
                        PublicationDate = r.Fields?.LastModified != null
                            ? DateTime.Parse(r.Fields.LastModified)
                            : r.WebPublicationDate,
                        Url = r.WebUrl,
                        ImageUrl = r.Fields?.Thumbnail,
                        Summary = r.Fields?.TrailText ?? "Nessuna descrizione disponibile.",
                        Author = r.Fields?.Byline ?? "The Guardian",
                        Tags = r.Tags?.Select(t => t.WebTitle).ToList() ?? new List<string>(),
                        ApiUrl = r.ApiUrl
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching news from Guardian API for section {Section}", section);
                return new List<NewsArticle>();
            }
        }

        public async Task<List<NewsArticle>> GetFeaturedNewsAsync(int count = 4)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "api-key", _apiKey },
                    { "show-fields", "headline,byline,trailText,lastModified,thumbnail,wordcount" },
                    { "show-tags", "keyword" },
                    { "page-size", count.ToString() },
                    { "order-by", "relevance" }
                };

                var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                var response = await _httpClient.GetAsync($"search?{queryString}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Guardian API returned status code: {StatusCode}", response.StatusCode);
                    return new List<NewsArticle>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var guardianResponse = JsonSerializer.Deserialize<GuardianNewsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (guardianResponse?.Response?.Results == null)
                {
                    _logger.LogWarning("Guardian API returned null or empty results for featured news");
                    return new List<NewsArticle>();
                }

                return guardianResponse.Response.Results
                    .Where(r => r.Fields?.Thumbnail != null) // Ensure we have images for featured news
                    .Select(r => new NewsArticle
                    {
                        Id = r.Id,
                        Title = r.Fields?.Headline ?? r.WebTitle,
                        Section = r.SectionName,
                        PublicationDate = r.Fields?.LastModified != null
                            ? DateTime.Parse(r.Fields.LastModified)
                            : r.WebPublicationDate,
                        Url = r.WebUrl,
                        ImageUrl = r.Fields?.Thumbnail,
                        Summary = r.Fields?.TrailText ?? "Nessuna descrizione disponibile.",
                        Author = r.Fields?.Byline ?? "The Guardian",
                        Tags = r.Tags?.Select(t => t.WebTitle).ToList() ?? new List<string>(),
                        ApiUrl = r.ApiUrl,
                        IsFeatured = true
                    })
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching featured news from Guardian API");
                return new List<NewsArticle>();
            }
        }




        public async Task<List<NewsArticle>> GetTrendingNewsAsync(int count = 6)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "api-key", _apiKey },
                    { "show-fields", "headline,byline,trailText,lastModified,thumbnail" },
                    { "tag", "tone/analysis" },
                    { "show-tags", "keyword" },
                    { "page-size", "20" }
                };

                var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                var response = await _httpClient.GetAsync($"search?{queryString}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Guardian API returned status code: {StatusCode}", response.StatusCode);
                    return new List<NewsArticle>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var guardianResponse = JsonSerializer.Deserialize<GuardianNewsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (guardianResponse?.Response?.Results == null)
                {
                    _logger.LogWarning("Guardian API returned null or empty results for trending news");
                    return new List<NewsArticle>();
                }

                // Get those with thumbnail images and randomly select trending news
                var articlesWithImages = guardianResponse.Response.Results
                    .Where(r => r.Fields?.Thumbnail != null)
                    .ToList();

                var random = new Random();
                return articlesWithImages
                    .OrderBy(_ => random.Next())
                    .Take(count)
                    .Select(r => new NewsArticle
                    {
                        Id = r.Id,
                        Title = r.Fields?.Headline ?? r.WebTitle,
                        Section = r.SectionName,
                        PublicationDate = r.Fields?.LastModified != null
                            ? DateTime.Parse(r.Fields.LastModified)
                            : r.WebPublicationDate,
                        Url = r.WebUrl,
                        ImageUrl = r.Fields?.Thumbnail,
                        Summary = r.Fields?.TrailText ?? "Nessuna descrizione disponibile.",
                        Author = r.Fields?.Byline ?? "The Guardian",
                        Tags = r.Tags?.Select(t => t.WebTitle).ToList() ?? new List<string>(),
                        ApiUrl = r.ApiUrl,
                        IsTrending = true
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trending news from Guardian API");
                return new List<NewsArticle>();
            }
        }
    }
}