namespace Prisma.Models
{
    public class NewsArticle
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public string ApiUrl { get; set; } = string.Empty;
        public bool IsFeatured { get; set; } = false;
        public bool IsTrending { get; set; } = false;

        public string FormattedDate => PublicationDate.ToString("d MMMM yyyy");

        public string GetTimeAgo()
        {
            var span = DateTime.Now - PublicationDate;

            if (span.TotalDays > 365)
                return $"{(int)(span.TotalDays / 365)} anno/i fa";
            if (span.TotalDays > 30)
                return $"{(int)(span.TotalDays / 30)} mese/i fa";
            if (span.TotalDays > 1)
                return $"{(int)span.TotalDays} giorno/i fa";
            if (span.TotalHours > 1)
                return $"{(int)span.TotalHours} ora/e fa";
            if (span.TotalMinutes > 1)
                return $"{(int)span.TotalMinutes} minuto/i fa";

            return "Ora";
        }

        public string GetTruncatedSummary(int maxChars = 150)
        {
            if (string.IsNullOrEmpty(Summary) || Summary.Length <= maxChars)
                return Summary;

            return Summary.Substring(0, maxChars) + "...";
        }

        public string GetPlaceholderImage()
        {
            return "/img/placeholder-news.jpg";
        }

        public string GetSectionColor()
        {
            return Section.ToLower() switch
            {
                "world" => "bg-primary",
                "politics" => "bg-danger",
                "environment" => "bg-success",
                "science" => "bg-purple",
                "technology" => "bg-info",
                "business" => "bg-warning",
                "money" => "bg-warning",
                "opinion" => "bg-yellow",
                "society" => "bg-teal",
                "culture" => "bg-cyan",
                _ => "bg-secondary"
            };
        }

        public string GetTrendingScore()
        {
            var random = new Random();
            return random.Next(75, 98) + "%";
        }
    }
}