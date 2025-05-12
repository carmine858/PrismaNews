using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Prisma.Models;
using Prisma.Services;
using System.Text;

namespace Prisma.Pages.Analysis
{
    public class NarrativeLensModel : PageModel
    {
        private readonly NarrativeLensService _narrativeLensService;
        private readonly INewsService _newsService;
        private readonly NewsScraperService _scraperService;
        private readonly ILogger<NarrativeLensModel> _logger;

        public NarrativeLens NarrativeLens { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string ArticleId { get; set; }
        public bool IsLoading { get; private set; }
        public string ErrorMessage { get; private set; }

        public NarrativeLensModel(
            NarrativeLensService narrativeLensService,
            INewsService newsService,
            NewsScraperService scraperService,
            ILogger<NarrativeLensModel> logger)
        {
            _narrativeLensService = narrativeLensService;
            _newsService = newsService;
            _scraperService = scraperService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
            {
                ErrorMessage = "Nessun articolo selezionato";
                return Page();
            }

            ArticleId = articleId;
            IsLoading = true;

            try
            {
                var article = await _newsService.GetArticleByIdAsync(ArticleId);
                if (article == null)
                {
                    ErrorMessage = "Articolo non trovato";
                    return Page();
                }

                var content = await _scraperService.ExtractArticleContentAsync(article.Url);

                NarrativeLens = await _narrativeLensService.GenerateNarrativesAsync(
                    articleId, content, article.Title);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating narrative lens for article {ArticleId}", articleId);
                ErrorMessage = "Si è verificato un errore durante la generazione delle prospettive";
                return Page();
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Metodi helper accessibili dalla vista Razor
        public string GetBackgroundColor(PerspectiveType perspectiveType)
        {
            return perspectiveType switch
            {
                PerspectiveType.Activist => "linear-gradient(135deg, #4CAF50, #8BC34A)",
                PerspectiveType.Politician => "linear-gradient(135deg, #3F51B5, #2196F3)",
                PerspectiveType.Expert => "linear-gradient(135deg, #FF9800, #FFEB3B)",
                PerspectiveType.Victim => "linear-gradient(135deg, #E91E63, #9C27B0)",
                _ => "linear-gradient(135deg, #607D8B, #90A4AE)"
            };
        }

        public string FormatContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            // Convert newlines to paragraph tags
            var paragraphs = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var formattedContent = new StringBuilder();

            foreach (var paragraph in paragraphs)
            {
                if (!string.IsNullOrWhiteSpace(paragraph))
                {
                    formattedContent.AppendLine($"<p>{paragraph}</p>");
                }
            }

            return formattedContent.ToString();
        }
    }
}