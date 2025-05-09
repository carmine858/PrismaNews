using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PrismaNews.Models;
using PrismaNews.Services;
using System.ComponentModel.DataAnnotations;

namespace PrismaNews.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterInput Input { get; set; }

        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            
            if (!ModelState.IsValid)
                return Page();

            if (Input.Password != Input.ConfirmPassword)
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Le password non corrispondono.");
                return Page();
            }

            var user = new User
            {
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName
            };

            var result = await _authService.RegisterUserAsync(user, Input.Password);

            if (!result)
            {
                ErrorMessage = "L'email è già registrata.";
                return Page();
            }

            return RedirectToPage("./RegisterConfirmation");
        }

        public class RegisterInput
        {
            [Required(ErrorMessage = "Il campo Email è obbligatorio")]
            [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Il campo Nome è obbligatorio")]
            [Display(Name = "Nome")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Il campo Cognome è obbligatorio")]
            [Display(Name = "Cognome")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Il campo Password è obbligatorio")]
            [StringLength(100, ErrorMessage = "La {0} deve essere almeno di {2} caratteri.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Il campo Conferma password è obbligatorio")]
            [DataType(DataType.Password)]
            [Display(Name = "Conferma password")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Ho letto e accetto i termini e le condizioni")]
            [Range(typeof(bool), "true", "true", ErrorMessage = "Devi accettare i termini e le condizioni")]
            public bool AcceptTerms { get; set; }
        }
    }
}
