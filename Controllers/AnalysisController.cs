using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PrismaNews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AnalysisController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeArticle([FromBody] string articleContent)
        {
            var apiKey = _configuration["AIzaSyByiEpp97Z3PZFUl8pkqQ9FaLkWlKsCGt0"];
            var apiUrl = _configuration["https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?"];  // es: endpoint Gemini proxy se hai API tua

            // Prompt di esempio — puoi parametrizzarlo
            var prompt = $@"
Agisci come un analista critico di notizie esperto in bias, retorica e confronto ideologico. 
Hai davanti un articolo giornalistico: il tuo compito è decostruirlo a fondo, come farebbe una redazione investigativa multidisciplinare.

Esegui le seguenti operazioni passo dopo passo:

1. **Riassunto Oggettivo**:
   - Riassumi in 5-7 righe il contenuto dell’articolo in modo neutrale e fattuale.

2. **Analisi del Bias**:
   - Identifica eventuali inclinazioni ideologiche, politiche o culturali presenti nel testo.
   - Specifica se l’autore o la fonte mostrano segni di parzialità.

3. **Tecniche Retoriche**:
   - Elenca le principali tecniche retoriche usate: linguaggio emotivo, metafore, generalizzazioni, appelli all’autorità, ecc.

4. **Framing**:
   - Descrivi come il problema viene inquadrato. Qual è la narrativa dominante? Chi viene presentato come vittima, eroe, colpevole?

5. **Controargomentazioni**:
   - Per ogni affermazione chiave dell’articolo (massimo 3-5), fornisci una controargomentazione logica e documentata.
   - Cita dati, fonti alternative o prospettive contrastanti.

6. **Prospettive Alternative**:
   - Presenta almeno 3 punti di vista alternativi: politico (es. destra, sinistra, centro), culturale (occidentale vs non occidentale), filosofico (es. utilitarismo, libertarismo).
   - Per ciascuno, spiega come vedrebbe il tema trattato e perché.

7. **Cosa manca**: 
   - Indica quali aspetti rilevanti o dati importanti non sono stati trattati nell’articolo.
   - Suggerisci fonti affidabili che potrebbero completare l’informazione.

8. **Output Strutturato in JSON** (opzionale per uso programmatico):
   - Se richiesto, restituisci l'output come oggetto JSON con campi: `summary`, `bias`, `rhetoric`, `framing`, `counterarguments`, `alternative_views`, `missing_information`.

ARTICOLO:
{{articleContent}}
";
            var requestBody = new
            {
                prompt = prompt,
                temperature = 0.7
            };

            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(apiUrl, jsonContent);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Errore nell'analisi con AI API");

            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }


    }
}
