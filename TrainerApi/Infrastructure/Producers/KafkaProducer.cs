using System.Text.Json;
using Confluent.Kafka;
using TrainerApi.Events;

namespace TrainerApi.Infrastructure.Producers;

public class KafkaProducer : IMessageBrokerProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private bool _disposed = false;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            Acks = Acks.All,
            EnableIdempotence = true,
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => 
                _logger.LogError("Error in Kafka producer: {Error}", e.Reason))
            .Build();
        
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
    
    public async Task ProduceAsync<T>(T message, CancellationToken cancellationToken) where T : class, IEventMessage
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(KafkaProducer));
        try
        {
            var serializedMessage = JsonSerializer.Serialize(message, _jsonSerializerOptions);
            var kafkaMessage = new Message<string, string>
            {
                Value = serializedMessage,
                Key = message.GetEventKey() ?? Guid.NewGuid().ToString(),
                Headers = new Headers
                {
                    //AVRO -- esquemas o formatos para mensajeria
                    //DLQ -- Dead letter queue
                    //Reintentos en message brokers
                    //Poison Message
                    { "content-type", "application/json"u8.ToArray() },
                    { "produced-at", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("0")) },
                    { "idempotence-key", Guid.NewGuid().ToByteArray() },
                }
            };

            //trainer-created
            //trainer-updated
            //trainer-deleted
            var result = await _producer.ProduceAsync(message.Topic, kafkaMessage, cancellationToken);
            _logger.LogInformation(
                "Message delivered to {Topic} partition {Partition} at offset {Offset} with key {Key}",
                message.Topic, result.Partition.Value, result.Offset.Value, message.GetEventKey());
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError("Error in Kafka producer: {Error} with key {Key}", ex.Error.Reason, message.GetEventKey());
        }
        catch (Exception ex)
        {
            //Outbox pattern --> Para el publisher
            //Inbox patter --> para los consumidores
            //SAGA --> Transacciones con eventos (Coreografia y Orquestacion)
            _logger.LogError("Error in Kafka producer: {Error} with key {Key}", ex.Message, message.GetEventKey());
        }
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
            _disposed = true;
        }
    }
}