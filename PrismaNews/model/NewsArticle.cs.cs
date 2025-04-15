using System.ComponentModel.DataAnnotations;

namespace PrismaNews.model
{
    public class NewsArticle
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        public string Url { get; set; }

        public DateTime PublishedAt { get; set; }

        public string Source { get; set; }

        // Navigazione: una notizia ha una o più analisi associate
        public List<AnalysisResult> AnalysisResults { get; set; }
    }
}
