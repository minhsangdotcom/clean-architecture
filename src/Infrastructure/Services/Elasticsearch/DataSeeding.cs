using Domain.Aggregates.AuditLogs;
using Elastic.Clients.Elasticsearch;

namespace Infrastructure.Services.Elasticsearch;

public static class DataSeeding
{
    /// <summary>
    /// fake data for test
    /// </summary>
    /// <param name="elasticsearchClient"></param>
    /// <returns></returns>
    public static async Task SeedingAsync(
        this ElasticsearchClient elasticsearchClient,
        string prefix
    )
    {
        var auditLog = await elasticsearchClient.CountAsync<AuditLog>();
        if (auditLog.Count > 0)
        {
            return;
        }

        List<AuditLog> logs =
        [
            new AuditLog
            {
                Id = "01KC3GC0F03F2FZAQMAX9BS43H",
                Entity = "Customer",
                Type = 3,
                OldValue = new { Phone = "0123456789" },
                NewValue = new { Phone = "0987654321" },
                ActionPerformBy = "admin-superuser",
                Agent = new Agent
                {
                    FirstName = "David",
                    LastName = "Tran",
                    Email = "david.tran@example.com",
                    DayOfBirth = new DateTime(1988, 12, 1),
                    Gender = 1,
                },
            },
            new AuditLog
            {
                Id = Ulid.NewUlid().ToString(),
                Entity = "Order",
                Type = 2,
                OldValue = new { Status = "Pending" },
                NewValue = new { Status = "Completed" },
                ActionPerformBy = "system-job",
                Agent = new Agent
                {
                    FirstName = "Sarah",
                    LastName = "Nguyen",
                    Email = "sarah.nguyen@example.com",
                    DayOfBirth = new DateTime(1995, 5, 10),
                    Gender = 2,
                },
            },
            new AuditLog
            {
                Id = Ulid.NewUlid().ToString(),
                Entity = "Product",
                Type = 1,
                OldValue = new { Price = 999, Name = "iPhone 13" },
                NewValue = new { Price = 899, Name = "iPhone 13" },
                ActionPerformBy = "admin-system",
                Agent = new Agent
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    DayOfBirth = new DateTime(1990, 1, 5),
                    Gender = 1,
                },
            },
        ];

        var response = await elasticsearchClient.IndexManyAsync(
            logs,
            ElkIndexExtension.GetName<AuditLog>(prefix)
        );

        if (response.IsSuccess())
        {
            Console.WriteLine("Elasticsearch has seeded.");
        }
        else
        {
            Console.WriteLine(
                $"Elasticsearch has been failed in seeding with {response.DebugInformation}"
            );
        }
    }
}
