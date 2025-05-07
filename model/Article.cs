using System.ComponentModel.DataAnnotations;

namespace PrismaNews.model
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Source { get; set; }
        public string SourceUrl { get; set; }
        public DateTime PublishedDate { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public bool IsUserSubmitted { get; set; }
        public List<ArticleTag> Tags { get; set; }
        public ArticleAnalysis Analysis { get; set; }
    }
}
