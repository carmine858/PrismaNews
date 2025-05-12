using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;

namespace Prisma.Services
{
    public class NewsScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NewsScraperService> _logger;

        public NewsScraperService(HttpClient httpClient, ILogger<NewsScraperService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configurazione per evitare problemi con i siti di news
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
        }

        public async Task<string> ExtractArticleContentAsync(string url)
        {
            try
            {
                _logger.LogInformation($"Extracting content from: {url}");

                // Download del contenuto HTML
                var html = await _httpClient.GetStringAsync(url);

                // Parsing HTML
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Estrai il dominio per usare lo scraper appropriato
                var uri = new Uri(url);
                var domain = uri.Host.ToLower();

                string content;
                if (domain.Contains("theguardian.com"))
                {
                    content = ExtractGuardianContent(htmlDoc);
                }
                else
                {
                    // Fallback a strategia generica
                    content = ExtractGenericContent(htmlDoc);
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning($"Failed to extract content from {url}");
                    return "Impossibile estrarre il contenuto dell'articolo.";
                }

                _logger.LogInformation($"Successfully extracted {content.Length} characters");
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting content from {url}");
                throw;
            }
        }

        private string ExtractGuardianContent(HtmlDocument htmlDoc)
        {
            var contentBuilder = new StringBuilder();

            // Estrai il titolo
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode(
                "//h1[contains(@class, 'dcr-y70mar')] | " +
                "//h1[contains(@class, 'headline')] | " +
                "//h1");

            if (titleNode != null)
            {
                contentBuilder.AppendLine(CleanText(titleNode.InnerText));
                contentBuilder.AppendLine();
            }

            // Estrai il sottotitolo/standfirst
            var standfirstNode = htmlDoc.DocumentNode.SelectSingleNode(
                "//div[contains(@class, 'dcr-1aul2ye')] | " +
                "//div[contains(@class, 'content__standfirst')]");

            if (standfirstNode != null)
            {
                contentBuilder.AppendLine(CleanText(standfirstNode.InnerText));
                contentBuilder.AppendLine();
            }

            // Estrai il corpo dell'articolo
            var contentNodes = htmlDoc.DocumentNode.SelectNodes(
                "//div[contains(@class, 'article-body-commercial-selector')]//p | " +
                "//div[contains(@class, 'dcr-1eve8g9')]//p | " +
                "//div[contains(@class, 'content__article-body')]//p");

            if (contentNodes != null)
            {
                foreach (var node in contentNodes)
                {
                    var text = CleanText(node.InnerText);
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 20)
                    {
                        contentBuilder.AppendLine(text);
                        contentBuilder.AppendLine();
                    }
                }
            }

            return contentBuilder.ToString();
        }

        private string ExtractGenericContent(HtmlDocument htmlDoc)
        {
            var contentBuilder = new StringBuilder();

            // Estrai il titolo
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1");
            if (titleNode != null)
            {
                contentBuilder.AppendLine(CleanText(titleNode.InnerText));
                contentBuilder.AppendLine();
            }

            // Prova diversi selettori comuni per i contenuti degli articoli
            var contentNodes = htmlDoc.DocumentNode.SelectNodes(
                "//article//p | " +
                "//*[contains(@class, 'article')]//p | " +
                "//*[contains(@class, 'content')]//p | " +
                "//div[contains(@class, 'entry')]//p");

            if (contentNodes != null)
            {
                foreach (var node in contentNodes)
                {
                    var text = CleanText(node.InnerText);
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 20)
                    {
                        contentBuilder.AppendLine(text);
                        contentBuilder.AppendLine();
                    }
                }
            }

            return contentBuilder.ToString();
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Decodifica le entità HTML
            text = System.Net.WebUtility.HtmlDecode(text);

            // Rimuovi tag HTML
            text = Regex.Replace(text, "<.*?>", string.Empty);

            // Normalizza spazi bianchi
            text = Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }
    }
}