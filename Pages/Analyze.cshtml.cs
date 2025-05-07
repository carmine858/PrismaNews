using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using PrismaNews.Controllers; // Assicurati che il namespace sia corretto

namespace PrismaNews.Pages
{
    public class AnalyzeModel : PageModel
    {
        private readonly ArticlesController _controller;

        public AnalyzeModel(ArticlesController controller)
        {
            _controller = controller;
        }

        [BindProperty]
        public new string? Url { get; set; } // Usa "new" per nascondere il membro ereditato e rendilo nullable

        [BindProperty]
        public new string? Content { get; set; } // Usa "new" per nascondere il membro ereditato e rendilo nullable

        public bool AnalysisResultAvailable { get; set; }
        public int? NewArticleId { get; set; } // Rendi nullable per evitare problemi di dereferenziamento

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(Url))
            {
                var result = await _controller.AnalyzeFromUrl(new ArticlesController.UrlAnalysisRequest { Url = Url });
                if (result is OkObjectResult ok && ok.Value is not null) // Controlla che ok.Value non sia null
                {
                    var data = (ok.Value as dynamic);
                    if (data?.Article?.Id != null) // Controlla che Article e Id non siano null
                    {
                        NewArticleId = (int)data.Article.Id;
                        AnalysisResultAvailable = true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(Content))
            {
                var result = await _controller.AnalyzeFromText(new ArticlesController.TextAnalysisRequest
                {
                    Title = "Articolo Incollato",
                    Content = Content
                });
                if (result is OkObjectResult ok && ok.Value is not null) // Controlla che ok.Value non sia null
                {
                    var data = (ok.Value as dynamic);
                    if (data?.Article?.Id != null) // Controlla che Article e Id non siano null
                    {
                        NewArticleId = (int)data.Article.Id;
                        AnalysisResultAvailable = true;
                    }
                }
            }

            return Page();
        }
    }
}
