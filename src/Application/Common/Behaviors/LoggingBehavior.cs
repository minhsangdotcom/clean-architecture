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

        const string replacePhrase = "for user {userId}";
        string loggingMessage =
            "\n\n Incoming request: {Name} " + replacePhrase + " with payload \n {@Request} \n";

        List<object?> parameters = [requestName, id, message];
        if (id == null)
        {
            loggingMessage = loggingMessage.Replace(
                $"{replacePhrase}",
                "for anonymous",
                StringComparison.Ordinal
            );
            parameters.RemoveAt(1);
        }

        logger.LogInformation(loggingMessage, [.. parameters]);
        return default!;
    }
}
