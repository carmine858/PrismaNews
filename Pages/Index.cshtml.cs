using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PrismaNews.Data;
using PrismaNews.ViewModels;

namespace PrismaNews.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IArticleRepository _repo;

        public IndexModel(IArticleRepository repo)
        {
            _repo = repo;
        }

        public List<ArticleViewModel> Articles { get; set; }

        public async Task OnGetAsync()
        {
            var entities = await _repo.GetAllAsync(page: 1, pageSize: 10);
            Articles = entities.Select(a => new ArticleViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Source = a.Source,
                PublishedDate = a.PublishedDate,
                Snippet = a.Content.Length > 200 ? a.Content.Substring(0, 200) + "…" : a.Content
            }).ToList();
        }
    }
}