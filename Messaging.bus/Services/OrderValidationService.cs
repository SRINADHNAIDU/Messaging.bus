using Messaging.bus.Models;
using Messaging.bus.MQ;

namespace Messaging.bus.Services;

public class OrderValidationService
{
    private readonly IRabbitMQPublisher<OrderValidation> rabbitMQPublisher;

    public OrderValidationService(IRabbitMQPublisher<OrderValidation> rabbitMQPublisher)
    {
        this.rabbitMQPublisher = rabbitMQPublisher;
    }

    public async Task<bool> ValidateOrder()
    {
        // Prepare validation data

        //var orderValidation = await _chemistService.GetOrderValidationInfoByOrderEntry(order.OrderEntryBy, order, orderDetailsList);

        // publish order validation data
        var orderValidation = new OrderValidation
        {
            NotificationReceiverId = Guid.NewGuid(),
            OrderEntryByCode = "983265432"
        };
        await rabbitMQPublisher.PublishMessageAsync(orderValidation, RabbitMQQueues.OrderValidationQueue);
        return true;
    }
}
