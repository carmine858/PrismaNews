using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Prisma.Models;
using Prisma.Services;

namespace Prisma.Services
{
    public class NarrativeLensService
    {
        private readonly GeminiService _geminiService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NarrativeLensService> _logger;

        public NarrativeLensService(GeminiService geminiService, IMemoryCache cache, ILogger<NarrativeLensService> logger)
        {
            _geminiService = geminiService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<NarrativeLens> GenerateNarrativesAsync(string articleId, string originalContent, string title)
        {
            // Check cache first
            string cacheKey = $"narrative_lens_{articleId}";
            if (_cache.TryGetValue(cacheKey, out NarrativeLens cachedLens))
            {
                return cachedLens;
            }

            var lens = new NarrativeLens
            {
                ArticleId = articleId,
                OriginalTitle = title,
                OriginalContent = originalContent,
                Perspectives = new List<Perspective>()
            };

            // Generate each perspective in parallel
            var tasks = new List<Task>();
            var perspectives = new[]
            {
                new { Type = PerspectiveType.Activist, Emoji = "🧑‍🌾", Name = "Attivista" },
                new { Type = PerspectiveType.Politician, Emoji = "🧑‍⚖️", Name = "Politico" },
                new { Type = PerspectiveType.Expert, Emoji = "👨‍🔬", Name = "Esperto" },
                new { Type = PerspectiveType.Victim, Emoji = "😢", Name = "Vittima" }
            };

            foreach (var p in perspectives)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await _geminiService.GeneratePerspectiveAsync(
                            originalContent,
                            title,
                            p.Type,
                            p.Name);

                        lock (lens.Perspectives)
                        {
                            lens.Perspectives.Add(new Perspective
                            {
                                Type = p.Type,
                                Title = result.Title,
                                Content = result.Content,
                                Summary = result.Summary,
                                Emoji = p.Emoji,
                                Name = p.Name,
                                Keywords = result.Keywords
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error generating {Perspective} perspective", p.Name);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Cache the result for 24 hours
            _cache.Set(cacheKey, lens, TimeSpan.FromHours(24));

            return lens;
        }
    }
}