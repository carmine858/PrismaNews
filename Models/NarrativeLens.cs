namespace Prisma.Models
{
    public enum PerspectiveType
    {
        Activist,
        Politician,
        Expert,
        Victim
    }

    public class NarrativeLens
    {
        public string ArticleId { get; set; }
        public string OriginalTitle { get; set; }
        public string OriginalContent { get; set; }
        public List<Perspective> Perspectives { get; set; } = new List<Perspective>();
    }

    public class Perspective
    {
        public PerspectiveType Type { get; set; }
        public string Name { get; set; }
        public string Emoji { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }

    public class PerspectiveResult
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }
}