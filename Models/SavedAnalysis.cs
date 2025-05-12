using System;
using System.ComponentModel.DataAnnotations;

namespace Prisma.Models
{
    public class SavedAnalysis
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }  // Assumo che User.Id sia un int, modifica se diverso

        // Relazione con User (opzionale ma utile per EF Core)
       

        [Required]
        public string ArticleTitle { get; set; }

        [Required]
        public string ArticleUrl { get; set; }

        [Required]
        public string GeminiAnalysisJson { get; set; } // JSON completo dell'analisi AI

        public DateTime SavedAt { get; set; } = DateTime.Now;
    }
}