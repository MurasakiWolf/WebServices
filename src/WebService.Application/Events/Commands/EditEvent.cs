using AutoMapper;
using MediatR;
using WebService.Domain;
using WebService.Persistence;

namespace WebService.Application.Events.Commands;

public class EditEvent
{
    public class Command : IRequest
    {
        public required Event Event { get; set; }
        public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command>
        {
            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var @event = await context.Events.FindAsync([request.Event.Id], cancellationToken) ?? throw new Exception("Event not found");
                mapper.Map(request.Event, @event);

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

}

