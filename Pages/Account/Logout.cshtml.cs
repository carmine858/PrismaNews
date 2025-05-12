using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Prisma.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // L'utente è arrivato direttamente alla pagina di logout, mostra il form di conferma
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // L'utente ha confermato il logout
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("Utente {Username} ha eseguito il logout", User.Identity.Name);

            returnUrl = returnUrl ?? Url.Content("~/");

            TempData["StatusMessage"] = "Hai eseguito il logout con successo.";
            return LocalRedirect(returnUrl);
        }
    }
}