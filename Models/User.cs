using System.ComponentModel.DataAnnotations;

namespace Prisma.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Salt { get; set; }

        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public string VerificationToken { get; set; }

        public bool EmailConfirmed { get; set; } = false;

        public DateTime? TokenExpiryDate { get; set; }

        
        
    }
}