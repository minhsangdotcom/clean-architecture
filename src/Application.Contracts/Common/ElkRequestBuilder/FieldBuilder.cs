using Application.Contracts.Dtos.Requests;

namespace Application.Contracts.Common.ElkRequestBuilder;

public class FieldBuilder(
    FilterBuilder parent,
    string field,
    FieldType fieldType,
    string? path = null
)
{
    private FilterBuilder Add(
        string op,
        string? value = null,
        List<string>? values = null,
        string? from = null,
        string? to = null
    )
    {
        parent.AddFilter(
            new FilterItem
            {
                Field = field,
                Operator = op,
                Value = value,
                Values = values,
                From = from,
                To = to,
                Path = path,
                FieldType = fieldType,
            }
        );

        return parent;
    }

    public FilterBuilder Eq(string value) => Add("=", value);

    public FilterBuilder Neq(string value) => Add("!=", value);

    public FilterBuilder Contains(string value) => Add("contains", value);

    public FilterBuilder Phrase(string value) => Add("phrase", value);

    public FilterBuilder In(params List<string> values) => Add("in", values: values);

    public FilterBuilder Gte(string from) => Add("gte", from: from);

    public FilterBuilder Lte(string to) => Add("lte", to: to);

    public FilterBuilder Gt(string from) => Add("gt", from: from);

    public FilterBuilder Lt(string to) => Add("lt", to: to);

    public FilterBuilder Exists() => Add("exists");

    public FilterBuilder Missing() => Add("missing");
}
