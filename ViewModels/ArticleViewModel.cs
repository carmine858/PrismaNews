namespace PrismaNews.ViewModels
{
    public class ArticleViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public DateTime PublishedDate { get; set; }
        public string Snippet { get; set; }
    }
}