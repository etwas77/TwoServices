using Contracts.Config;
using Contracts.DTO;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ServiceA.Services
{
    public class OrderPublisherService
    {
        private readonly ILogger<OrderPublisherService> _logger;
        private readonly RabbitMqSettings _rabbitMqSettings;

        public OrderPublisherService(
            ILogger<OrderPublisherService> logger,
            IOptions<RabbitMqSettings> rabbitMqSettings
        )
        {
            _logger = logger;
            _rabbitMqSettings = rabbitMqSettings.Value;
        }

        public async Task PublishOrderAsync(OrderDto orderDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Publishing order: {orderDto}", orderDto);
            var payload = JsonSerializer.Serialize(orderDto);
            var body = Encoding.UTF8.GetBytes(payload);

            var factory = new ConnectionFactory() { 
                HostName = _rabbitMqSettings.HostName,
                Port = _rabbitMqSettings.Port,
                UserName = _rabbitMqSettings.UserName,
                Password = _rabbitMqSettings.Password,
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _rabbitMqSettings.OrderQueue,
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _rabbitMqSettings.FailedOrdersQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _rabbitMqSettings.OrderQueue,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Order published successfully: {orderId} to queue: {OrderQueue}", orderDto.Id, _rabbitMqSettings.OrderQueue);
        }
    }
}
