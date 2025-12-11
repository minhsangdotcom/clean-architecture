namespace Infrastructure.Services.Queue;

public class QueueSettings
{
    public string OriginQueueName { get; set; } = "the_queue";
    public int MaxRetryAttempts { get; set; }
    public int MaximumDelayInSec { get; set; } = 90;
}
