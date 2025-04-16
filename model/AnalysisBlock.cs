namespace PrismaNews.model
{
    public class AnalysisBlock
    {
        
        public int Id { get; set; }
        public int AnalysisResultId { get; set; }
        public AnalysisResult AnalysisResult { get; set; }
        public string Type { get; set; } // Bias, Framing, Controprospettive, Omissioni, ecc.
        public string Content { get; set; }
    }
}
