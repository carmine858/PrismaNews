namespace PrismaNews.ViewModels
{
    public class ArticleDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SourceUrl { get; set; }
        public string Content { get; set; }
        public string BiasDetection { get; set; }
        public string RhetoricalTechniques { get; set; }
        public string AlternativePerspectives { get; set; }
        public string RelevantOmissions { get; set; }
        public string ScientificReferences { get; set; }
        public string CommercialInterests { get; set; }
        public string CounterArguments { get; set; }
    }
}