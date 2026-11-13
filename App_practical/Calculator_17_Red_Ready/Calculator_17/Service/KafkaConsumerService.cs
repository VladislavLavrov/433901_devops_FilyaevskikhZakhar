
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Calculator_17.Models;
using Confluent.Kafka;

namespace Calculator_17.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<KafkaConsumerService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = "calculator-17-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        var topic = _configuration["Kafka:Topic"];

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(topic);

        var httpClient = _httpClientFactory.CreateClient();

        var callbackUrl = _configuration["Kafka:CallbackUrl"]
            ?? "http://127.0.0.1:5518/Calculator/Callback";

        _logger.LogInformation("Kafka Consumer запущен. Топик: {Topic}", topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var kafkaMessage = consumer.Consume(stoppingToken);

                var message = JsonSerializer.Deserialize<CalculationMessage>(
                    kafkaMessage.Message.Value);

                if (message == null)
                    continue;

                Calculate(message);

                var response = await httpClient.PostAsJsonAsync(
                    callbackUrl,
                    message,
                    stoppingToken);

                response.EnsureSuccessStatusCode();

                consumer.Commit(kafkaMessage);

                _logger.LogInformation(
                    "Сообщение {RequestId} обработано и передано контроллеру",
                    message.RequestId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки сообщения Kafka");
                await Task.Delay(3000, stoppingToken);
            }
        }

        consumer.Close();
    }

    private static void Calculate(CalculationMessage message)
    {
        try
        {
            double result;

            switch (message.Operation)
            {
                case "+":
                    result = message.FirstNumber + message.SecondNumber;
                    break;
                case "-":
                    result = message.FirstNumber - message.SecondNumber;
                    break;
                case "*":
                    result = message.FirstNumber * message.SecondNumber;
                    break;
                case "/":
                    if (message.SecondNumber == 0)
                        throw new DivideByZeroException();

                    result = message.FirstNumber / message.SecondNumber;
                    break;
                default:
                    throw new InvalidOperationException("Неизвестная операция.");
            }

            message.Result = result.ToString("G", CultureInfo.CurrentCulture);
            message.Error = null;
        }
        catch (DivideByZeroException)
        {
            message.Result = null;
            message.Error = "На ноль делить нельзя.";
        }
        catch (Exception)
        {
            message.Result = null;
            message.Error = "Ошибка вычисления.";
        }
    }
}
