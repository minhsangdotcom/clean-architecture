using SharedKernel.Entities;

namespace Domain.Aggregates.Regions;

public class Region : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string EnglishFullName { get; set; } = string.Empty;

    public string? CustomName { get; set; }
}
