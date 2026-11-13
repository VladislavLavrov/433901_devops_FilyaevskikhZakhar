
using System;
using System.ComponentModel.DataAnnotations;

namespace Calculator_17.Models
{
    public class CalculationHistory
    {
        [Key]
        public int Id { get; set; }

        public string Expression { get; set; } = string.Empty;

        public string Result { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
