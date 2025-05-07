using PrismaNews.model;
using System.Text.Json;
using System.Net.Http;

namespace PrismaNews.services
{
    public class NewsApiService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NewsApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<Article>> GetLatestNewsAsync(string category = null, int count = 10)
        {
            var apiKey = _configuration["NewsApi:ApiKey"];
            var url = $"https://newsapi.org/v2/top-headlines?country=us&apiKey={apiKey}";

            if (!string.IsNullOrEmpty(category))
            {
                url += $"&category={category}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(content);

            return newsResponse.Articles.Take(count)
                .Select(a => new Article
                {
                    Title = a.Title,
                    Content = a.Content,
                    Source = a.Source.Name,
                    SourceUrl = a.Url,
                    PublishedDate = a.PublishedAt,
                    Author = a.Author,
                    IsUserSubmitted = false
                }).ToList();
        }

        public async Task<Article> GetArticleByUrlAsync(string url)
        {
            // Use a web scraping service or a specialized API to fetch article content from URL
            // For now, we'll implement a simple placeholder

            return new Article
            {
                Title = "Article from URL",
                Content = "Content extracted from URL",
                Source = new Uri(url).Host,
                SourceUrl = url,
                PublishedDate = DateTime.UtcNow,
                IsUserSubmitted = false
            };
        }

        public Task<Article> CreateArticleFromTextAsync(string title, string content, string source = "User Submitted")
        {
            return Task.FromResult(new Article
            {
                Title = title,
                Content = content,
                Source = source,
                PublishedDate = DateTime.UtcNow,
                IsUserSubmitted = true
            });
        }

        private class NewsApiResponse
        {
            public List<NewsApiArticle> Articles { get; set; }
        }

        private class NewsApiArticle
        {
            public NewsApiSource Source { get; set; }
            public string Author { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public string UrlToImage { get; set; }
            public DateTime PublishedAt { get; set; }
            public string Content { get; set; }
        }

        private class NewsApiSource
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
