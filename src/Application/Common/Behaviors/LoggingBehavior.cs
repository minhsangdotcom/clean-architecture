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
        string commandName = typeof(TMessage).Name;
        Ulid? id = currentUser.Id;

        if (id == Ulid.Empty)
        {
            logger.LogInformation(
                "\n\n Incoming command {CommandName} from unauthenticated user \n",
                commandName
            );
        }
        else
        {
            logger.LogInformation(
                "\n\n Incoming command {CommandName} from user {id} \n",
                commandName,
                id
            );
        }
        return default!;
    }
}
