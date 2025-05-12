using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prisma.Data;
using Prisma.Models;
using Prisma.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Prisma.Pages.Analysis
{
    public class AnalyzeModel : PageModel
    {
        private readonly ILogger<AnalyzeModel> _logger;
        private readonly INewsService _newsService;
        private readonly NewsScraperService _scraperService;
        private readonly GeminiService _geminiService;
        private readonly PrismaDbContext _context;

        [BindProperty(SupportsGet = true)]
        public string ArticleId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ArticleUrl { get; set; }

        public NewsArticle Article { get; private set; }
        public string ArticleContent { get; private set; }
        public AnalysisResult Analysis { get; private set; }
        public string ErrorMessage { get; private set; }
        public bool IsLoading { get; private set; } = true;
        public AnalysisStep CurrentStep { get; private set; } = AnalysisStep.Starting;

        [TempData]
        public string SaveMessage { get; set; }

        // Nuova proprietà per indicare se l'analisi è già stata salvata
        public bool IsAlreadySaved { get; private set; }

        public enum AnalysisStep
        {
            Starting,
            FetchingArticle,
            ScrapingContent,
            AnalyzingContent,
            Complete,
            Error
        }

        public AnalyzeModel(
           ILogger<AnalyzeModel> logger,
           INewsService newsService,
           NewsScraperService scraperService,
           GeminiService geminiService,
           PrismaDbContext context)
          
        {
            _logger = logger;
            _newsService = newsService;
            _scraperService = scraperService;
            _geminiService = geminiService;
            _context = context;
            
        }


        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation($"Starting analysis for ArticleId: {ArticleId}, ArticleUrl: {ArticleUrl}");

                // Se non abbiamo né ID né URL, reindirizza alla home
                if (string.IsNullOrEmpty(ArticleId) && string.IsNullOrEmpty(ArticleUrl))
                {
                    return RedirectToPage("/Index");
                }

                // Step 1: Ottieni l'articolo
                CurrentStep = AnalysisStep.FetchingArticle;

                if (!string.IsNullOrEmpty(ArticleId))
                {
                    // Prova prima con l'API Guardian
                    if (ArticleId.StartsWith("guardian/"))
                    {
                        var guardianId = ArticleId.Replace("guardian/", "");
                        Article = await _newsService.GetArticleByIdAsync(ArticleId);
                    }

                    // Se non è stato trovato tramite API, prova tramite servizio news
                    if (Article == null)
                    {
                        Article = await _newsService.GetArticleByIdAsync(ArticleId);
                    }

                    if (Article == null)
                    {
                        ErrorMessage = $"Impossibile trovare l'articolo con ID: {ArticleId}";
                        CurrentStep = AnalysisStep.Error;
                        IsLoading = false;
                        return Page();
                    }
                }
                else if (!string.IsNullOrEmpty(ArticleUrl))
                {
                    // Crea un oggetto articolo basato sull'URL
                    Article = new NewsArticle
                    {
                        Id = Guid.NewGuid().ToString(),
                        Url = ArticleUrl,
                        Title = "Articolo da URL esterno",
                        Section = DetermineCategoryFromUrl(ArticleUrl),
                        PublicationDate = DateTime.Now
                    };
                }

                // Step 2: Estrai il contenuto dell'articolo
                CurrentStep = AnalysisStep.ScrapingContent;

                try
                {
                    ArticleContent = await _scraperService.ExtractArticleContentAsync(Article.Url);

                    if (string.IsNullOrEmpty(ArticleContent) || ArticleContent.Length < 100)
                    {
                        ErrorMessage = "Contenuto dell'articolo insufficiente per l'analisi.";
                        CurrentStep = AnalysisStep.Error;
                        IsLoading = false;
                        return Page();
                    }

                    // Aggiorna il titolo se non lo abbiamo
                    if (Article.Title == "Articolo da URL esterno" && ArticleContent.Length > 0)
                    {
                        var firstLine = ArticleContent.Split('\n')[0];
                        if (firstLine.Length > 10 && firstLine.Length < 200)
                        {
                            Article.Title = firstLine;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Errore nell'estrazione del contenuto da {Article.Url}");
                    ErrorMessage = $"Impossibile estrarre il contenuto dell'articolo: {ex.Message}";
                    CurrentStep = AnalysisStep.Error;
                    IsLoading = false;
                    return Page();
                }

                // Step 3: Analizza il contenuto con Gemini
                CurrentStep = AnalysisStep.AnalyzingContent;

                try
                {
                    Analysis = await _geminiService.AnalyzeArticleAsync(Article, ArticleContent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore nell'analisi con Gemini");
                    ErrorMessage = $"Errore durante l'analisi con Gemini: {ex.Message}";
                    CurrentStep = AnalysisStep.Error;
                    IsLoading = false;
                    return Page();
                }

                // Completato
                CurrentStep = AnalysisStep.Complete;
                IsLoading = false;

                if (User.Identity.IsAuthenticated)
                {
                    // Ottieni l'ID dell'utente corrente dal claim
                    if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                    {
                        IsAlreadySaved = await _context.SavedAnalyses.AnyAsync(
                            sa => sa.UserId == userId && sa.ArticleUrl == Article.Url);
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generale nel processo di analisi");
                ErrorMessage = $"Si è verificato un errore: {ex.Message}";
                CurrentStep = AnalysisStep.Error;
                IsLoading = false;
                return Page();
            }
        }
        public async Task<IActionResult> OnPostSaveAnalysisAsync(string articleId, string articleTitle, string articleUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
                SaveMessage = "Devi accedere per salvare questa analisi.";
                return RedirectToPage(new { articleId = articleId });
            }

            try
            {
                // Ottieni l'ID dell'utente corrente dal claim
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                {
                    SaveMessage = "Errore nell'identificazione dell'utente.";
                    return RedirectToPage(new { articleId = articleId });
                }

                // Verifica se l'analisi è già stata salvata
                bool alreadySaved = await _context.SavedAnalyses.AnyAsync(
                    sa => sa.UserId == userId && sa.ArticleUrl == articleUrl);

                if (alreadySaved)
                {
                    SaveMessage = "Analisi già presente nei preferiti.";
                    return RedirectToPage(new { articleId = articleId });
                }

                // Carica l'articolo e l'analisi
                Article = await _newsService.GetArticleByIdAsync(articleId);
                ArticleContent = await _scraperService.ExtractArticleContentAsync(articleUrl);
                Analysis = await _geminiService.AnalyzeArticleAsync(Article, ArticleContent);

                // Crea un nuovo record SavedAnalysis
                var savedAnalysis = new SavedAnalysis
                {
                    UserId = userId,
                    ArticleTitle = articleTitle,
                    ArticleUrl = articleUrl,
                    GeminiAnalysisJson = Analysis.RawAnalysisJson,
                    SavedAt = DateTime.Now
                };

                // Salva nel database
                await _context.SavedAnalyses.AddAsync(savedAnalysis);
                await _context.SaveChangesAsync();

                SaveMessage = "Analisi salvata con successo!";

                return RedirectToPage(new { articleId = articleId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il salvataggio dell'analisi");
                SaveMessage = $"Errore durante il salvataggio: {ex.Message}";
                return RedirectToPage(new { articleId = articleId });
            }
        }

        private string DetermineCategoryFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "General";

            url = url.ToLower();

            if (url.Contains("/politics/"))
                return "Politics";
            else if (url.Contains("/environment/") || url.Contains("/climate/"))
                return "Environment";
            else if (url.Contains("/technology/") || url.Contains("/tech/"))
                return "Technology";
            else if (url.Contains("/business/") || url.Contains("/economy/"))
                return "Economy";
            else if (url.Contains("/society/") || url.Contains("/culture/"))
                return "Society";

            return "General";
        }
    }
}