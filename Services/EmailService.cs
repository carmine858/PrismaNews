using Microsoft.Extensions.Configuration;

namespace PrismaNews.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationEmailAsync(string email, string token)
        {
            // In un'applicazione reale, qui implementeresti il servizio email
            // Uso variabile di configurazione
            var apiKey = _configuration["EmailSettings:ApiKey"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];

            // Per ora, simula l'invio
            await Task.CompletedTask;
            Console.WriteLine($"Verification email sent to {email} with token {token}");
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            // Simula l'invio di email di reset password
            await Task.CompletedTask;
            Console.WriteLine($"Password reset email sent to {email} with token {token}");
        }
    }
}
