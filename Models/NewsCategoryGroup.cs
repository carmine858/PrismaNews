namespace Prisma.Models
{
    public class NewsCategoryGroup
    {
        public string CategoryName { get; set; }
        public string CategorySlug { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public string ThemeColor { get; set; }
        public string GradientClass { get; set; }
        public List<NewsArticle> Articles { get; set; } = new List<NewsArticle>();

        public NewsCategoryGroup(string name)
        {
            CategoryName = name;
            CategorySlug = name.ToLower().Replace(" ", "-");

            // Set default values based on category name
            switch (name.ToLower())
            {
                case "politics":
                    Description = "Analisi del discorso politico e delle sue implicazioni";
                    IconClass = "bi-award";
                    ThemeColor = "#dc3545";
                    GradientClass = "category-politics";
                    break;
                case "environment":
                    Description = "Impatto ambientale e sostenibilità in primo piano";
                    IconClass = "bi-tree";
                    ThemeColor = "#198754";
                    GradientClass = "category-environment";
                    break;
                case "world":
                    Description = "Eventi globali e analisi geopolitica";
                    IconClass = "bi-globe";
                    ThemeColor = "#0d6efd";
                    GradientClass = "category-world";
                    break;
                case "technology":
                    Description = "Innovazioni tecnologiche e i loro effetti sulla società";
                    IconClass = "bi-cpu";
                    ThemeColor = "#6610f2";
                    GradientClass = "category-technology";
                    break;
                case "business":
                case "money":
                    Description = "Dinamiche economiche e finanziarie";
                    IconClass = "bi-cash-coin";
                    ThemeColor = "#fd7e14";
                    GradientClass = "category-business";
                    break;
                case "science":
                    Description = "Scoperte scientifiche e ricerca";
                    IconClass = "bi-activity";
                    ThemeColor = "#6f42c1";
                    GradientClass = "category-science";
                    break;
                case "society":
                case "culture":
                    Description = "Tendenze sociali e culturali";
                    IconClass = "bi-people";
                    ThemeColor = "#20c997";
                    GradientClass = "category-society";
                    break;
                case "opinion":
                    Description = "Editoriali e analisi di opinione";
                    IconClass = "bi-chat-quote";
                    ThemeColor = "#ffc107";
                    GradientClass = "category-opinion";
                    break;
                default:
                    Description = "Notizie ed eventi recenti";
                    IconClass = "bi-newspaper";
                    ThemeColor = "#6c757d";
                    GradientClass = "category-default";
                    break;
            }
        }
    }
}