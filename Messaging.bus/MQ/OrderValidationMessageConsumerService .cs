using Messaging.bus.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace Messaging.bus.MQ;

public class OrderValidationMessageConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderValidationMessageConsumerService> _logger;
    private readonly RabbitMQSetting _rabbitMqSetting;
    private IConnection _connection;
    private IModel _channel;

    public OrderValidationMessageConsumerService(IOptions<RabbitMQSetting> rabbitMqSetting, IServiceProvider serviceProvider, ILogger<OrderValidationMessageConsumerService> logger)
    {
        _rabbitMqSetting = rabbitMqSetting.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSetting.HostName,
            UserName = _rabbitMqSetting.UserName,
            Password = _rabbitMqSetting.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        StartConsuming(RabbitMQQueues.OrderValidationQueue, stoppingToken);
        await Task.CompletedTask;
    }

    private void StartConsuming(string queueName, CancellationToken cancellationToken)
    {
        _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            bool processedSuccessfully = false;
            try
            {
                processedSuccessfully = await ProcessMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred while processing message from queue {queueName}: {ex}");
            }

            if (processedSuccessfully)
            {
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            else
            {
                _channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    private async Task<bool> ProcessMessageAsync(string message)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                //var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                //var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                //var orderValidationRepository = scope.ServiceProvider.GetRequiredService<IValidationRepository>();
                var orderValidation = JsonConvert.DeserializeObject<OrderValidation>(message);

                if (string.IsNullOrEmpty(orderValidation?.SupervisorDeviceId) || orderValidation.Products == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

                //foreach (var item in orderValidation.Products)
                //{
                //    bool isExceed = await orderValidationRepository.IsValidationAmountExceed(item.ProductId);

                //    if (isExceed)
                //    {
                //        var notificationModel = new NotificationModel
                //        {
                //            DeviceId = orderValidation.SupervisorDeviceId,
                //            IsAndroiodDevice = true,
                //            Title = "OrderValidation",
                //            Body = $"Chemist Order - {orderValidation.OrderNumber} exceeds the validation amount for product {item.ProductName}-{item.ProductCode}"
                //        };

                //        var notificationResponse = await notificationService.SendNotificationAsync(notificationModel);

                //        if (!notificationResponse.IsSuccess)
                //        {
                //            return false;
                //        }

                //        var notification = new Notification
                //        {
                //            Title = notificationModel.Title,
                //            Body = notificationModel.Body,
                //            ReceiverId = orderValidation.NotificationReceiverId,
                //            TypeIdentifyId = orderValidation.OrderNumber.ToString(),
                //            Type = notificationModel.Title,
                //        };

                //        await notificationRepository.AddNotificationAsync(notification);
                //        await notificationRepository.SaveNotificationAsync();

                //        return true;
                //    }
                //}

                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing message: {ex.Message}");
            return false;
        }
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}