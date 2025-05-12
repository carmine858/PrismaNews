using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Prisma.Data;
using Prisma.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Prisma.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly PrismaDbContext _context;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(PrismaDbContext context, ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Il campo Email è obbligatorio")]
            [EmailAddress(ErrorMessage = "Il formato dell'email non è valido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Il campo Password è obbligatorio")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Ricordami")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true; // Opzionale, se vuoi nascondere anche il footer
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Input.Email);

                if (user != null && VerifyPassword(Input.Password, user.PasswordHash, user.Salt))
                {
                    // Verifica che l'email sia stata confermata
                    if (!user.EmailConfirmed)
                    {
                        _logger.LogWarning("Tentativo di accesso con email non confermata: {Email}", Input.Email);
                        ModelState.AddModelError(string.Empty, "Devi confermare la tua email prima di accedere. Controlla la tua casella di posta.");
                        return Page();
                    }

                    // Verifica che l'account sia attivo
                    if (!user.IsActive)
                    {
                        _logger.LogWarning("Tentativo di accesso con account disabilitato: {Email}", Input.Email);
                        ModelState.AddModelError(string.Empty, "Questo account è stato disabilitato. Contatta l'assistenza per maggiori informazioni.");
                        return Page();
                    }

                    _logger.LogInformation("Utente {Username} autenticato con successo", user.Username);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = Input.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(Input.RememberMe ? 30 : 1)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return LocalRedirect(returnUrl);
                }

                _logger.LogWarning("Tentativo di login non riuscito per l'utente: {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Email o password non validi.");
            }

            // Se siamo arrivati qui, qualcosa è andato storto, rimostra il form
            return Page();
        }

        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPasswordBytes = new byte[passwordBytes.Length + saltBytes.Length];

                Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
                Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, passwordBytes.Length, saltBytes.Length);

                byte[] hashBytes = sha256.ComputeHash(saltedPasswordBytes);
                string computedHash = Convert.ToBase64String(hashBytes);

                return computedHash == storedHash;
            }
        }
    }
}