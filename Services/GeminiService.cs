using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prisma.Models;

namespace Prisma.Services
{

    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private readonly string _apiKey;
        private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public GeminiService(
            HttpClient httpClient,
            ILogger<GeminiService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GeminiApiKey"] ?? "AIzaSyByiEpp97Z3PZFUl8pkqQ9FaLkWlKsCGt0";
        }

        public async Task<AnalysisResult> AnalyzeArticleAsync(NewsArticle article, string articleContent)
        {
            try
            {
                _logger.LogInformation($"Starting analysis for article: {article.Id} - {article.Title}");

                // Determina la categoria per il prompt
                string category = MapCategory(article.Section);

                // Genera il prompt specifico per categoria
                string prompt = GenerateCategoryPrompt(category, article.Title, articleContent);

                // Chiama l'API Gemini
                var analysisText = await CallGeminiApiAsync(prompt);

                // Estrai il JSON dalla risposta
                var jsonResult = ExtractJsonFromResponse(analysisText);

                // Deserializza la risposta
                var analysis = DeserializeAnalysis(jsonResult);

                // Completa con metadati dell'articolo
                analysis.ArticleId = article.Id;
                analysis.Title = article.Title;
                analysis.Category = article.Section;
                analysis.Url = article.Url;
                analysis.Source = ExtractSourceFromUrl(article.Url);
                analysis.RawAnalysisJson = jsonResult;
                analysis.IsComplete = true;

                _logger.LogInformation($"Analysis completed for article: {article.Id}");

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during article analysis: {article?.Id ?? "unknown"}");
                throw;
            }
        }

        private string MapCategory(string section)
        {
            return section?.ToLower() switch
            {
                "politics" => "politics",
                "environment" => "environment",
                "technology" => "technology",
                "business" => "economy",
                "money" => "economy",
                "economy" => "economy",
                "society" => "society",
                "culture" => "society",
                "world" => "world",
                "science" => "science",
                _ => "general"
            };
        }

