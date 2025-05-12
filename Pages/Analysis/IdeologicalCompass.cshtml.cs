using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prisma.Data;
using Prisma.Models;
using Prisma.Services;

namespace Prisma.Pages.Analysis
{
    public class IdeologicalCompassModel : PageModel
    {
        private readonly ILogger<IdeologicalCompassModel> _logger;
        private readonly PrismaDbContext _context;
        private readonly INewsService _newsService;
        private readonly NewsScraperService _scraperService;
        private readonly GeminiService _geminiService;

        [BindProperty(SupportsGet = true)]
        public string ArticleId { get; set; }

        public IdeologicalAnalysis CurrentAnalysis { get; set; }
        public List<IdeologicalAnalysis> OtherAnalyses { get; set; } = new List<IdeologicalAnalysis>();
        public List<IdeologicalAnalysis> RecentAnalyses { get; set; } = new List<IdeologicalAnalysis>();
        public string ArticleTitle { get; set; }
        public string ErrorMessage { get; set; }

        public IdeologicalCompassModel(
            ILogger<IdeologicalCompassModel> logger,
            PrismaDbContext context,
            INewsService newsService,
            NewsScraperService scraperService,
            GeminiService geminiService)
        {
            _logger = logger;
            _context = context;
            _newsService = newsService;
            _scraperService = scraperService;
            _geminiService = geminiService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(ArticleId))
            {
                // Se non viene fornito un ArticleId, carica solo le analisi recenti
                await LoadRecentAnalysesAsync();
                return Page();
            }

            try
            {
                // Verifica se esiste già un'analisi per questo articolo
                var existingAnalysis = await _context.IdeologicalAnalyses
                    .FirstOrDefaultAsync(a => a.ArticleId == ArticleId);

                if (existingAnalysis != null)
                {
                    _logger.LogInformation($"Analisi ideologica trovata per l'articolo {ArticleId}");
                    CurrentAnalysis = existingAnalysis;
                    ArticleTitle = existingAnalysis.ArticleTitle;
                }
                else
                {
                    _logger.LogInformation($"Generazione nuova analisi ideologica per l'articolo {ArticleId}");
                    // Recupera l'articolo e il contenuto
                    var article = await _newsService.GetArticleByIdAsync(ArticleId);
                    if (article == null)
                    {
                        ErrorMessage = "Articolo non trovato.";
                        return Page();
                    }

                    ArticleTitle = article.Title;

                    // Ottieni il contenuto dell'articolo
                    var content = await _scraperService.ExtractArticleContentAsync(article.Url);

                    // Esegui l'analisi ideologica
                    CurrentAnalysis = await _geminiService.AnalyzeIdeologyAsync(article, content);

                    // Salva l'analisi nel database
                    await _context.IdeologicalAnalyses.AddAsync(CurrentAnalysis);
                    await _context.SaveChangesAsync();
                }

                // Carica altre analisi per il grafico di confronto (massimo 20)
                OtherAnalyses = await _context.IdeologicalAnalyses
                    .Where(a => a.ArticleId != ArticleId)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(20)
                    .ToListAsync();

                // Carica analisi recenti per la tabella
                await LoadRecentAnalysesAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento o l'analisi ideologica");
                ErrorMessage = $"Si è verificato un errore: {ex.Message}";
                return Page();
            }
        }

        private async Task LoadRecentAnalysesAsync()
        {
            RecentAnalyses = await _context.IdeologicalAnalyses
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();
        }
    }
}