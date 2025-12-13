namespace Infrastructure.Services.Queue;

public class QueueSettings
{
    public string OriginQueueName { get; set; } = "queue:the_queue";
    public int MaxRetryAttempts { get; set; } = 10;
    public int MaximumDelayInSec { get; set; } = 90;
}
