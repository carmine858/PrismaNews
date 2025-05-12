//using System.Collections.Generic;

//namespace PrismaNews.Models
//{
//    public static class AnalysisPrompts
//    {
//        private const string BasePrompt = @"
//Analizza il seguente articolo giornalistico e fornisci una decostruzione critica strutturata nei seguenti elementi e se necessario traducilo in lingua italiana:

//1. RIEPILOGO: Breve sintesi dell'articolo (max 5 frasi).

//2. FRAMING PRINCIPALE: Come l'articolo inquadra la questione e quale prospettiva privilegia.

//3. BIAS COGNITIVI: Identifica 2-4 bias cognitivi presenti nell'articolo. Per ciascuno fornisci:
//   - Tipo di bias
//   - Breve descrizione di come si manifesta nell'articolo
//   - Livello di gravità (da 1 a 100)

//4. TECNICHE RETORICHE: Identifica 2-4 tecniche retoriche o persuasive utilizzate. Per ciascuna fornisci:
//   - Nome della tecnica
//   - Breve descrizione di come viene utilizzata nell'articolo

//5. PROSPETTIVA ALTERNATIVA: Una breve descrizione di come la stessa notizia potrebbe essere raccontata da una prospettiva diversa.

//6. TERMINI CHIAVE: 5 parole o espressioni particolarmente significative utilizzate nell'articolo.

//7. PUNTEGGI:
//   - Punteggio di bias (1-100): quanto l'articolo presenta una visione sbilanciata
//   - Punteggio di persuasione (1-100): quanto l'articolo utilizza tecniche persuasive efficaci

//8. VALUTAZIONE COMPLESSIVA: Un breve paragrafo che valuta criticamente l'articolo nel suo insieme.

//Formatta la tua risposta in modo strutturato e conciso, utilizzando i titoli delle sezioni sopra indicate.

//ARTICOLO DA ANALIZZARE:
//";

//        private static readonly Dictionary<string, string> CategorySpecificInstructions = new()
//        {
//            ["politics"] = @"
//Concentrati sui seguenti aspetti aggiuntivi:
//- Identificazione del posizionamento politico dell'articolo
//- Rilevamento di bias ideologici specifici (conservatore, progressista, liberale, ecc.)
//- Uso del linguaggio politicamente connotato
//- Selezione strategica di fatti e statistiche per supportare una particolare posizione
//- Rappresentazione degli attori politici (heroes vs. villains framing)
//",

//            ["environment"] = @"
//Concentrati sui seguenti aspetti aggiuntivi:
//- Identificazione della prospettiva sull'ambiente (antropocentrica, ecocentrica, ecc.)
//- Rilevamento dell'uso di linguaggio apocalittico o ottimistico
//- Analisi di come vengono presentati i dati scientifici
//- Identificazione degli interessi economici sottesi al discorso ambientale
//- Rappresentazione del rapporto uomo-natura
//",

//            ["technology"] = @"
//Concentrati sui seguenti aspetti aggiuntivi:
//- Identificazione dell'atteggiamento verso la tecnologia (tecnottimismo vs tecnoallarmismo)
//- Rilevamento dell'uso del determinismo tecnologico
//- Analisi della rappresentazione degli sviluppatori/aziende tech
//- Identificazione del framing della tecnologia (soluzione vs problema)
//- Presenza di gergo tecnico e sue funzioni retoriche
//",

//            ["economy"] = @"
//Concentrati sui seguenti aspetti aggiuntivi:
//- Identificazione dell'orientamento economico sotteso (liberal, keynesiano, neoliberista, ecc.)
//- Rilevamento dell'uso di indicatori economici selezionati strategicamente
//- Analisi della rappresentazione dei diversi attori economici
//- Identificazione dei gruppi sociali avvantaggiati o svantaggiati dalla narrativa proposta
//- Presenza di semplificazioni di concetti economici complessi
//",

//            ["society"] = @"
//Concentrati sui seguenti aspetti aggiuntivi:
//- Identificazione delle rappresentazioni di gruppi sociali e minoranze
//- Rilevamento di stereotipi espliciti o impliciti
//- Analisi dell'uso di storie aneddotiche per generalizzazioni
//- Identificazione del framing delle questioni sociali (problema individuale vs sistemico)
//- Rappresentazione di cause e responsabilità per problemi sociali
//"
//        };

//        public static string GetPromptForCategory(string category, string articleText)
//        {
//            string specificInstructions = "";

//            if (CategorySpecificInstructions.TryGetValue(category.ToLower(), out var instructions))
//            {
//                specificInstructions = instructions;
//            }

//            return $"{BasePrompt}{specificInstructions}\n\n{articleText}";
//        }
//    }
//}