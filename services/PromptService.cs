namespace PrismaNews.services
{
    public class PromptService
    {

        private readonly Dictionary<string, string> _prompts = new()
    {
        {"Politica", "Analizza l'articolo politico individuando bias ideologici, framing e omissioni."},
        {"Salute", "Analizza l'articolo medico valutando verificabilità scientifica, omissioni e conflitti di interesse."},
        {"Tecnologia", "Analizza l'articolo tecnologico rilevando tono promozionale, interessi economici e omissioni."},
        {"Economia", "Analizza l'articolo economico individuando implicazioni sociali, framing di mercato e controprospettive."}
    };

        public string GetPrompt(string category, string content)
        {
            if (!_prompts.ContainsKey(category))
                category = "Politica"; // default fallback

            return $"{_prompts[category]}\n\nTesto:\n{content}";
        }
    }
}
