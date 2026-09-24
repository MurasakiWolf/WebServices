using Microsoft.AspNetCore.Mvc;
using WebService.Domain;
using MediatR;
using WebService.Application.Events.Queries;

namespace WebService.Api.Controllers;

public class EventsController(IMediator mediator) : BaseWebServiceController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Event>>> GetEventsAsync()
    {
        return await mediator.Send(new GetEventList.Query());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetEventDetailAsync(string id)
    {
        return await mediator.Send(new GetEventDetails.Query { Id = id });
    }
}
