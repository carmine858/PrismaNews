using PrismaNews.Models;

namespace PrismaNews.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(User user, string password);
        Task<User> AuthenticateUserAsync(string email, string password);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
