using Specification;
using Specification.Builders;

namespace Domain.Aggregates.QueueLogs.Specifications;

public class ListQueueResponseSpecification : Specification<QueueLog>
{
    public ListQueueResponseSpecification()
    {
        Query.AsNoTracking();
    }
}
