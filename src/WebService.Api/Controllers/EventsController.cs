using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebService.Persistence;
using WebService.Domain;

namespace WebService.Api.Controllers;

public class EventsController(AppDbContext context) : BaseWebServiceController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Event>>> GetEventsAsync()
    {
        return await context.Events.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetEventDetail(string id)
    {
        var result = await context.Events.FindAsync(id);

        if (result == null) return NotFound("The event was not found");

        return result;
    }
}
