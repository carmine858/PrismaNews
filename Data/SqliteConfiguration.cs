using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace PrismaNews.Data
{
    public static class SqliteConfiguration
    {
        public static void ConfigureDatabase(IServiceCollection services, string connectionString)
        {
            // Ensure directory exists for SQLite database file
            var dbPath = connectionString.Replace("Data Source=", "").Trim();
            var dbDirectory = Path.GetDirectoryName(dbPath);

            if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            // Configure SQLite options
            services.AddDbContext<PrismaNewsDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqliteOptions =>
                {
                    // Configure SQLite specific options
                    sqliteOptions.MigrationsAssembly(typeof(PrismaNewsDbContext).Assembly.FullName);
                });

                // Enable SQLite retries for better concurrency handling
                options.EnableSensitiveDataLogging(false); // Set to true only during development
                options.EnableDetailedErrors(false);       // Set to true only during development
            });
        }
    }
}
