using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrismaNews.model;
using PrismaNews.services;

namespace PrismaNews.Services
{
    public class Gemini2FlashService : IArticleAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _apiKey;

        public Gemini2FlashService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            // Utilizza l'URL specifico fornito
            _apiUrl = _configuration["Gemini:AiApiUrl"] ??
                      "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

            _apiKey = _configuration["Gemini:ApiKey"];

            // Verifica che l'URL includa già il parametro di query
            if (!_apiUrl.Contains("?"))
            {
                _apiUrl += "?";
            }

            // Aggiungi la chiave API all'URL se non è già presente
            if (!_apiUrl.Contains("key="))
            {
                _apiUrl += $"key={_apiKey}";
            }
        }

        public async Task<ArticleAnalysis> AnalyzeArticleAsync(Article article, string category = null)
        {
            if (string.IsNullOrEmpty(category))
            {
                category = await DetectCategoryAsync(article);
            }

            var analysis = new ArticleAnalysis
            {
                ArticleId = article.Id,
                BiasDetection = await DetectBiasAsync(article, category),
                RhetoricalTechniques = await IdentifyRhetoricalTechniquesAsync(article, category),
                AlternativePerspectives = await GenerateAlternativePerspectivesAsync(article, category),
                RelevantOmissions = await IdentifyOmissionsAsync(article, category),
                CounterArguments = await GenerateCounterArgumentsAsync(article, category)
            };

            // Componenti di analisi specifici per categoria
            if (category.Equals("health", StringComparison.OrdinalIgnoreCase) ||
                category.Equals("science", StringComparison.OrdinalIgnoreCase) ||
                category.Equals("salute", StringComparison.OrdinalIgnoreCase) ||
                category.Equals("scienza", StringComparison.OrdinalIgnoreCase))
            {
                analysis.ScientificReferences = await AnalyzeScientificReferencesAsync(article);
            }

            if (category.Equals("technology", StringComparison.OrdinalIgnoreCase) ||
                category.Equals("economy", StringComparison.OrdinalIgnoreCase) ||
                category.Equals("tecnologia", StringComparison.OrdinalIgnoreCase) ||
                category.Equals("economia", StringComparison.OrdinalIgnoreCase))
            {
                analysis.CommercialInterests = await IdentifyCommercialInterestsAsync(article);
            }

            return analysis;
        }

        public async Task<string> DetectCategoryAsync(Article article)
        {
            var prompt = $"Classifica questo articolo in una di queste categorie: politica, salute, tecnologia, scienza, economia, altro.\n\nArticolo: {article.Title}\n{article.Content}\n\nCategoria:";
            return await CallGeminiAsync(prompt);
        }

        public async Task<string> DetectBiasAsync(Article article, string category)
        {
            string promptTemplate = GetCategorySpecificPrompt(category, "bias");
            var prompt = string.Format(promptTemplate, article.Title, article.Content);
            return await CallGeminiAsync(prompt);
        }

        public async Task<string> IdentifyRhetoricalTechniquesAsync(Article article, string category)
        {
            string promptTemplate = GetCategorySpecificPrompt(category, "rhetorical");
            var prompt = string.Format(promptTemplate, article.Title, article.Content);
            return await CallGeminiAsync(prompt);
        }

        public async Task<string> GenerateAlternativePerspectivesAsync(Article article, string category)
        {
            string promptTemplate = GetCategorySpecificPrompt(category, "perspectives");
            var prompt = string.Format(promptTemplate, article.Title, article.Content);
            return await CallGeminiAsync(prompt);
        }

        public async Task<string> IdentifyOmissionsAsync(Article article, string category)
        {
            string promptTemplate = GetCategorySpecificPrompt(category, "omissions");
            var prompt = string.Format(promptTemplate, article.Title, article.Content);
            return await CallGeminiAsync(prompt);
        }

        public async Task<string> AnalyzeScientificReferencesAsync(Article article)
        {
            var prompt = $"Analizza i riferimenti scientifici in questo articolo. Identifica se sono peer-reviewed, se le fonti sono credibili, e se mancano riferimenti scientifici importanti:\n\nArticolo: {article.Title}\n{article.Content}";
            return await CallGeminiAsync(prompt);
        }

        public async Task<string> IdentifyCommercialInterestsAsync(Article article)
        {
            var prompt = $"Identifica potenziali interessi commerciali o bias finanziari in questo articolo. Cerca sponsorizzazioni non dichiarate, conflitti di interesse o contenuti promozionali:\n\nArticolo: {article.Title}\n{article.Content}";
            return await CallGeminiAsync(prompt);
        }

        public async Task<string> GenerateCounterArgumentsAsync(Article article, string category)
        {
            string promptTemplate = GetCategorySpecificPrompt(category, "counterarguments");
            var prompt = string.Format(promptTemplate, article.Title, article.Content);
            return await CallGeminiAsync(prompt);
        }

        private string GetCategorySpecificPrompt(string category, string analysisType)
        {
            // Restituisci template di prompt diversi basati sulla categoria e sul tipo di analisi
            // Gestisce sia nomi di categorie in italiano che in inglese
            string categoryLower = category.ToLower();

            if (categoryLower == "politica" || categoryLower == "politics")
            {
                switch (analysisType)
                {
                    case "bias":
                        return "Identifica bias politici, framing di parte e tendenze ideologiche in questo articolo:\n\nArticolo: {0}\n{1}";
                    case "perspectives":
                        return "Genera prospettive politiche alternative da diverse posizioni ideologiche (progressista, conservatore, libertario, ecc.) sui temi discussi in questo articolo:\n\nArticolo: {0}\n{1}";
                    default:
                        return "Analizza il seguente articolo politico per {2}:\n\nArticolo: {0}\n{1}";
                }
            }

            if (categoryLower == "salute" || categoryLower == "health" ||
                categoryLower == "scienza" || categoryLower == "science")
            {
                switch (analysisType)
                {
                    case "bias":
                        return "Identifica eventuali bias scientifici, rappresentazioni errate della ricerca o eccessive semplificazioni in questo articolo su salute/scienza:\n\nArticolo: {0}\n{1}";
                    case "omissions":
                        return "Identifica importanti contesti scientifici, avvertenze, limitazioni o prove contrarie che mancano in questo articolo su salute/scienza:\n\nArticolo: {0}\n{1}";
                    default:
                        return "Analizza il seguente articolo su salute/scienza per {2}:\n\nArticolo: {0}\n{1}";
                }
            }

            if (categoryLower == "tecnologia" || categoryLower == "technology")
            {
                switch (analysisType)
                {
                    case "bias":
                        return "Identifica determinismo tecnologico, hype o affermazioni non supportate in questo articolo sulla tecnologia:\n\nArticolo: {0}\n{1}";
                    case "rhetorical":
                        return "Identifica linguaggio di marketing, tecno-ottimismo/pessimismo o altri dispositivi retorici utilizzati in questo articolo sulla tecnologia:\n\nArticolo: {0}\n{1}";
                    default:
                        return "Analizza il seguente articolo sulla tecnologia per {2}:\n\nArticolo: {0}\n{1}";
                }
            }

            if (categoryLower == "economia" || categoryLower == "economy")
            {
                switch (analysisType)
                {
                    case "bias":
                        return "Identifica ideologia economica, interessi finanziari o bias di mercato in questo articolo economico:\n\nArticolo: {0}\n{1}";
                    case "omissions":
                        return "Identifica importanti contesti economici, teorie economiche alternative o punti dati rilevanti mancanti da questo articolo economico:\n\nArticolo: {0}\n{1}";
                    default:
                        return "Analizza il seguente articolo economico per {2}:\n\nArticolo: {0}\n{1}";
                }
            }

            // Prompt generici per categorie non specifiche
            switch (analysisType)
            {
                case "bias":
                    return "Identifica eventuali bias, tecniche di framing o presentazioni soggettive in questo articolo:\n\nArticolo: {0}\n{1}";
                case "rhetorical":
                    return "Identifica tecniche retoriche, linguaggio persuasivo o appelli emotivi utilizzati in questo articolo:\n\nArticolo: {0}\n{1}";
                case "perspectives":
                    return "Genera prospettive e punti di vista alternativi sui temi discussi in questo articolo:\n\nArticolo: {0}\n{1}";
                case "omissions":
                    return "Identifica contesti importanti, fatti o prospettive che sono mancanti o sottorappresentati in questo articolo:\n\nArticolo: {0}\n{1}";
                case "counterarguments":
                    return "Genera controargomentazioni ben ragionate alle principali affermazioni fatte in questo articolo:\n\nArticolo: {0}\n{1}";
                default:
                    return "Analizza il seguente articolo per {2}:\n\nArticolo: {0}\n{1}";
            }
        }

        private async Task<string> CallGeminiAsync(string prompt)
        {
            try
            {
                // Formatta la richiesta secondo la struttura richiesta da Gemini 2.0 Flash
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 1000,
                        topP = 0.8,
                        topK = 40
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Invia la richiesta all'API
                var response = await _httpClient.PostAsync(_apiUrl, content);

                // Assicurati che la richiesta sia andata a buon fine
                response.EnsureSuccessStatusCode();

                // Elabora la risposta
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseContent);

                // Estrai il testo della risposta
                var candidatesElement = responseJson.RootElement.GetProperty("candidates");

                if (candidatesElement.GetArrayLength() > 0)
                {
                    var contentElement = candidatesElement[0].GetProperty("content");
                    var partsArray = contentElement.GetProperty("parts");

                    if (partsArray.GetArrayLength() > 0 && partsArray[0].TryGetProperty("text", out var textElement))
                    {
                        return textElement.GetString() ?? "Analisi non disponibile";
                    }
                }

                return "Analisi non disponibile";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la chiamata a Gemini API: {ex.Message}");
                return $"Si è verificato un errore durante l'analisi: {ex.Message}";
            }
        }
    }
}