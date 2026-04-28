using Microsoft.AspNetCore.Mvc;

namespace QueueProducer.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "QueueProducer.Api",
            checkedAtUtc = DateTimeOffset.UtcNow
        });
    }
}
