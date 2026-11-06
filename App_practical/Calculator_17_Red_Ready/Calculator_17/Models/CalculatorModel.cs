using System.ComponentModel.DataAnnotations;

namespace Calculator_17.Models;

public class CalculatorModel
{
    [Required]
    public double FirstNumber { get; set; }

    [Required]
    public double SecondNumber { get; set; }

    public string Operation { get; set; } = "+";

    public string? Result { get; set; }

    public string? Error { get; set; }
}
