using Calculator_17.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Calculator_17.Controllers;

public class CalculatorController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new CalculatorModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(CalculatorModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Error = "Проверьте введённые числа.";
            return View(model);
        }

        double result;

        switch (model.Operation)
        {
            case "+":
                result = model.FirstNumber + model.SecondNumber;
                break;
            case "-":
                result = model.FirstNumber - model.SecondNumber;
                break;
            case "*":
                result = model.FirstNumber * model.SecondNumber;
                break;
            case "/":
                if (model.SecondNumber == 0)
                {
                    model.Error = "На ноль делить нельзя.";
                    return View(model);
                }

                result = model.FirstNumber / model.SecondNumber;
                break;
            default:
                model.Error = "Выберите математическое действие.";
                return View(model);
        }

        model.Result = result.ToString("G", CultureInfo.CurrentCulture);
        return View(model);
    }
}
