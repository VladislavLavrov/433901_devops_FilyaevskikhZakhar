using System.ComponentModel.DataAnnotations;

namespace Calculator_17.Models;

public class CalculationHistory
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Expression { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Result { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
