using Application.Common.Interfaces.Services.Accessors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger,
    ICurrentUser currentUser
) : MessagePreProcessor<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        string requestName = typeof(TMessage).Name;
        Ulid? id = currentUser.Id;

        if (id == Ulid.Empty)
        {
            logger.LogInformation(
                "\n\n Incoming request: {Name} for anonymous user  with payload \n {@Request} \n",
                requestName,
                message
            );
        }
        else
        {
            logger.LogInformation(
                "\n\n Incoming request: {Name} for user {userId} with payload \n {@Request} \n",
                requestName,
                id,
                message
            );
        }
        return default!;
    }
}
