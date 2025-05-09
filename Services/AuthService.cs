using Microsoft.EntityFrameworkCore;
using PrismaNews.Data;
using PrismaNews.Models;
using System.Security.Cryptography;
using System.Text;

namespace PrismaNews.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AuthService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> RegisterUserAsync(User user, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return false;
            }

            user.PasswordHash = HashPassword(password);
            user.RegistrationDate = DateTime.UtcNow;
            user.EmailConfirmed = false;
            user.VerificationToken = GenerateToken();
            user.TokenExpiryDate = DateTime.UtcNow.AddDays(1);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Invia email di verifica
            await _emailService.SendVerificationEmailAsync(user.Email, user.VerificationToken);

            return true;
        }

        public async Task<User> AuthenticateUserAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
                return null;
                
            if (!user.EmailConfirmed)
                return null;
                
            if (!VerifyPassword(password, user.PasswordHash))
                return null;
                
            return user;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.VerificationToken == token && 
                u.TokenExpiryDate > DateTime.UtcNow);
                
            if (user == null)
                return false;
                
            user.EmailConfirmed = true;
            user.VerificationToken = null;
            user.TokenExpiryDate = null;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
                return false;
                
            user.VerificationToken = GenerateToken();
            user.TokenExpiryDate = DateTime.UtcNow.AddHours(1);
            
            await _context.SaveChangesAsync();
            
            // Invia email di reset password
            await _emailService.SendPasswordResetEmailAsync(email, user.VerificationToken);
            
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.VerificationToken == token && 
                u.TokenExpiryDate > DateTime.UtcNow);
                
            if (user == null)
                return false;
                
            user.PasswordHash = HashPassword(newPassword);
            user.VerificationToken = null;
            user.TokenExpiryDate = null;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string GenerateToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
