using System.Linq.Expressions;
using Application.Contracts.Dtos.Requests;
using DotNetCoreExtension.Extensions.Expressions;
using DotNetCoreExtension.Extensions.Reflections;

namespace Application.Contracts.Common.ElkRequestBuilder;

public class FilterBuilder
{
    private readonly ElkFilterRequest request = new();

    public static FilterBuilder Create() => new();

    public FieldBuilder Where<T>(string field)
    {
        Type type = PropertyInfoExtensions.GetNestedPropertyInfo(typeof(T), field).PropertyType;
        FieldType t = type switch
        {
            Type x
                when x == typeof(int)
                    || x == typeof(long)
                    || x == typeof(float)
                    || x == typeof(double)
                    || x == typeof(decimal) => FieldType.Number,

            Type x when x == typeof(DateTime) || x == typeof(DateOnly) => FieldType.Date,

            _ => FieldType.Unknown,
        };
        return new FieldBuilder(this, field, t);
    }

    public FieldBuilder Where<T>(Expression<Func<T, object>> expression)
    {
        string field = expression.ToStringProperty();
        Type type = PropertyInfoExtensions.ToPropertyInfo(expression).PropertyType;
        FieldType t = type switch
        {
            Type x
                when x == typeof(int)
                    || x == typeof(long)
                    || x == typeof(float)
                    || x == typeof(double)
                    || x == typeof(decimal) => FieldType.Number,

            Type x when x == typeof(DateTime) || x == typeof(DateOnly) => FieldType.Date,

            _ => FieldType.Unknown,
        };
        return new FieldBuilder(this, field, t);
    }

    public FieldBuilder Where<T>(
        Expression<Func<T, object>> pathExpr,
        Expression<Func<T, object>> fieldExpr
    )
    {
        string path = pathExpr.ToStringProperty();
        string field = fieldExpr.ToStringProperty();
        Type type = PropertyInfoExtensions.ToPropertyInfo(fieldExpr).PropertyType;
        FieldType t = type switch
        {
            Type x
                when x == typeof(int)
                    || x == typeof(long)
                    || x == typeof(float)
                    || x == typeof(double)
                    || x == typeof(decimal) => FieldType.Number,

            Type x when x == typeof(DateTime) || x == typeof(DateOnly) => FieldType.Date,

            _ => FieldType.Unknown,
        };
        return new FieldBuilder(this, field, t, path: path);
    }

    public FilterBuilder GroupBy(string field, int size = 10)
    {
        request.GroupBy.Add(new GroupByItem { Field = field, Size = size });
        return this;
    }

    public FilterBuilder GroupBy<T>(Expression<Func<T, object>> expression, int size = 10)
    {
        string field = expression.ToStringProperty();
        request.GroupBy.Add(new GroupByItem { Field = field, Size = size });
        return this;
    }

    internal void AddFilter(FilterItem item)
    {
        request.Filters.Add(item);
    }

    public ElkFilterRequest Build()
    {
        return request;
    }
}
