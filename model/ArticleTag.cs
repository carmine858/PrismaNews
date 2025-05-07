namespace PrismaNews.model
{
    public class ArticleTag
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public string Name { get; set; }
    }
}
