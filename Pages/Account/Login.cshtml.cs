using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PrismaNews.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PrismaNews.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginInput Input { get; set; }

        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }
        public bool ShowForgetPassword { get; set; } = false;
        public bool ShowResendVerification { get; set; } = false;

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            
            if (!ModelState.IsValid)
                return Page();

            var user = await _authService.AuthenticateUserAsync(Input.Email, Input.Password);

            if (user == null)
            {
                ErrorMessage = "Email o password non validi.";
                ShowForgetPassword = true;
                ShowResendVerification = true;
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = DateTime.UtcNow.AddDays(30)
                });

            return LocalRedirect(ReturnUrl);
        }

        public async Task<IActionResult> OnPostForgotPasswordAsync()
        {
            if (string.IsNullOrEmpty(Input.Email))
            {
                ErrorMessage = "Inserisci la tua email.";
                return Page();
            }

            await _authService.RequestPasswordResetAsync(Input.Email);
            
            // Non rivelare se l'email esiste o meno per motivi di sicurezza
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        public class LoginInput
        {
            [Required(ErrorMessage = "Il campo Email è obbligatorio")]
            [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Il campo Password è obbligatorio")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Ricordami")]
            public bool RememberMe { get; set; }
        }
    }
}
