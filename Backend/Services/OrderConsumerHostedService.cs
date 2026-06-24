using Backend.Repositories;
using Contracts.Config;
using Contracts.DTO;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Backend.Services
{
    public class OrderConsumerHostedService : BackgroundService
    {
        private readonly ILogger<OrderConsumerHostedService> _logger;
        private readonly RabbitMqSettings _rabbitMqSettings;
        private IConnection? _connection;
        private IChannel? _channel;
        //private readonly OrderRepository _orderRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OrderConsumerHostedService(
            ILogger<OrderConsumerHostedService> logger, 
            IOptions<RabbitMqSettings> rabbitMqSettings,
            //OrderRepository orderRepository,
            IServiceScopeFactory serviceScopeFactory
            )
        {
            _logger = logger;
            _rabbitMqSettings = rabbitMqSettings.Value;
            //_orderRepository = orderRepository;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderConsumerHostedService is starting. Listening to queue: {OrderQueue}", _rabbitMqSettings.OrderQueue);
            await InitializeRabbitMqAsync(stoppingToken);

            await _channel!.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,   // give one message at a time to the consumer
                global: false,
                cancellationToken: stoppingToken); // Fair dispatch
            _logger.LogInformation("RabbitMQ Qos configured for queue {OrderQueue}", _rabbitMqSettings.OrderQueue);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var orderRepository = scope.ServiceProvider.GetRequiredService<OrderRepository>();

                _logger.LogInformation("received message with delivery tag {DeliveryTag}", ea.DeliveryTag);
                var message = string.Empty;
                _logger.LogInformation("Received message from queue {OrderQueue}: {Message}", _rabbitMqSettings.OrderQueue, message);
                try
                {
                    var body = ea.Body.ToArray();
                    message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("message with delivery tag {DeliveryTag} received: {Message}", ea.DeliveryTag, message);

                    var orderDto = JsonSerializer.Deserialize<OrderDto>(message);
                    if (orderDto is null)
                    {
                        _logger.LogError("Failed to deserialize message from queue {OrderQueue}: {Message}", _rabbitMqSettings.OrderQueue, message);
                        await _channel!.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                        return;
                    }

                    var existingOrder = await orderRepository.GetByIdAsync(orderDto.Id);
                    if (existingOrder is not null)
                    {
                        _logger.LogInformation("Order with ID {OrderId} already exists. Skipping processing.", orderDto.Id);
                        await _channel!.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                        return;
                    }

                    var order = new Models.Order
                    {
                        Id = orderDto.Id,
                        CustomerName = orderDto.CustomerName,
                        Items = orderDto.Items.Select(i => new Models.Item
                        {
                            Id = i.Id,
                            Name = i.Name,
                            Quantity = i.Quantity
                        }).ToList()
                    };
                    await orderRepository.CreateAsync(order);
                    _logger.LogInformation("Order with ID {OrderId} processed and saved to database.", orderDto.Id);

                    await _channel!.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the message with delivery tag {DeliveryTag}", ea.DeliveryTag);
                    var failedMessage = new
                    {
                        originalMessage = message,
                        error = ex.Message,
                        queue = _rabbitMqSettings.OrderQueue,
                        deliveryTag = ea.DeliveryTag
                    };
                    var failedPayload = JsonSerializer.Serialize(failedMessage);
                    var failedBody = Encoding.UTF8.GetBytes(failedPayload);

                    await _channel!.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: _rabbitMqSettings.FailedOrdersQueue,
                        body: failedBody,
                        cancellationToken: stoppingToken
                        );

                    _logger.LogWarning("message with tag {DeliveryTag} moved to queue {FailedOrdersQueue}", _rabbitMqSettings.FailedOrdersQueue, ea.DeliveryTag);
                    await _channel!.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(
                    queue: _rabbitMqSettings.OrderQueue,
                    autoAck: false, // manual acknowledgment
                    consumer: consumer,
                    cancellationToken: stoppingToken);

            _logger.LogInformation("Consumer attached to queue {OrderQueue}", _rabbitMqSettings.OrderQueue);



            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Simulate work
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping order consumer hosted service");
            if(_channel is not null)
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
            }
            if(_connection is not null)
            {
                await _connection.CloseAsync(cancellationToken);
                await _connection.DisposeAsync();
            }
            await base.StopAsync(cancellationToken);
        }

        private async Task InitializeRabbitMqAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _rabbitMqSettings.HostName,
                UserName = _rabbitMqSettings.UserName,
                Password = _rabbitMqSettings.Password,
                Port = _rabbitMqSettings.Port
            };
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _logger.LogInformation("RabbitMQ connection established to {HostName}:{Port}", _rabbitMqSettings.HostName, _rabbitMqSettings.Port);

            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("RabbitMQ channel created for queue: {OrderQueue}", _rabbitMqSettings.OrderQueue);

            await _channel.QueueDeclareAsync(
                queue: _rabbitMqSettings.OrderQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);
            _logger.LogInformation("RabbitMQ queue declared: {OrderQueue}", _rabbitMqSettings.OrderQueue);

            await _channel.QueueDeclareAsync(
                queue: _rabbitMqSettings.FailedOrdersQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);
            _logger.LogInformation("RabbitMQ queue declared: {FailedOrdersQueue}", _rabbitMqSettings.FailedOrdersQueue);
        }
    }
}
