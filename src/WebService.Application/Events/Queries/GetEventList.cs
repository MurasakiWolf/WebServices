using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebService.Domain;
using WebService.Persistence;

namespace WebService.Application.Events.Queries;

public class GetEventList
{
    public class Query : IRequest<List<Event>> { }

    public class Handler(AppDbContext context, ILogger<GetEventList> logger) : IRequestHandler<Query, List<Event>>
    {
        public async Task<List<Event>> Handle(Query request, CancellationToken cancellationToken)
        {
            //Example used to prove the use of the Cancellation Token
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(0, cancellationToken);
                    logger.LogInformation($"Task {i} was COMPLETED!");
                }

            }
            catch (Exception ex)
            {
                logger.LogInformation("Task was CANCELLED!");

            }

            return await context.Events.ToListAsync(cancellationToken);
        }
    }
}