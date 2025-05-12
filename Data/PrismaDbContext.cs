using Microsoft.EntityFrameworkCore;
using Prisma.Models;
using System.Runtime.ConstrainedExecution;

namespace Prisma.Data
{
    public class PrismaDbContext : DbContext
    {
        public PrismaDbContext(DbContextOptions<PrismaDbContext> options) : base(options)
        {
        }
        public DbSet<IdeologicalAnalysis> IdeologicalAnalyses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SavedAnalysis> SavedAnalyses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurazione dei modelli
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<SavedAnalysis>()
              .HasIndex(s => new { s.UserId, s.ArticleUrl })
              .IsUnique(); // Previene duplicati per lo stesso articolo per utente

            modelBuilder.Entity<IdeologicalAnalysis>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ArticleId).IsRequired();
                entity.Property(e => e.XAxis).IsRequired();
                entity.Property(e => e.YAxis).IsRequired();
                entity.Property(e => e.Explanation).IsRequired();
                entity.HasIndex(e => e.ArticleId).IsUnique();
            });
        }
    }
}