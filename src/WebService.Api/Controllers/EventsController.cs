using Microsoft.AspNetCore.Mvc;
using WebService.Domain;
using MediatR;
using WebService.Application.Events.Queries;
using WebService.Application.Events.Commands;

namespace WebService.Api.Controllers;

public class EventsController : WebServiceBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Event>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Event>>> GetEventsAsync()
    {
        return await Mediator.Send(new GetEventList.Query());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Event), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Event>> GetEventDetailAsync(string id)
    {
        return await Mediator.Send(new GetEventDetails.Query { Id = id });
    }

    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> CreateEventAsync(Event @event)
    {
        return await Mediator.Send(new CreateEvent.Command { Event = @event });
    }
}
