
using System.Text.Json;
using Calculator_17.Models;
using Confluent.Kafka;

namespace Calculator_17.Services;

public class KafkaProducerService
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;

    public KafkaProducerService(IConfiguration configuration)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka BootstrapServers не настроен.");

        _topic = configuration["Kafka:Topic"]
            ?? throw new InvalidOperationException("Kafka Topic не настроен.");

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task SendCalculationAsync(CalculationMessage message)
    {
        var json = JsonSerializer.Serialize(message);

        await _producer.ProduceAsync(
            _topic,
            new Message<Null, string> { Value = json });
    }
}
