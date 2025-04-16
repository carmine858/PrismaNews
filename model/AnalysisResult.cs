namespace PrismaNews.model
{
    public class AnalysisResult
    {
        public int Id { get; set; }

        // Collegamento con la notizia
        public int NewsArticleId { get; set; }
        public NewsArticle NewsArticle { get; set; }

        public string Summary { get; set; }

        public string Bias { get; set; }

        public string Framing { get; set; }

        // JSON string per controargomentazioni
        public string CounterargumentsJson { get; set; }

        // JSON string per prospettive alternative
        public string AlternativeViewsJson { get; set; }

        // JSON string per informazioni mancanti
        public string MissingInfoJson { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
