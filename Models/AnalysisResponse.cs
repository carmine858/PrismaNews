using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PrismaNews.Models
{
    public class AnalysisResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }

        public class Candidate
        {
            [JsonPropertyName("content")]
            public Content Content { get; set; }

            [JsonPropertyName("finishReason")]
            public string FinishReason { get; set; }
        }

        public class Content
        {
            [JsonPropertyName("parts")]
            public List<Part> Parts { get; set; }
        }

        public class Part
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
        }
    }
}