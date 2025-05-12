using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PrismaNews.Models
{
    public class AnalysisRequest
    {
        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; } = new List<Content>();

        public class Content
        {
            [JsonPropertyName("parts")]
            public List<Part> Parts { get; set; } = new List<Part>();
        }

        public class Part
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
        }

        public static AnalysisRequest Create(string prompt)
        {
            return new AnalysisRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Parts = new List<Part>
                        {
                            new Part { Text = prompt }
                        }
                    }
                }
            };
        }
    }
}