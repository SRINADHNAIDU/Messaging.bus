using Messaging.bus.Services;
using Microsoft.AspNetCore.Mvc;

namespace Messaging.bus.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly OrderValidationService service;

    public HomeController(OrderValidationService    service)
    {
        this.service = service;
    }
    
    [HttpGet("PublishMessage")]
    public async Task<IActionResult> PublishMessage()
    {

        await service.ValidateOrder();
        return Ok("published successfully.");
    }
}
