using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using PrismaNews.Data;
using PrismaNews.ViewModels;

namespace PrismaNews.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly IArticleRepository _repo;

        public DetailsModel(IArticleRepository repo)
        {
            _repo = repo;
        }

        [BindProperty]
        public ArticleDetailsViewModel Article { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return NotFound();

            Article = new ArticleDetailsViewModel
            {
                Id = entity.Id,
                Title = entity.Title,
                SourceUrl = entity.SourceUrl,
                Content = entity.Content,
                BiasDetection = entity.Analysis?.BiasDetection,
                RhetoricalTechniques = entity.Analysis?.RhetoricalTechniques,
                AlternativePerspectives = entity.Analysis?.AlternativePerspectives,
                RelevantOmissions = entity.Analysis?.RelevantOmissions,
                ScientificReferences = entity.Analysis?.ScientificReferences,
                CommercialInterests = entity.Analysis?.CommercialInterests,
                CounterArguments = entity.Analysis?.CounterArguments
            };

            return Page();
        }
    }
}