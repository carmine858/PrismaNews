using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Prisma.Services
{
    public static class WebScrapingExtensions
    {
        public static IServiceCollection AddWebScraper(this IServiceCollection services)
        {
            // Configurazione di un HttpClient ottimizzato per lo scraping
            services.AddHttpClient<NewsScraperService>(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                UseCookies = true,
                CookieContainer = new CookieContainer()
            });

            return services;
        }
    }
}