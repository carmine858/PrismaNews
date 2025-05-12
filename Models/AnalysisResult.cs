using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Prisma.Models
{
    public class AnalysisResult
    {
        public string ArticleId { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
        public string Url { get; set; }
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
        // Nuovi campi per supportare le analisi avanzate
        public bool IsComparative { get; set; } = false;
        public bool IsSourceAnalysis { get; set; } = false;
        public bool IsDiscourseAnalysis { get; set; } = false;

        [JsonPropertyName("keyTermsAnalysis")]
        public List<KeyTermAnalysis> KeyTermsAnalysis { get; set; } = new List<KeyTermAnalysis>();

        // Aggiungi questa proprietà per supportare l'analisi delle fonti
        [JsonPropertyName("sourceAnalysis")]
        public SourceQualityAnalysis SourceAnalysis { get; set; }

        // Aggiungi questa proprietà per supportare la prospettiva di sintesi
        [JsonPropertyName("synthesisPerspective")]
        public string SynthesisPerspective { get; set; }

        // Altri campi per supportare le nuove strutture JSON se necessario...



        public Dictionary<string, List<string>> OmissionsMap { get; set; }
        public Dictionary<string, int> AdditionalScores { get; set; }

        // Analisi Gemini
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("primaryFraming")]
        public string PrimaryFraming { get; set; }

        [JsonPropertyName("framing")]
        public List<string> FramingTechniques { get; set; } = new List<string>();

        [JsonPropertyName("bias")]
        public List<BiasFinding> Biases { get; set; } = new List<BiasFinding>();

        [JsonPropertyName("rhetoric")]
        public List<RhetoricTechnique> RhetoricTechniques { get; set; } = new List<RhetoricTechnique>();

        [JsonPropertyName("counterArguments")]
        public List<string> CounterArguments { get; set; } = new List<string>();

        [JsonPropertyName("alternativePerspective")]
        public string AlternativePerspective { get; set; }

        [JsonPropertyName("keyTerms")]
        public List<string> KeyTerms { get; set; } = new List<string>();

        [JsonPropertyName("scores")]
        public AnalysisScores Scores { get; set; } = new AnalysisScores();

        [JsonPropertyName("overallAssessment")]
        public string OverallAssessment { get; set; }

        // Metadati
        public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
        public bool IsComplete { get; set; }
        public string RawAnalysisJson { get; set; }

        // Helper methods
        public string GetBiasClass() => Scores?.BiasScore switch
        {
            >= 75 => "high",
            >= 50 => "medium",
            _ => "low"
        };

        public string GetPersuasionClass() => Scores?.PersuasionScore switch
        {
            >= 75 => "high",
            >= 50 => "medium",
            _ => "low"
        };
    }

    public class BiasFinding
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("severity")]
        public int Severity { get; set; }

        [JsonPropertyName("examples")]
        public List<string> Examples { get; set; } = new List<string>();

        public string GetSeverityClass() => Severity switch
        {
            >= 75 => "severe",
            >= 50 => "moderate",
            _ => "mild"
        };
    }

    public class RhetoricTechnique
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("examples")]
        public List<string> Examples { get; set; } = new List<string>();

        [JsonPropertyName("effect")]
        public string Effect { get; set; }
    }


    // Aggiungi questa classe per l'analisi dei termini chiave
    public class KeyTermAnalysis
    {
        [JsonPropertyName("term")]
        public string Term { get; set; }

        [JsonPropertyName("analysis")]
        public string Analysis { get; set; }
    }

    // Aggiungi questa classe per l'analisi della qualità delle fonti
    public class SourceQualityAnalysis
    {
        [JsonPropertyName("quality")]
        public string Quality { get; set; }

        [JsonPropertyName("variety")]
        public string Variety { get; set; }

        [JsonPropertyName("missingPerspectives")]
        public List<string> MissingPerspectives { get; set; } = new List<string>();
    }
    public class AnalysisScores
    {
        [JsonPropertyName("bias")]
        public int BiasScore { get; set; }

        [JsonPropertyName("persuasion")]
        public int PersuasionScore { get; set; }

        [JsonPropertyName("complexity")]
        public int ComplexityScore { get; set; }

        [JsonPropertyName("emotionality")]
        public int EmotionalityScore { get; set; }

        // Aggiungi questi nuovi punteggi
        [JsonPropertyName("factualAccuracy")]
        public int FactualAccuracyScore { get; set; }

        [JsonPropertyName("sourceTransparency")]
        public int SourceTransparencyScore { get; set; }

        [JsonPropertyName("perspectiveBalance")]
        public int BalanceScore { get; set; }
    }

    public class SourceAnalysisData
    {
        public List<string> PrimarySources { get; set; }
        public List<string> SecondarySources { get; set; }
        public List<Expert> Experts { get; set; }
        public List<string> InstitutionalSources { get; set; }
        public string AnonymousSources { get; set; }
    }

    public class Expert
    {
        public string Name { get; set; }
        public string Credentials { get; set; }
        public string Relevance { get; set; }
        public string PotentialBias { get; set; }
    }

    public class FramingComparison
    {
        public List<string> ConvergencePoints { get; set; }
        public List<string> DivergencePoints { get; set; }
        public List<FrameAnalysis> FrameAnalysis { get; set; }
    }

    public class FrameAnalysis
    {
        public string Source { get; set; }
        public string DominantFrame { get; set; }
        public string ActorPortrayal { get; set; }
        public string Causality { get; set; }
        public string Implications { get; set; }
        public string Solutions { get; set; }
    }
}