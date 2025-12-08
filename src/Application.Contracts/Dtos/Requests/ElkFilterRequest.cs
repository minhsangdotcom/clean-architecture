namespace Application.Contracts.Dtos.Requests;

public class ElkFilterRequest
{
    public List<FilterItem> Filters { get; set; } = [];
    public List<GroupByItem> GroupBy { get; set; } = [];
}

public class FilterItem
{
    public string? Path { get; set; }
    public string Field { get; set; } = default!;
    public FieldType FieldType { get; set; } = FieldType.Unknown;
    public string Operator { get; set; } = "=";
    public string? Value { get; set; }
    public List<string>? Values { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
}

public enum FieldType
{
    Unknown,
    Number,
    Date,
}

public enum RangeOperator
{
    Gte,
    Lte,
    Gt,
    Lt,
}

public class GroupByItem
{
    public string Field { get; set; } = default!;
    public int Size { get; set; } = 10;
}
