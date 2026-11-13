
using Calculator_17.Data;
using Calculator_17.Models;
using Calculator_17.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Calculator_17.Controllers;

public class CalculatorController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly CalculationResultStore _resultStore;
    private readonly KafkaProducerService _producer;
    public CalculatorController(
    ApplicationDbContext db,
    CalculationResultStore resultStore,
    KafkaProducerService producer)
    {
        _db = db;
        _resultStore = resultStore;
        _producer = producer;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new CalculatorModel();
        await LoadHistory(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
    CalculatorModel model,
    CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Error = "Проверьте введённые числа.";
            await LoadHistory(model);
            return View(model);
        }

        if (model.Operation == "/" && model.SecondNumber == 0)
        {
            model.Error = "На ноль делить нельзя.";
            await LoadHistory(model);
            return View(model);
        }

        if (model.Operation != "+" &&
            model.Operation != "-" &&
            model.Operation != "*" &&
            model.Operation != "/")
        {
            model.Error = "Выберите математическое действие.";
            await LoadHistory(model);
            return View(model);
        }

        var message = new CalculationMessage
        {
            RequestId = Guid.NewGuid(),
            FirstNumber = model.FirstNumber,
            SecondNumber = model.SecondNumber,
            Operation = model.Operation,
            CreatedAt = DateTime.Now
        };

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

            timeout.CancelAfter(TimeSpan.FromSeconds(20));

            var resultTask = _resultStore.WaitForResultAsync(
                message.RequestId,
                timeout.Token);

            await _producer.SendCalculationAsync(message);

            var resultMessage = await resultTask;

            model.Result = resultMessage.Result;
            model.Error = resultMessage.Error;
        }
        catch (OperationCanceledException)
        {
            model.Error = "Не удалось получить ответ от Kafka за 20 секунд.";
        }
        catch (Exception ex)
        {
            model.Error = "Ошибка отправки или получения сообщения Kafka: " + ex.Message;
        }

        await LoadHistory(model);
        return View(model);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Callback(
        [FromBody] CalculationMessage message)
    {
        if (message == null)
            return BadRequest();

        _db.CalculationHistories.Add(new CalculationHistory
        {
            Expression = $"{message.FirstNumber} {message.Operation} {message.SecondNumber}",
            Result = message.Result ?? message.Error ?? "Ошибка",
            CreatedAt = message.CreatedAt
        });

        await _db.SaveChangesAsync();

        _resultStore.Complete(message);

        return Ok();
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
