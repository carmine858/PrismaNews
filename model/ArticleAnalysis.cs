namespace PrismaNews.model
{
    public class ArticleAnalysis
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public string BiasDetection { get; set; }
        public string RhetoricalTechniques { get; set; }
        public string AlternativePerspectives { get; set; }
        public string RelevantOmissions { get; set; }
        public string ScientificReferences { get; set; }
        public string CommercialInterests { get; set; }
        public string CounterArguments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
