using Microsoft.EntityFrameworkCore;
using PrismaNews.model;
namespace PrismaNews.Data
{
    public class PrismaNewsDbContext : DbContext
    {
        public PrismaNewsDbContext(DbContextOptions<PrismaNewsDbContext> options) : base(options)
        {
        }

        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<AnalysisResult> AnalysisResults { get; set; }
        
    }
}
