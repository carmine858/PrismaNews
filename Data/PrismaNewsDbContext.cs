using Microsoft.EntityFrameworkCore;
using PrismaNews.model;
namespace PrismaNews.Data
{
    public class PrismaNewsDbContext : DbContext
    {
        public PrismaNewsDbContext(DbContextOptions<PrismaNewsDbContext> options)
           : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleAnalysis> ArticleAnalyses { get; set; }
        public DbSet<ArticleTag> ArticleTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<ArticleAnalysis>()
                .HasOne(a => a.Article)
                .WithOne(a => a.Analysis)
                .HasForeignKey<ArticleAnalysis>(a => a.ArticleId);

            modelBuilder.Entity<ArticleTag>()
                .HasOne(t => t.Article)
                .WithMany(a => a.Tags)
                .HasForeignKey(t => t.ArticleId);
        }


        
    }
}
