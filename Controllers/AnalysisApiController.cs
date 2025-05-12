using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prisma.Data;
using Prisma.Models;

namespace Prisma.Controllers
{
    [ApiController]
    [Route("api/analysis")]
    public class AnalysisApiController : ControllerBase
    {
        private readonly PrismaDbContext _context;

        public AnalysisApiController(PrismaDbContext context)
        {
            _context = context;
        }

        [HttpPost("savepoliticalanalysis")]
        [Authorize]
        public async Task<IActionResult> SavePoliticalAnalysis([FromBody] SaveAnalysisRequest request)
        {
            if (string.IsNullOrEmpty(request.ArticleId))
            {
                return BadRequest("ArticleId è obbligatorio");
            }

            try
            {
                // Ottieni l'ID dell'utente corrente
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                {
                    return Unauthorized("Impossibile identificare l'utente");
                }

                // Trova l'analisi ideologica
                var analysis = await _context.IdeologicalAnalyses
                    .FirstOrDefaultAsync(a => a.ArticleId == request.ArticleId);

                if (analysis == null)
                {
                    return NotFound("Analisi non trovata");
                }

                // Verifica se l'analisi è già stata salvata
                var existingSaved = await _context.SavedAnalyses
            .FirstOrDefaultAsync(sa => sa.UserId == userId && sa.ArticleUrl == request.ArticleId);


                if (existingSaved != null)
                {
                    return Ok(new { message = "Analisi già salvata" });
                }

                // Salva l'analisi nei preferiti dell'utente
                var savedAnalysis = new SavedAnalysis
                {
                    UserId = userId,
                    ArticleUrl = analysis.ArticleId,
                    ArticleTitle = analysis.ArticleTitle,
                    
                    GeminiAnalysisJson = System.Text.Json.JsonSerializer.Serialize(analysis),
                    SavedAt = DateTime.Now
                };

                await _context.SavedAnalyses.AddAsync(savedAnalysis);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Analisi salvata con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno: {ex.Message}");
            }
        }
    }

    public class SaveAnalysisRequest
    {
        public string ArticleId { get; set; }
    }
}