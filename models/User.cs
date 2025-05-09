using System.ComponentModel.DataAnnotations;

namespace PrismaNews.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public DateTime RegistrationDate { get; set; }
        
        public bool EmailConfirmed { get; set; }
        
        public string VerificationToken { get; set; }
        
        public DateTime? TokenExpiryDate { get; set; }
    }
}
