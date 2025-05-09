using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PrismaNews.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Reindirizza alla pagina di login
            return RedirectToPage("/Account/Login");
            
            // Alternativa: reindirizza solo gli utenti non autenticati
            // if (!User.Identity.IsAuthenticated)
            // {
            //     return RedirectToPage("/Account/Login");
            // }
            // return Page();
        }
    }
}