        private string GenerateCategoryPrompt(string category, string title, string content)
        {
            // Base prompt con struttura JSON richiesta
            var basePrompt = @"
Analizza il seguente articolo giornalistico traducendolo in italiano se necessario e genera una risposta JSON strutturata che decostruisca in profondità le tecniche retoriche, i bias, il framing e altre strategie persuasive utilizzate.

REQUISITI IMPORTANTI:
1. La risposta DEVE essere in formato JSON valido.
2. La risposta deve seguire ESATTAMENTE questa struttura:

{
  ""summary"": ""Una sintesi approfondita delle principali tematiche e tesi dell'articolo in max 5 frasi"",
  ""primaryFraming"": ""La cornice narrativa principale utilizzata nell'articolo, identificando anche l'approccio ideologico sottostante"",
  ""framing"": [""Elenco dettagliato"", ""delle tecniche di framing"", ""con specificazione di metafore strutturanti"", ""dicotomie presentate"", ""e presupposti impliciti""],
  ""bias"": [
    {
      ""type"": ""Nome specifico del bias cognitivo"",
      ""description"": ""Descrizione dettagliata di come il bias si manifesta nell'articolo, con riferimento al meccanismo psicologico attivato"",
      ""severity"": 75,
      ""examples"": [""Citazione specifica 1 dall'articolo"", ""Citazione specifica 2 dall'articolo""]
    }
  ],
  ""rhetoric"": [
    {
      ""name"": ""Nome specifico della tecnica retorica"",
      ""description"": ""Descrizione approfondita di come viene utilizzata strategicamente nell'articolo"",
      ""examples"": [""Citazione specifica dall'articolo""],
      ""effect"": ""Analisi dettagliata dell'effetto potenziale sul lettore e valutazione dell'efficacia (1-100)""
    }
  ],
  ""counterArguments"": [
    ""Controargomentazione principale: versione forte dell'argomento opposto"", 
    ""Punti ciechi: prospettive rilevanti completamente assenti"",
    ""Fatti mancanti: informazioni fattuali essenziali omesse"", 
    ""Narrazione alternativa completa da prospettiva opposta""
  ],
  ""alternativePerspective"": ""Analisi dettagliata di come la stessa notizia potrebbe essere raccontata da una prospettiva completamente diversa, con proposta di sintesi equilibrata"",
  ""keyTerms"": [
    {
      ""term"": ""termine1"",
      ""analysis"": ""Analisi del significato denotativo vs connotativo e funzione persuasiva""
    },
    {
      ""term"": ""termine2"",
      ""analysis"": ""Analisi del significato denotativo vs connotativo e funzione persuasiva""
    }
  ],
  ""scores"": {
    ""bias"": 65,
    ""persuasion"": 70,
    ""complexity"": 45,
    ""emotionality"": 60,
    ""factualAccuracy"": 80,
    ""sourceTransparency"": 55,
    ""perspectiveBalance"": 40
  },
  ""sourceAnalysis"": {
    ""quality"": ""Valutazione della qualità delle fonti citate"",
    ""variety"": ""Analisi della varietà e rappresentatività delle fonti"",
    ""missingPerspectives"": [""Fonte importante 1 non consultata"", ""Fonte importante 2 non consultata""]
  },
  ""overallAssessment"": ""Valutazione critica approfondita dell'articolo, riflettendo sulla sua posizione nel panorama mediatico e sugli effetti potenziali sul dibattito pubblico""
}

3. Includi sempre valori numerici (punteggi) per tutte le metriche su scala 1-100.
4. Identifica almeno 3-5 bias cognitivi e tecniche retoriche.
5. Fornisci sempre esempi concreti e citazioni specifiche dall'articolo.
6. Sviluppa controargomentazioni strutturate e approfondite.

ARTICOLO DA ANALIZZARE:
Titolo: """ + title + @"""

Contenuto:
" + content;

            // Aggiungi istruzioni specifiche per categoria
            string categorySpecificInstructions = category.ToLower() switch
            {
                "politics" => @"
ISTRUZIONI SPECIFICHE PER ANALISI POLITICA:
1. ANALISI DELL'ORIENTAMENTO IDEOLOGICO
   - Posiziona l'articolo su molteplici spettri ideologici (non solo destra-sinistra, ma anche autoritarismo-libertarismo, progressismo-conservatorismo)
   - Identifica i marcatori linguistici che segnalano l'orientamento ideologico
   - Analizza come vengono rappresentati i diversi schieramenti politici (linguaggio, attributi, azioni)

2. ANALISI DELLA POLARIZZAZIONE
   - Identifica elementi di divisione e polarizzazione retorica
   - Analizza l'uso di etichette politiche e la loro funzione stigmatizzante/valorizzante
   - Rileva meccanismi di demonizzazione dell'avversario o sacralizzazione della propria parte

3. CONTROARGOMENTAZIONE POLITICA
   - Sviluppa controargomentazioni da almeno tre prospettive politiche diverse
   - Identifica quali elettorati verrebbero maggiormente persuasi dalla narrazione dell'articolo
   - Evidenzia le conseguenze politiche implicite dell'accettazione del frame proposto

4. ANALISI DEGLI INTERESSI RAPPRESENTATI
   - Identifica quali gruppi sociali, economici e politici beneficerebbero della narrazione proposta
   - Analizza quali interessi strutturali sono implicitamente protetti o promossi
   - Esamina quali voci o prospettive sono sistematicamente marginalizzate
",
                "environment" => @"
ISTRUZIONI SPECIFICHE PER ANALISI AMBIENTALE:
1. ANALISI DEL PARADIGMA ECOLOGICO
   - Identifica il paradigma ambientale sottostante (antropocentrico, biocentrico, ecocentrico)
   - Analizza come viene presentato il rapporto uomo-natura
   - Esamina la temporalità del discorso (urgenza immediata vs prospettiva di lungo termine)

2. ANALISI DEL LINGUAGGIO SCIENTIFICO
   - Identifica l'accuratezza della terminologia scientifica utilizzata
   - Analizza il grado di incertezza espresso vs la certezza scientifica attuale
   - Esamina come vengono presentati i dati (contestualizzazione, scale temporali, paragoni)

3. ANALISI DELLA RAPPRESENTAZIONE DEL RISCHIO
   - Identifica il frame di rischio utilizzato (catastrofismo vs minimizzazione)
   - Analizza il bilanciamento tra rischi a breve e lungo termine
   - Esamina come vengono presentate le probabilità vs gli impatti

4. ANALISI DEGLI INTERESSI ECONOMICI
   - Identifica quali settori economici beneficerebbero delle politiche ambientali suggerite
   - Analizza la presenza/assenza di considerazioni di giustizia ambientale
   - Esamina come vengono rappresentati i costi economici vs i benefici ambientali
",
                "technology" => @"
ISTRUZIONI SPECIFICHE PER ANALISI TECNOLOGICA:
1. ANALISI DEL DETERMINISMO TECNOLOGICO
   - Identifica elementi di determinismo tecnologico vs agency umana nella narrazione
   - Analizza come viene presentato il rapporto tra società e tecnologia
   - Esamina la rappresentazione della neutralità tecnologica vs valori incorporati

2. ANALISI DELLA POLARIZZAZIONE TECNO-VALORIALE
   - Identifica elementi di tecnottimismo vs tecnoallarmismo
   - Analizza la rappresentazione dei rischi vs opportunità della tecnologia
   - Esamina il bilanciamento tra entusiasmo per l'innovazione e principio di precauzione

3. ANALISI DEGLI ATTORI TECNOLOGICI
   - Identifica come vengono rappresentati i diversi attori dell'ecosistema tecnologico
   - Analizza quali voci sono amplificate vs minimizzate nel dibattito tecnologico
   - Esamina come vengono rappresentate le aziende tech e i loro leader

4. ANALISI DELLE COMPETENZE TECNICHE
   - Valuta l'accuratezza tecnica delle descrizioni tecnologiche
   - Identifica semplificazioni eccessive o distorsioni tecniche
   - Esamina l'uso di gergo tecnico e la sua funzione retorica
",
                "economy" => @"
ISTRUZIONI SPECIFICHE PER ANALISI ECONOMICA:
1. ANALISI DEL PARADIGMA ECONOMICO
   - Identifica il paradigma economico sottostante (liberista, keynesiano, socialista, ecc.)
   - Analizza i presupposti impliciti sull'efficienza dei mercati, ruolo dello stato, ecc.
   - Esamina come vengono presentati i trade-off economici

2. ANALISI DEGLI INDICATORI ECONOMICI
   - Identifica quali indicatori economici vengono privilegiati e quali omessi
   - Analizza la contestualizzazione storica e geografica dei dati economici
   - Esamina la rappresentazione della causalità nei fenomeni economici

3. ANALISI DISTRIBUTIVA
   - Identifica come vengono rappresentati gli impatti distributivi delle politiche/fenomeni
   - Analizza quali gruppi socioeconomici sono visibilizzati vs invisibilizzati
   - Esamina come viene trattato il tema delle disuguaglianze

4. ANALISI DEGLI INTERESSI ECONOMICI
   - Identifica quali settori economici o classi sociali beneficerebbero delle politiche suggerite
   - Analizza la trasparenza su potenziali conflitti di interesse nelle fonti
   - Esamina la rappresentazione del rapporto tra potere economico e politico
",
                "society" => @"
ISTRUZIONI SPECIFICHE PER ANALISI SOCIALE:
1. ANALISI DELLE RAPPRESENTAZIONI SOCIALI
   - Identifica come vengono rappresentati diversi gruppi sociali
   - Analizza la distribuzione di caratteristiche positive/negative tra gruppi sociali
   - Esamina l'uso di pronomi inclusivi/esclusivi (noi/loro)

2. ANALISI DEI PROBLEMI SOCIALI
   - Identifica il framing dei problemi sociali (individuali vs strutturali)
   - Analizza l'attribuzione di responsabilità causale e risolutiva
   - Esamina come vengono presentate le temporalità dei problemi

3. ANALISI DELLA NORMALITÀ/DEVIANZA
   - Identifica quali comportamenti/gruppi sono presentati come normativi vs devianti
   - Analizza i processi impliciti di patologizzazione o normalizzazione
   - Esamina l'uso di linguaggio medico/statistico per rinforzare norme sociali

4. ANALISI DEL POTERE E DELL'AGENCY
   - Identifica come viene rappresentata la distribuzione del potere tra attori sociali
   - Analizza l'agency attribuita a diversi gruppi (attivi vs passivi)
   - Esamina la rappresentazione di strutture vs azioni individuali
",
                "world" => @"
ISTRUZIONI SPECIFICHE PER ANALISI RELAZIONI INTERNAZIONALI:
1. ANALISI DEL PARADIGMA GEOPOLITICO
   - Identifica il paradigma delle relazioni internazionali sottostante
   - Analizza la rappresentazione della sovranità nazionale vs governance globale
   - Esamina come vengono presentate le relazioni di potere tra stati/regioni

2. ANALISI DELLE RAPPRESENTAZIONI CULTURALI
   - Identifica dinamiche di orientalismo o occidentalismo nella rappresentazione di culture/paesi
   - Analizza l'uso di stereotipi nazionali/culturali
   - Esamina il posizionamento implicito rispetto a gerarchie globali

3. ANALISI DELL'INQUADRAMENTO STORICO
   - Identifica quali precedenti storici sono inclusi vs omessi
   - Analizza la rappresentazione della continuità/discontinuità storica
   - Esamina come vengono presentate le responsabilità storiche

4. ANALISI DEGLI INTERESSI NAZIONALI
   - Identifica quali interessi nazionali/regionali sono implicitamente promossi
   - Analizza la trasparenza sulle alleanze geopolitiche rilevanti
   - Esamina come vengono rappresentati i conflitti di interesse tra potenze
",
                "science" => @"
ISTRUZIONI SPECIFICHE PER ANALISI SCIENTIFICA:
1. ANALISI DELLA RAPPRESENTAZIONE SCIENTIFICA
   - Identifica accuratezze e inaccuratezze nella presentazione di metodi e risultati
   - Analizza come viene presentata l'incertezza e il concetto di prova scientifica
   - Esamina la rappresentazione della comunità scientifica e del processo di revisione

2. ANALISI EPISTEMOLOGICA
   - Identifica quali tipologie di conoscenza sono privilegiate vs marginalizzate
   - Analizza la rappresentazione della relazione tra scienza e altri sistemi di conoscenza
   - Esamina come vengono presentati i limiti del metodo scientifico

3. ANALISI DEL LINGUAGGIO SCIENTIFICO
   - Identifica l'uso appropriato vs inappropriato di terminologia scientifica
   - Analizza il livello di semplificazione di concetti complessi
   - Esamina l'uso di metafore esplicative e il loro impatto sulla comprensione

4. ANALISI DEL CONTESTO SCIENTIFICO
   - Identifica come l'articolo si posiziona rispetto al consenso scientifico corrente
   - Analizza l'inserimento dei risultati nel corpo di conoscenze esistenti
   - Valuta la trasparenza su finanziamenti e potenziali conflitti d'interesse
",
                _ => @"
ISTRUZIONI GENERICHE PER ANALISI APPROFONDITA:
1. ANALISI DEGLI ASSIOMI IMPLICITI
   - Identifica i presupposti non dichiarati che l'articolo dà per scontati
   - Analizza i valori sottesi all'argomentazione
   - Esamina le premesse implicite che guidano le conclusioni

2. ANALISI RETORICA AVANZATA
   - Identifica l'uso di tecniche persuasive e la loro funzione strategica
   - Analizza la costruzione dell'ethos, pathos e logos nell'articolo
   - Esamina le strategie di legittimazione e delegittimazione

3. ANALISI DELLA COMPLESSITÀ
   - Valuta il grado di complessità presentato vs la complessità reale del tema
   - Analizza le semplificazioni e le omissioni strategiche
   - Esamina il bilanciamento tra diverse prospettive

4. ANALISI META-DISCORSIVA
   - Identifica come l'articolo si posiziona nel più ampio dibattito mediatico
   - Analizza il tipo di audience implicitamente costruita dal testo
   - Esamina quali competenze di literacy mediatica sono necessarie per una lettura critica
"
            };

            return basePrompt + categorySpecificInstructions;
        }

        /// <summary>
        /// Esegue un'analisi comparativa di più articoli sullo stesso tema
        /// </summary>
        public async Task<AnalysisResult> AnalyzeComparativeAsync(List<NewsArticle> articles, List<string> articleContents)
        {
            if (articles.Count < 2 || articleContents.Count < 2)
                throw new ArgumentException("L'analisi comparativa richiede almeno due articoli");

            try
            {
                _logger.LogInformation($"Starting comparative analysis for {articles.Count} articles");

                // Prepara i dati per il prompt comparativo
                string[] articleTexts = articleContents.ToArray();
                string[] sources = articles.Select(a => ExtractSourceFromUrl(a.Url)).ToArray();

                // Determina la categoria comune (usa la più frequente)
                string commonCategory = articles
                    .GroupBy(a => MapCategory(a.Section))
                    .OrderByDescending(g => g.Count())
                    .First().Key;

                // Genera il prompt comparativo
                string prompt = GenerateComparativePrompt(articleTexts, sources, commonCategory);

                // Chiama l'API Gemini
                var analysisText = await CallGeminiApiAsync(prompt);

                // Estrai il JSON dalla risposta
                var jsonResult = ExtractJsonFromResponse(analysisText);

                // Deserializza la risposta
                var analysis = DeserializeAnalysis(jsonResult);

                // Completa con metadati
                analysis.ArticleId = string.Join(",", articles.Select(a => a.Id));
                analysis.Title = "Analisi Comparativa: " + articles[0].Title;
                analysis.Category = commonCategory;
                analysis.Url = articles[0].Url;
                analysis.Source = "Multiple Sources";
                analysis.RawAnalysisJson = jsonResult;
                analysis.IsComplete = true;
                analysis.IsComparative = true;

                _logger.LogInformation($"Comparative analysis completed");

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during comparative article analysis");
                throw;
            }
        }

        /// <summary>
        /// Esegue un'analisi specifica sulla qualità delle fonti di un articolo
        /// </summary>
        public async Task<AnalysisResult> AnalyzeSourceQualityAsync(NewsArticle article, string articleContent)
        {
            try
            {
                _logger.LogInformation($"Starting source quality analysis for article: {article.Id}");

                string source = ExtractSourceFromUrl(article.Url);
                string prompt = GenerateSourceQualityPrompt(articleContent, source);

                // Chiama l'API Gemini
                var analysisText = await CallGeminiApiAsync(prompt);

                // Estrai il JSON dalla risposta
                var jsonResult = ExtractJsonFromResponse(analysisText);

                // Deserializza la risposta
                var analysis = DeserializeAnalysis(jsonResult);

                // Completa con metadati dell'articolo
                analysis.ArticleId = article.Id;
                analysis.Title = article.Title;
                analysis.Category = article.Section;
                analysis.Url = article.Url;
                analysis.Source = source;
                analysis.RawAnalysisJson = jsonResult;
                analysis.IsComplete = true;
                analysis.IsSourceAnalysis = true;

                _logger.LogInformation($"Source quality analysis completed for article: {article.Id}");

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during source quality analysis: {article?.Id ?? "unknown"}");
                throw;
            }
        }

        /// <summary>
        /// Esegue un'analisi del discorso su un articolo
        /// </summary>
        public async Task<AnalysisResult> AnalyzeDiscourseAsync(NewsArticle article, string articleContent)
        {
            try
            {
                _logger.LogInformation($"Starting discourse analysis for article: {article.Id}");

                // Determina la categoria per il prompt
                string category = MapCategory(article.Section);

                // Genera il prompt per l'analisi del discorso
                string prompt = GenerateDiscourseAnalysisPrompt(articleContent, category);

                // Chiama l'API Gemini
                var analysisText = await CallGeminiApiAsync(prompt);

                // Estrai il JSON dalla risposta
                var jsonResult = ExtractJsonFromResponse(analysisText);

                // Deserializza la risposta
                var analysis = DeserializeAnalysis(jsonResult);

                // Completa con metadati dell'articolo
                analysis.ArticleId = article.Id;
                analysis.Title = article.Title;
                analysis.Category = article.Section;
                analysis.Url = article.Url;
                analysis.Source = ExtractSourceFromUrl(article.Url);
                analysis.RawAnalysisJson = jsonResult;
                analysis.IsComplete = true;
                analysis.IsDiscourseAnalysis = true;

                _logger.LogInformation($"Discourse analysis completed for article: {article.Id}");

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during discourse analysis: {article?.Id ?? "unknown"}");
                throw;
            }
        }

        /// <summary>
        /// Genera un prompt per l'analisi comparativa di più articoli
        /// </summary>
        private string GenerateComparativePrompt(string[] articleTexts, string[] sources, string category)
        {
            // Formatta gli articoli con le loro fonti
            string formattedArticles = "";
            for (int i = 0; i < articleTexts.Length; i++)
            {
                formattedArticles += $"\n\nARTICOLO {i + 1} (Fonte: {sources[i]}):\n{articleTexts[i]}";
            }

            string basePrompt = @"
Conduci un'analisi critica comparativa dei seguenti articoli giornalistici che trattano lo stesso tema/evento. Genera una risposta JSON strutturata secondo lo schema specificato.

REQUISITI IMPORTANTI:
1. La risposta DEVE essere in formato JSON valido.
2. La risposta deve seguire ESATTAMENTE questa struttura:

{
  ""summary"": ""Sintesi integrata del tema centrale trattato da tutti gli articoli"",
  ""framingComparison"": {
    ""convergencePoints"": [""Punti di convergenza informativa"", ""tra le fonti""],
    ""divergencePoints"": [""Principali divergenze"", ""di contenuto e interpretazione""],
    ""frameAnalysis"": [
      {
        ""source"": ""Fonte 1"",
        ""dominantFrame"": ""Frame dominante in questo articolo"",
        ""actorPortrayal"": ""Come vengono rappresentati gli attori principali"",
        ""causality"": ""Come vengono presentate le cause dell'evento"",
        ""implications"": ""Implicazioni e conseguenze evidenziate"",
        ""solutions"": ""Soluzioni proposte o implicite""
      }
    ]
  },
  ""omissionsMap"": {
    ""source1OnlyInfo"": [""Informazioni presenti solo nella fonte 1""],
    ""source2OnlyInfo"": [""Informazioni presenti solo nella fonte 2""],
    ""emphasisDifferences"": ""Analisi di quali elementi vengono enfatizzati o minimizzati nelle diverse narrazioni""
  },
  ""rhetoricComparison"": {
    ""languageDifferences"": ""Confronto del linguaggio, metafore e tono utilizzati"",
    ""persuasionTechniques"": {
      ""source1"": [""Tecniche persuasive fonte 1""],
      ""source2"": [""Tecniche persuasive fonte 2""]
    },
    ""presentationDifferences"": ""Analisi di come lo stesso fatto viene presentato con sfumature linguistiche diverse""
  },
  ""biasAssessment"": [
    {
      ""source"": ""Fonte 1"",
      ""biases"": [""Principali bias identificati""],
      ""editorialPerspective"": ""Come la prospettiva editoriale influenza la presentazione"",
      ""balanceScore"": 65
    }
  ],
  ""integratedNarrative"": ""Narrazione integrata che incorpora informazioni verificabili da tutte le fonti"",
  ""legitimateDisagreements"": [""Questioni su cui permane genuina incertezza"", ""o legittimo disaccordo""],
  ""readerRecommendations"": {
    ""criticalApproach"": ""Approccio critico suggerito per navigare le diverse narrazioni"",
    ""keyQuestions"": [""Domande chiave"", ""che il lettore dovrebbe porsi""],
    ""additionalSources"": [""Fonti aggiuntive suggerite"", ""per una comprensione più completa""]
  }
}

3. Includi sempre valori numerici per i punteggi di equilibrio (balanceScore) su scala 1-100.
4. Fornisci un'analisi dettagliata e concreta delle differenze tra le fonti.
5. Sviluppa raccomandazioni pratiche per il lettore.

ARTICOLI DA ANALIZZARE:" + formattedArticles;

            // Aggiungi istruzioni specifiche per categoria se disponibili
            string categoryInstructions = category.ToLower() switch
            {
                "politics" => @"
ISTRUZIONI SPECIFICHE PER ANALISI COMPARATIVA POLITICA:
- Analizza le differenze nella rappresentazione degli attori politici tra le fonti
- Confronta l'uso di termini politicamente connotati nei diversi articoli
- Identifica le diverse narrative ideologiche sottostanti
- Analizza come ogni fonte attribuisce responsabilità politiche
",
                "environment" => @"
ISTRUZIONI SPECIFICHE PER ANALISI COMPARATIVA AMBIENTALE:
- Confronta il grado di urgenza attribuito alle questioni ambientali nei diversi articoli
- Analizza le differenze nella presentazione dei dati scientifici
- Identifica i diversi frame di rischio (catastrofismo vs ottimismo)
- Confronta la rappresentazione degli interessi economici vs ambientali
",
                "technology" => @"
ISTRUZIONI SPECIFICHE PER ANALISI COMPARATIVA TECNOLOGICA:
- Confronta gli atteggiamenti verso la tecnologia nelle diverse fonti
- Analizza le differenze nella rappresentazione dei rischi vs opportunità
- Identifica i diversi livelli di determinismo tecnologico
- Confronta la rappresentazione degli attori dell'ecosistema tech
",
                "economy" => @"
ISTRUZIONI SPECIFICHE PER ANALISI COMPARATIVA ECONOMICA:
- Confronta i diversi paradigmi economici sottostanti
- Analizza le differenze nella selezione e presentazione di indicatori economici
- Identifica i diversi gruppi socioeconomici privilegiati nelle narrazioni
- Confronta l'attribuzione di responsabilità per fenomeni economici
",
                _ => @"
ISTRUZIONI GENERICHE PER ANALISI COMPARATIVA:
- Confronta accuratamente le diverse prospettive presentate
- Analizza le differenze nel tono, linguaggio e framing
- Identifica informazioni presenti in una fonte ma assenti nelle altre
- Valuta l'equilibrio complessivo di ciascuna fonte
"
            };

            return basePrompt + categoryInstructions;
        }

        /// <summary>
        /// Genera un prompt per l'analisi della qualità delle fonti di un articolo
        /// </summary>
        private string GenerateSourceQualityPrompt(string articleContent, string source)
        {
            string prompt = @"
Conduci un'analisi approfondita della qualità delle fonti e dell'affidabilità dell'informazione nel seguente articolo giornalistico. Genera una risposta JSON strutturata secondo lo schema specificato.

REQUISITI IMPORTANTI:
1. La risposta DEVE essere in formato JSON valido.
2. La risposta deve seguire ESATTAMENTE questa struttura:

{
  ""summary"": ""Sintesi dell'articolo e delle principali conclusioni sull'affidabilità delle fonti"",
  ""sourceMapping"": {
    ""primarySources"": [""Testimoni diretti"", ""documenti originali""],
    ""secondarySources"": [""Report"", ""analisi""],
    ""experts"": [
      {
        ""name"": ""Nome esperto"",
        ""credentials"": ""Credenziali dell'esperto"",
        ""relevance"": ""Pertinenza rispetto al tema"",
        ""potentialBias"": ""Possibili conflitti di interesse""
      }
    ],
    ""institutionalSources"": [""Fonti istituzionali citate""],
    ""anonymousSources"": ""Analisi dell'uso di fonti anonime e loro giustificazione""
  },
  ""evidenceAnalysis"": {
    ""keyFactualClaims"": [""Affermazione fattuale 1"", ""Affermazione fattuale 2""],
    ""unsupportedClaims"": [""Affermazione non adeguatamente supportata 1"", ""Affermazione non adeguatamente supportata 2""],
    ""factsVsOpinions"": ""Analisi della distinzione tra fatti, opinioni e speculazioni nell'articolo""
  },
  ""informationTriangulation"": {
    ""multipleSourceClaims"": [""Informazioni confermate da più fonti""],
    ""singleSourceClaims"": [""Affermazioni basate su fonte unica""],
    ""perspectiveDiversity"": ""Valutazione della diversità ideologica delle fonti consultate"",
    ""significantOmissions"": [""Fonti rilevanti non considerate""]
  },
  ""sourceContextAnalysis"": {
    ""publicationProfile"": ""Analisi del posizionamento editoriale di " + source + @""",
    ""trackRecord"": ""Track record della pubblicazione su temi simili"",
    ""coverageAdequacy"": ""Valutazione dell'adeguatezza della copertura rispetto alla complessità del tema"",
    ""knownBias"": ""Eventuali bias sistematici noti della pubblicazione""
  },
  ""accuracyVerification"": {
    ""dataAccuracy"": ""Valutazione della precisione di dati e statistiche"",
    ""contextIssues"": ""Analisi di problemi di contestualizzazione dei dati"",
    ""dataSelectionBias"": ""Identificazione di potenziali distorsioni nella selezione dei dati"",
    ""internalConsistency"": ""Valutazione della coerenza interna delle informazioni""
  },
  ""citationTechniques"": {
    ""circularCitations"": ""Identificazione di eventuali citazioni circolari"",
    ""pseudoExperts"": ""Rilevo dell'uso di pseudo-esperti"",
    ""cherryPicking"": ""Analisi del cherry-picking di fonti o dati"",
    ""outOfContextQuotes"": [""Citazioni utilizzate fuori contesto""]
  },
  ""reliabilityScores"": {
    ""sourceQuality"": 65,
    ""factualAccuracy"": 70,
    ""transparency"": 55,
    ""completeness"": 60,
    ""factOpinionSeparation"": 75
  },
  ""readerRecommendations"": {
    ""complementarySources"": [""Fonti complementari suggerite""],
    ""claimsToVerify"": [""Affermazioni che richiedono ulteriore verifica""],
    ""criticalQuestions"": [""Domande critiche che il lettore dovrebbe porsi""]
  },
  ""overallAssessment"": ""Valutazione complessiva dell'affidabilità dell'articolo e della qualità delle fonti utilizzate""
}

3. Includi sempre valori numerici (punteggi) per le dimensioni di affidabilità su scala 1-100.
4. Analizza attentamente la qualità, diversità e trasparenza delle fonti.
5. Identifica le affermazioni fattuali non adeguatamente supportate.
6. Fornisci raccomandazioni pratiche per il lettore.

ARTICOLO DA ANALIZZARE:

" + articleContent;

            return prompt;
        }

        /// <summary>
        /// Genera un prompt per l'analisi del discorso di un articolo
        /// </summary>
        private string GenerateDiscourseAnalysisPrompt(string articleContent, string category)
        {
            string prompt = @"
Conduci un'analisi critica del discorso sul seguente articolo giornalistico, esaminando le strutture linguistiche e discorsive che costruiscono rappresentazioni specifiche della realtà. Genera una risposta JSON strutturata secondo lo schema specificato.

REQUISITI IMPORTANTI:
1. La risposta DEVE essere in formato JSON valido.
2. La risposta deve seguire ESATTAMENTE questa struttura:

{
  ""summary"": ""Sintesi dell'articolo e delle principali conclusioni dell'analisi del discorso"",
  ""macroDiscursiveAnalysis"": {
    ""dominantDiscourses"": [""Discorso dominante 1"", ""Discorso dominante 2""],
    ""hegemonicRelations"": ""Analisi di come l'articolo riproduce o sfida discorsi egemonici"",
    ""intertextuality"": ""Esame delle relazioni intertestuali con altri testi/discorsi"",
    ""implicitAssumptions"": [""Presupposto culturale/sociale implicito 1"", ""Presupposto 2""]
  },
  ""discursiveStrategies"": [
    {
      ""type"": ""Strategia di legittimazione/delegittimazione"",
      ""description"": ""Descrizione della strategia"",
      ""examples"": [""Citazione testuale 1"", ""Citazione testuale 2""]
    },
    {
      ""type"": ""Strategia di polarizzazione"",
      ""description"": ""Descrizione della strategia"",
      ""examples"": [""Citazione testuale 1"", ""Citazione testuale 2""]
    }
  ],
  ""linguisticAnalysis"": {
    ""lexicalChoices"": {
      ""dominantFields"": [""Campo semantico dominante 1"", ""Campo semantico dominante 2""],
      ""significantTerms"": [""Termine significativo 1"", ""Termine significativo 2""]
    },
    ""syntacticStructures"": {
      ""voiceUsage"": ""Analisi dell'uso di voce attiva/passiva"",
      ""nominalizations"": ""Analisi delle nominalizzazioni"",
      ""examples"": [""Esempio di struttura sintattica significativa""]
    },
    ""modalityAnalysis"": ""Analisi delle modalità ed epistemologia (certezza vs possibilità)"",
    ""argumentStructures"": ""Analisi delle strutture argomentative e inferenze"",
    ""presuppositions"": [""Presupposizione 1"", ""Presupposizione 2""]
  },
  ""socialActorRepresentation"": {
    ""inclusionExclusion"": ""Analisi delle strategie di inclusione vs esclusione"",
    ""activationPassivation"": ""Analisi dell'attivazione vs passivizzazione degli attori"",
    ""individualCollective"": ""Analisi dell'individualizzazione vs collettivizzazione"",
    ""powerRelations"": ""Analisi delle relazioni di potere implicite""
  },
  ""narrativesAndMetaphors"": {
    ""dominantNarratives"": [""Struttura narrativa dominante 1"", ""Struttura narrativa dominante 2""],
    ""conceptualMetaphors"": [
      {
        ""metaphor"": ""Metafora concettuale"",
        ""effect"": ""Impatto sulla comprensione"",
        ""examples"": [""Esempio dall'articolo""]
      }
    ],
    ""emergingFrames"": [""Frame emergente 1"", ""Frame emergente 2""]
  },
  ""historicalContextualAnalysis"": {
    ""historicalPositioning"": ""Posizionamento del discorso in un contesto storico più ampio"",
    ""socioPoliticalRelations"": ""Analisi della relazione tra il discorso e le strutture socio-politiche"",
    ""discursiveContinuity"": ""Continuità e discontinuità con discorsi precedenti""
  },
  ""counternarratives"": {
    ""alternativeDiscourses"": [""Discorso alternativo 1"", ""Discorso alternativo 2""],
    ""excludedVoices"": [""Voce sistematicamente esclusa 1"", ""Voce sistematicamente esclusa 2""],
    ""alternativeConstructions"": ""Costruzioni linguistiche alternative per una rappresentazione più equilibrata""
  },
  ""sociopoliticalImpact"": {
    ""performativeEffects"": ""Analisi dei potenziali effetti performativi del discorso"",
    ""ideologicalImplications"": ""Implicazioni ideologiche delle costruzioni discorsive"",
    ""powerMaintenance"": ""Come il discorso contribuisce a mantenere o sfidare relazioni di potere"",
    ""publicDebateImpact"": ""Impatto potenziale sul dibattito pubblico""
  },
  ""overallAssessment"": ""Valutazione complessiva del discorso analizzato e sue implicazioni socio-politiche""
}

3. Identifica strategie discorsive concrete con esempi testuali specifici.
4. Analizza attentamente le strutture linguistiche micro e macro.
5. Esamina la rappresentazione degli attori sociali e le relazioni di potere.
6. Sviluppa contronarrative e discorsi alternativi.

ARTICOLO DA ANALIZZARE:

" + articleContent;

            // Aggiungi istruzioni specifiche per categoria se disponibili
            string categoryInstructions = category.ToLower() switch
            {
                "politics" => @"
ISTRUZIONI SPECIFICHE PER ANALISI DEL DISCORSO POLITICO:
- Analizza specificamente le costruzioni discorsive del potere politico
- Identifica le strategie di legittimazione dell'autorità e delegittimazione dell'opposizione
- Esamina l'uso di termini polarizzanti e divisivi
- Analizza la costruzione discorsiva dell'identità nazionale o di gruppo
",
                "environment" => @"
ISTRUZIONI SPECIFICHE PER ANALISI DEL DISCORSO AMBIENTALE:
- Analizza le costruzioni discorsive del rapporto uomo-natura
- Identifica le metafore dominanti utilizzate per descrivere fenomeni ambientali
- Esamina le attribuzioni di responsabilità ambientale
- Analizza le temporalità del discorso ambientale (urgenza, futuro, passato)
",
                "economy" => @"
ISTRUZIONI SPECIFICHE PER ANALISI DEL DISCORSO ECONOMICO:
- Analizza le costruzioni discorsive dei fenomeni economici come naturali o artificiali
- Identifica le metafore utilizzate per descrivere l'economia
- Esamina la rappresentazione discorsiva degli attori economici
- Analizza il framing dei conflitti di interesse economici
",
                _ => @"
ISTRUZIONI GENERICHE PER ANALISI DEL DISCORSO:
- Analizza le strutture linguistiche che costruiscono rappresentazioni della realtà
- Identifica strategie discorsive e loro funzioni sociali
- Esamina relazioni di potere implicite nel testo
- Analizza presupposti culturali e ideologici
"
            };

            return prompt + categoryInstructions;
        }

        private async Task<string> CallGeminiApiAsync(string prompt)
        {
            try
            {
                // Prepara la richiesta
                var request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = prompt
                                }
                            }
                        }
                    }
                };

                var requestJson = JsonSerializer.Serialize(request);
                var requestUrl = $"{ApiBaseUrl}?key={_apiKey}";

                // Effettua la chiamata
                var response = await _httpClient.PostAsync(
                    requestUrl,
                    new StringContent(requestJson, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserializza la risposta
                var jsonResponse = JsonDocument.Parse(responseContent).RootElement;
                var candidates = jsonResponse.GetProperty("candidates");

                if (candidates.GetArrayLength() > 0)
                {
                    var content = candidates[0].GetProperty("content");
                    var parts = content.GetProperty("parts");
                    if (parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString();
                    }
                }

                throw new Exception("Risposta Gemini non valida");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                throw;
            }
        }

        private string ExtractJsonFromResponse(string response)
        {
            // Cerca il JSON nella risposta
            int startIndex = response.IndexOf('{');
            int endIndex = response.LastIndexOf('}');

            if (startIndex >= 0 && endIndex > startIndex)
            {
                return response.Substring(startIndex, endIndex - startIndex + 1);
            }

            _logger.LogWarning("Could not extract JSON from response");
            throw new FormatException("Impossibile estrarre JSON dalla risposta di Gemini");
        }

        private AnalysisResult DeserializeAnalysis(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Prova a deserializzare direttamente
                try
                {
                    return JsonSerializer.Deserialize<AnalysisResult>(json, options);
                }
                catch (JsonException ex) when (ex.Message.Contains("$.keyTerms[0]"))
                {
                    // Se fallisce a causa del formato di keyTerms, proviamo a convertire
                    var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    // Crea un nuovo oggetto JSON con lo stesso contenuto ma con keyTerms come array di stringhe
                    using var ms = new MemoryStream();
                    using var writer = new Utf8JsonWriter(ms);

                    writer.WriteStartObject();

                    foreach (var property in root.EnumerateObject())
                    {
                        if (property.Name == "keyTerms" && property.Value.ValueKind == JsonValueKind.Array)
                        {
                            writer.WritePropertyName("keyTerms");
                            writer.WriteStartArray();

                            foreach (var item in property.Value.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("term", out var termElement))
                                {
                                    writer.WriteStringValue(termElement.GetString());

                                    // Opzionalmente memorizza anche l'analisi, ma non è richiesto per questo fix
                                }
                                else if (item.ValueKind == JsonValueKind.String)
                                {
                                    writer.WriteStringValue(item.GetString());
                                }
                            }

                            writer.WriteEndArray();

                            // Aggiungi anche keyTermsAnalysis se necessario
                            writer.WritePropertyName("keyTermsAnalysis");
                            property.Value.WriteTo(writer);
                        }
                        else
                        {
                            property.WriteTo(writer);
                        }
                    }

                    writer.WriteEndObject();
                    writer.Flush();

                    return JsonSerializer.Deserialize<AnalysisResult>(Encoding.UTF8.GetString(ms.ToArray()), options);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing analysis response");
                throw;
            }
        }
        public async Task<IdeologicalAnalysis> AnalyzeIdeologyAsync(NewsArticle article, string content)
        {
            try
            {
                _logger.LogInformation($"Inizio analisi ideologica per l'articolo: {article.Title}");

                var prompt = GenerateIdeologyPrompt(article.Title, content);
                var response = await CallGeminiApiAsync(prompt);

                // Deserializza la risposta JSON
                var ideologicalAnalysis = DeserializeIdeologicalAnalysis(response);

                // Aggiungi informazioni contestuali
                ideologicalAnalysis.ArticleId = article.Id;
                
                ideologicalAnalysis.ArticleTitle = article.Title;

                _logger.LogInformation($"Analisi ideologica completata: X={ideologicalAnalysis.XAxis}, Y={ideologicalAnalysis.YAxis}");

                return ideologicalAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'analisi ideologica dell'articolo");
                throw;
            }
        }

        private string GenerateIdeologyPrompt(string title, string content)
        {
            // Limita la lunghezza del contenuto per rispettare i limiti dell'API
            var limitedContent = content.Length > 15000 ? content.Substring(0, 15000) + "..." : content;

            return $@"
Analizza questo articolo di notizie e determina la sua posizione ideologica su due assi:

1. Asse X: da -5 (DESTRA) a +5 (SINISTRA)
2. Asse Y: da -5 (CONSERVATORE) a +5 (PROGRESSISTA)

Basa la tua valutazione su:
- Linguaggio utilizzato
- Temi trattati
- Prospettiva presentata
- Tono complessivo

Titolo: {title}

Contenuto dell'articolo:
{limitedContent}

Rispondi SOLO con un oggetto JSON nel seguente formato, SENZA commenti o spiegazioni aggiuntive:
{{
  ""x_axis"": [numero tra -5 e +5],
  ""y_axis"": [numero tra -5 e +5],
  ""explanation"": ""[Breve spiegazione in massimo 100 parole del perché hai assegnato questi valori]""
}}";
        }

        private IdeologicalAnalysis DeserializeIdeologicalAnalysis(string json)
        {
            try
            {
                // Trova l'inizio e la fine dell'oggetto JSON nella risposta
                int startIndex = json.IndexOf('{');
                int endIndex = json.LastIndexOf('}') + 1;

                if (startIndex >= 0 && endIndex > startIndex)
                {
                    string jsonObject = json.Substring(startIndex, endIndex - startIndex);

                    // Deserializza l'oggetto JSON
                    using (JsonDocument document = JsonDocument.Parse(jsonObject))
                    {
                        var root = document.RootElement;

                        return new IdeologicalAnalysis
                        {
                            XAxis = root.GetProperty("x_axis").GetSingle(),
                            YAxis = root.GetProperty("y_axis").GetSingle(),
                            Explanation = root.GetProperty("explanation").GetString(),
                            CreatedAt = DateTime.UtcNow
                        };
                    }
                }

                throw new JsonException("Formato JSON non valido nella risposta di Gemini");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la deserializzazione dell'analisi ideologica");
                throw;
            }
        }
        private string ExtractSourceFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "Fonte sconosciuta";

            try
            {
                var uri = new Uri(url);
                var host = uri.Host.ToLower();

                if (host.Contains("theguardian"))
                    return "The Guardian";

                // Rimuovi www. e prendi il dominio principale
                return host.Replace("www.", "").Split('.')[0];
            }
            catch
            {
                return "Fonte sconosciuta";
            }
        }

        public async Task<PerspectiveResult> GeneratePerspectiveAsync(string content, string title, PerspectiveType perspective, string perspectiveName)
        {
            string prompt = CreatePromptForPerspective(content, title, perspective, perspectiveName);

            var response = await CallGeminiApiAsync(prompt);
            return ParseGeminiResponse(response, perspective);
        }

        private string CreatePromptForPerspective(string content, string title, PerspectiveType perspective, string perspectiveName)
        {
            return $@"
TASK: Riscrivi un articolo dal punto di vista di un {perspectiveName}.

REGOLE IMPORTANTI:
- RISPOSTA SOLO IN JSON PURO, NESSUNA FORMATTAZIONE MARKDOWN
- NON USARE TRIPLE BACKTICK (```) O SINGOLI BACKTICK (`)
- NON USARE 'json' O ALTRI IDENTIFICATORI PRIMA DEL JSON
- INIZIA DIRETTAMENTE CON LA PARENTESI GRAFFA {{
- NESSUNA INTRODUZIONE O TESTO ESPLICATIVO

ARTICOLO ORIGINALE:
Titolo: {title}
Contenuto: {content}

Rispondi SOLO con un oggetto JSON nel seguente formato:
{{
  ""title"": ""Nuovo titolo dell'articolo dal punto di vista di un {perspectiveName}"",
  ""content"": ""Contenuto dell'articolo riscritto dal punto di vista di un {perspectiveName}..."",
  ""summary"": ""Breve riassunto dell'articolo in 2-3 frasi"",
  ""keywords"": [""parola1"", ""parola2"", ""parola3"", ""parola4"", ""parola5""]
}}

GUIDA PER IL PUNTO DI VISTA DEL {perspectiveName.ToUpper()}:
{prospectivePromptDetail(perspective)}
";
        }

        private string prospectivePromptDetail(PerspectiveType perspective)
        {
            return perspective switch
            {
                PerspectiveType.Activist => @"Come attivista:
- Usa un linguaggio emotivo e appassionato
- Evidenzia le ingiustizie sociali e i problemi sistemici
- Sottolinea l'urgenza dell'azione
- Usa frasi che chiamano all'impegno e alla mobilitazione
- Critica l'inazione delle istituzioni",

                PerspectiveType.Politician => @"Come politico:
- Usa un linguaggio diplomatico e misurato
- Bilancia diverse posizioni
- Considera le conseguenze politiche degli eventi
- Evita affermazioni troppo dirette o controverse
- Fai riferimento a politiche e soluzioni istituzionali",

                PerspectiveType.Expert => @"Come esperto:
- Usa terminologia tecnica e specialistica
- Basa le affermazioni su dati e ricerche
- Mantieni un tono analitico e distaccato
- Fornisci contesto storico o scientifico
- Esamina cause ed effetti con metodo",

                PerspectiveType.Victim => @"Come persona direttamente colpita:
- Usa il racconto in prima persona
- Esprimi emozioni e reazioni personali
- Descrivi l'esperienza diretta e le conseguenze sulla tua vita
- Condividi speranze, paure e difficoltà
- Rendi umano e concreto l'impatto degli eventi",

                _ => "Presenta i fatti in modo neutrale e obiettivo"
            };
        }

       

        private PerspectiveResult ParseGeminiResponse(string response, PerspectiveType perspective)
        {
            try
            {
                _logger.LogInformation($"Ricevuta risposta da Gemini (lunghezza: {response?.Length ?? 0})");

                string cleanedResponse = response;

                // 1. Verifica se la risposta è in formato markdown e contiene blocchi di codice
                if (response.StartsWith("```") || response.StartsWith("`"))
                {
                    _logger.LogInformation("Rilevato blocco di codice markdown nella risposta");

                    // Rimuovi i blocchi di codice markdown (```json ... ```)
                    if (response.StartsWith("```"))
                    {
                        int startIndex = response.IndexOf('\n');
                        if (startIndex > 0)
                        {
                            // Trova la fine del blocco di codice
                            int endIndex = response.LastIndexOf("```");
                            if (endIndex > startIndex)
                            {
                                // Estrai solo il contenuto all'interno del blocco di codice
                                cleanedResponse = response.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                                _logger.LogInformation($"Estratto contenuto dal blocco di codice markdown (lunghezza: {cleanedResponse.Length})");
                            }
                        }
                    }
                    else if (response.StartsWith("`"))
                    {
                        // Rimuovi i singoli backtick
                        cleanedResponse = response.Trim('`').Trim();
                        _logger.LogInformation("Rimossi backtick singoli dalla risposta");
                    }
                }

                // 2. Prova a cercare un oggetto JSON nella risposta pulita
                int jsonStartIndex = cleanedResponse.IndexOf('{');
                int jsonEndIndex = cleanedResponse.LastIndexOf('}');

                if (jsonStartIndex >= 0 && jsonEndIndex > jsonStartIndex)
                {
                    string jsonString = cleanedResponse.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                    _logger.LogInformation($"JSON identificato nella risposta (lunghezza: {jsonString.Length})");

                    try
                    {
                        // Tenta di analizzare direttamente come un oggetto JSON
                        var resultDoc = JsonDocument.Parse(jsonString);
                        var root = resultDoc.RootElement;

                        // Verifica che tutte le proprietà richieste esistano
                        if (root.TryGetProperty("title", out _) &&
                            root.TryGetProperty("content", out _) &&
                            root.TryGetProperty("summary", out _) &&
                            root.TryGetProperty("keywords", out _))
                        {
                            _logger.LogInformation("Trovate tutte le proprietà richieste nel JSON");

                            // Deserializza e restituisci il risultato
                            return new PerspectiveResult
                            {
                                Title = root.GetProperty("title").GetString(),
                                Content = root.GetProperty("content").GetString(),
                                Summary = root.GetProperty("summary").GetString(),
                                Keywords = JsonSerializer.Deserialize<List<string>>(root.GetProperty("keywords").ToString())
                            };
                        }
                        else
                        {
                            _logger.LogWarning("JSON trovato ma mancano alcune proprietà richieste");
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Errore durante il parsing del JSON estratto");
                    }
                }

                // 3. Se arriviamo qui, proviamo con l'approccio standard (per compatibilità)
                try
                {
                    var jsonDocument = JsonDocument.Parse(response);

                    if (jsonDocument.RootElement.TryGetProperty("candidates", out var candidates) &&
                        candidates.GetArrayLength() > 0 &&
                        candidates[0].TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0 &&
                        parts[0].TryGetProperty("text", out var text))
                    {
                        string textContent = text.GetString();

                        // Cerca JSON nel contenuto testuale
                        jsonStartIndex = textContent.IndexOf('{');
                        jsonEndIndex = textContent.LastIndexOf('}');

                        if (jsonStartIndex >= 0 && jsonEndIndex > jsonStartIndex)
                        {
                            string jsonContent = textContent.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                            var resultDoc = JsonDocument.Parse(jsonContent);
                            var root = resultDoc.RootElement;

                            if (root.TryGetProperty("title", out _) &&
                                root.TryGetProperty("content", out _) &&
                                root.TryGetProperty("summary", out _) &&
                                root.TryGetProperty("keywords", out _))
                            {
                                return new PerspectiveResult
                                {
                                    Title = root.GetProperty("title").GetString(),
                                    Content = root.GetProperty("content").GetString(),
                                    Summary = root.GetProperty("summary").GetString(),
                                    Keywords = JsonSerializer.Deserialize<List<string>>(root.GetProperty("keywords").ToString())
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fallito anche l'approccio standard di parsing");
                }

                // 4. Fallback se tutto fallisce
                _logger.LogWarning("Impossibile estrarre dati JSON validi dalla risposta");

                return new PerspectiveResult
                {
                    Title = $"Prospettiva {perspective}",
                    Content = "Non è stato possibile generare questa prospettiva. Riprova più tardi.",
                    Summary = "Contenuto non disponibile",
                    Keywords = new List<string>() { "errore", "generazione", "fallita", "prospettiva", "riprova" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generale durante l'elaborazione della risposta");

                return new PerspectiveResult
                {
                    Title = $"Prospettiva {perspective}",
                    Content = "Si è verificato un errore durante l'elaborazione della risposta. Riprova più tardi.",
                    Summary = "Contenuto non disponibile",
                    Keywords = new List<string>() { "errore", "elaborazione", "fallita", "prospettiva", "riprova" }
                };
            }
        }

    }
}