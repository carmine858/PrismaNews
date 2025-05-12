using System;
using System.ComponentModel.DataAnnotations;

namespace Prisma.Models
{
    public class IdeologicalAnalysis
    {
        public int Id { get; set; }

        [Required]
        public string ArticleId { get; set; }

        [Required]
        public float XAxis { get; set; }  // Destra (-5) <-> Sinistra (+5)

        [Required]
        public float YAxis { get; set; }  // Conservatore (-5) <-> Progressista (+5)

        [Required]
        public string Explanation { get; set; }

        public string Source { get; set; }  // Nome della fonte dell'articolo

        public string ArticleTitle { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Metodo helper per convertire le coordinate in percentuali (0-100)
        public int XAxisPercentage => (int)((XAxis + 5) * 10);
        public int YAxisPercentage => (int)((YAxis + 5) * 10);
    }
}