using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Prisma.Data;
using Prisma.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Prisma.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly PrismaDbContext _context;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(PrismaDbContext context, ILogger<RegisterModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Il campo Username è obbligatorio")]
            [StringLength(50, ErrorMessage = "Lo username deve avere tra {2} e {1} caratteri.", MinimumLength = 3)]
            [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Lo username può contenere solo lettere, numeri, punti, trattini e underscore.")]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Il campo Email è obbligatorio")]
            [EmailAddress(ErrorMessage = "Il formato dell'email non è valido")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Il campo Password è obbligatorio")]
            [StringLength(100, ErrorMessage = "La password deve avere almeno {2} caratteri.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
             ErrorMessage = "La password deve contenere almeno 8 caratteri, una lettera minuscola, una maiuscola, un numero e un carattere speciale.")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Conferma password")]
            [Compare("Password", ErrorMessage = "La password di conferma non corrisponde.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Accetto i termini e le condizioni")]
            [Range(typeof(bool), "true", "true", ErrorMessage = "Devi accettare i termini e le condizioni per registrarti.")]
            public bool AcceptTerms { get; set; }
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
                // Verifica che email e username non siano già in uso
                if (await _context.Users.AnyAsync(u => u.Email == Input.Email))
                {
                    ModelState.AddModelError("Input.Email", "Questa email è già in uso.");
                    return Page();
                }

                if (await _context.Users.AnyAsync(u => u.Username == Input.Username))
                {
                    ModelState.AddModelError("Input.Username", "Questo username è già in uso.");
                    return Page();
                }

                // Genera salt e hash per la password
                var salt = GenerateSalt();
                var passwordHash = HashPassword(Input.Password, salt);

                // Genera un token di verifica
                var verificationToken = Guid.NewGuid().ToString("N");

                // Crea un nuovo utente
                var user = new Prisma.Models.User
                {
                    Username = Input.Username,
                    Email = Input.Email,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    RegistrationDate = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true, // Per semplicità impostiamo a true
                    VerificationToken = verificationToken,
                    TokenExpiryDate = DateTime.UtcNow.AddDays(7) // Il token è valido per 7 giorni
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Utente {Username} registrato con successo", user.Username);

                // In una situazione reale qui invieresti una mail di verifica
                // await SendVerificationEmailAsync(user);

                // Per semplicità, consideriamo l'utente già verificato
                // e lo indirizziamo direttamente alla pagina di login con un messaggio
                TempData["StatusMessage"] = "Registrazione completata con successo! Ora puoi accedere al tuo account.";
                return RedirectToPage("/Account/Login");
            }

            // Se siamo arrivati qui, qualcosa è andato storto, rimostra il form
            return Page();
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPasswordBytes = new byte[passwordBytes.Length + saltBytes.Length];

                Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
                Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, passwordBytes.Length, saltBytes.Length);

                byte[] hashBytes = sha256.ComputeHash(saltedPasswordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}