namespace Messaging.bus.Models;


public class OrderValidation
{
    public Guid? OrderEntryById { get; set; }
    public string? OrderEntryByCode { get; set; }
    public List<ProductValidation>? Products { get; set; }
    public string? SupervisorDeviceId { get; set; }
    public long OrderNumber { get; set; }
    public Guid NotificationReceiverId { get; set; }
}

public class ProductValidation
{
    public Guid? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public decimal? OrderAmount { get; set; }
    public string? ProductName { get; set; }
}
