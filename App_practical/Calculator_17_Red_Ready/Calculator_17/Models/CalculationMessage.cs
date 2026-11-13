
namespace Calculator_17.Models;

public class CalculationMessage
{
    public Guid RequestId { get; set; } = Guid.NewGuid();

    public double FirstNumber { get; set; }

    public double SecondNumber { get; set; }

    public string Operation { get; set; } = "+";

    public string? Result { get; set; }

    public string? Error { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
