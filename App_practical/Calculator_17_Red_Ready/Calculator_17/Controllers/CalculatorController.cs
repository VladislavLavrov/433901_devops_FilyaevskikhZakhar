using Calculator_17.Data;
using Calculator_17.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Calculator_17.Controllers;

public class CalculatorController : Controller
{
    private readonly ApplicationDbContext _db;
    public CalculatorController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new CalculatorModel();
        await LoadHistory(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CalculatorModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Error = "Проверьте введённые числа.";
            await LoadHistory(model);
            return View(model);
        }

        double result;
        string symbol;
        switch (model.Operation)
        {
            case "+": result = model.FirstNumber + model.SecondNumber; symbol = "+"; break;
            case "-": result = model.FirstNumber - model.SecondNumber; symbol = "−"; break;
            case "*": result = model.FirstNumber * model.SecondNumber; symbol = "×"; break;
            case "/":
                if (model.SecondNumber == 0)
                {
                    model.Error = "На ноль делить нельзя.";
                    await LoadHistory(model);
                    return View(model);
                }
                result = model.FirstNumber / model.SecondNumber; symbol = "÷"; break;
            default:
                model.Error = "Выберите математическое действие.";
                await LoadHistory(model);
                return View(model);
        }

        model.Result = result.ToString("G", CultureInfo.CurrentCulture);
        var expression = $"{model.FirstNumber.ToString("G", CultureInfo.CurrentCulture)} {symbol} {model.SecondNumber.ToString("G", CultureInfo.CurrentCulture)}";
        _db.CalculationHistories.Add(new CalculationHistory
        {
            Expression = expression,
            Result = model.Result,
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();
        await LoadHistory(model);
        return View(model);
    }

    private async Task LoadHistory(CalculatorModel model)
    {
        model.History = await _db.CalculationHistories
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(50)
            .ToListAsync();
    }
}
