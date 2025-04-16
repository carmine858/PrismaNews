using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrismaNews.services
{
    public class NewsService
    {
    


        private readonly HttpClient _httpClient;

        public NewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ExtractTextFromUrl(string url)
        {
            try
            {
                // Scarica il contenuto HTML dalla URL
                string html = await _httpClient.GetStringAsync(url);

                // Carica l'HTML nel documento
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Rimuovi script, stili e altri elementi non testuali
                var nodesToRemove = htmlDoc.DocumentNode.SelectNodes("//script|//style|//comment()");
                if (nodesToRemove != null)
                {
                    foreach (var node in nodesToRemove)
                    {
                        node.Remove();
                    }
                }

                // Estrai il testo dal contenuto principale
                // Qui puoi personalizzare il selettore in base alla struttura del sito
                var mainContent = htmlDoc.DocumentNode.SelectSingleNode("//article") ??
                                  htmlDoc.DocumentNode.SelectSingleNode("//main") ??
                                  htmlDoc.DocumentNode.SelectSingleNode("//div[@class='content']") ??
                                  htmlDoc.DocumentNode;

                return mainContent.InnerText.Trim();
            }
            catch (Exception ex)
            {
                // Gestisci gli errori appropriatamente
                return $"Errore nell'estrazione del testo: {ex.Message}";
            }
        }
    }
}

