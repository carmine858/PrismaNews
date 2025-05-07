using Microsoft.EntityFrameworkCore;
using PrismaNews.model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrismaNews.Data
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly PrismaNewsDbContext _context;

        public ArticleRepository(PrismaNewsDbContext context)
        {
            _context = context;
        }

        public async Task<Article> GetByIdAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Tags)
                .Include(a => a.Analysis)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Article>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            // SQLite optimization: load smaller chunks and avoid complex ordering
            return await _context.Articles
                .Include(a => a.Tags)
                .OrderByDescending(a => a.Id) // Using ID instead of date can be more efficient in SQLite
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Article>> GetByCategoryAsync(string category, int page = 1, int pageSize = 10)
        {
            // SQLite is case-sensitive by default, so use ToLower() on both sides
            var categoryLower = category.ToLower();
            return await _context.Articles
                .Include(a => a.Tags)
                .Where(a => a.Category.ToLower() == categoryLower)
                .OrderByDescending(a => a.Id) // Using ID instead of date can be more efficient
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> AddAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article.Id;
        }

        public async Task UpdateAsync(Article article)
        {
            _context.Articles.Update(article);

            // SQLite sometimes has issues with concurrency
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Se c'è un problema di concorrenza, ricarica l'entità e riprova
                var entry = _context.Entry(article);
                entry.State = EntityState.Detached;

                var reloadedArticle = await _context.Articles.FindAsync(article.Id);
                if (reloadedArticle != null)
                {
                    _context.Entry(reloadedArticle).CurrentValues.SetValues(article);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<ArticleAnalysis> GetAnalysisForArticleAsync(int articleId)
        {
            return await _context.ArticleAnalyses
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);
        }

        public async Task<int> AddAnalysisAsync(ArticleAnalysis analysis)
        {
            _context.ArticleAnalyses.Add(analysis);
            await _context.SaveChangesAsync();
            return analysis.Id;
        }
    }
}