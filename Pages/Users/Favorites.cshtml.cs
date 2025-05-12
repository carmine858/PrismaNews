using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Prisma.Data;
using Prisma.Models;

namespace Prisma.Pages.User
{
    [Authorize] // Solo utenti autenticati possono accedere
    public class FavoritesModel : PageModel
    {
        private readonly PrismaDbContext _context;

        public List<SavedAnalysisViewModel> SavedAnalyses { get; set; } = new List<SavedAnalysisViewModel>();

        [TempData]
        public string StatusMessage { get; set; }

        public FavoritesModel(PrismaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Ottieni l'ID dell'utente corrente dal claim
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return Challenge(); // Reindirizza al login
            }

            // Carica le analisi salvate dell'utente
            var savedAnalyses = await _context.SavedAnalyses
                .Where(sa => sa.UserId == userId)
                .OrderByDescending(sa => sa.SavedAt)
                .ToListAsync();

            // Converte in ViewModel
            SavedAnalyses = savedAnalyses.Select(sa => new SavedAnalysisViewModel
            {
                Id = sa.Id,
                ArticleTitle = sa.ArticleTitle,
                ArticleUrl = sa.ArticleUrl,
                SavedAt = sa.SavedAt,
                ArticleId = ExtractArticleId(sa.ArticleUrl),
                Analysis = DeserializeAnalysis(sa.GeminiAnalysisJson)
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteSavedAsync(int savedAnalysisId)
        {
            // Ottieni l'ID dell'utente corrente dal claim
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return Challenge(); // Reindirizza al login
            }

            // Trova l'analisi salvata
            var savedAnalysis = await _context.SavedAnalyses
                .FirstOrDefaultAsync(sa => sa.Id == savedAnalysisId && sa.UserId == userId);

            if (savedAnalysis != null)
            {
                _context.SavedAnalyses.Remove(savedAnalysis);
                await _context.SaveChangesAsync();

                StatusMessage = "Analisi rimossa dai preferiti.";
            }

            return RedirectToPage();
        }

        // Helper per estrarre l'ID dell'articolo dall'URL
        private string ExtractArticleId(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            // Implementa la logica per estrarre l'ID articolo dall'URL
            // Questo può variare in base alla struttura del tuo URL
            try
            {
                // Esempio: /news/guardian/technology/12345
                if (url.Contains("/guardian/"))
                {
                    return "guardian/" + url.Split("/").Last();
                }

                // Fallback: Usa l'ultima parte dell'URL
                return url.Split("/").Last();
            }
            catch
            {
                return string.Empty;
            }
        }

        // Helper per deserializzare l'analisi JSON
        private AnalysisResult DeserializeAnalysis(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<AnalysisResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception)
            {
                return null; // In caso di errore nella deserializzazione
            }
        }
    }

    // ViewModel per visualizzare analisi salvate
    public class SavedAnalysisViewModel
    {
        public int Id { get; set; }
        public string ArticleTitle { get; set; }
        public string ArticleUrl { get; set; }
        public string ArticleId { get; set; }
        public DateTime SavedAt { get; set; }
        public AnalysisResult Analysis { get; set; }
    }
}