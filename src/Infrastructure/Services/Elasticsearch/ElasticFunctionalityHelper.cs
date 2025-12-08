using System.Reflection;
using Application.Contracts.Dtos.Requests;
using CaseConverter;
using DotNetCoreExtension.Extensions.Reflections;
using DynamicQuery.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FluentConfiguration.Configurations;
using Infrastructure.Services.Elasticsearch;
using SharedKernel.Constants;

namespace Infrastructure.Services.Elasticsearch;

public static class ElasticFunctionalityHelper
{
    public static SearchRequestDescriptor<T> OrderBy<T>(
        this SearchRequestDescriptor<T> sortQuery,
        string sort
    )
        where T : class
    {
        List<SortOptions> results = [];
        if (string.IsNullOrWhiteSpace(sort))
        {
            return new();
        }

        string[] sorts = sort.Split(',', StringSplitOptions.TrimEntries) ?? [];
        List<SortItemResult> sortItems = GetSortItems<T>(sorts);

        foreach (SortItemResult sortItem in sortItems)
        {
            string property = sortItem.PropertyName;
            string order = sortItem.Order;

            SortOrder sortOrder = order == OrderTerm.DESC ? SortOrder.Desc : SortOrder.Asc;
            bool isNestedSort = property.Contains('.');
            Type propertyType = sortItem.PropertyInfo.PropertyType;

            bool isStringPropertyType =
                propertyType == typeof(string)
                && !propertyType.IsUserDefineType()
                && !propertyType.IsArrayGenericType();

            if (isStringPropertyType)
            {
                property += $".{ElsIndexExtension.GetKeywordName<T>(property)}";
            }

            if (!isNestedSort)
            {
                var sortOptions = new SortOptions()
                {
                    Field = new FieldSort() { Field = property.ToCamelCase(), Order = sortOrder },
                };
                // A.ARAW
                results.Add(sortOptions);
            }
            else
            {
                //A.B.C.CRAW
                //PATH: A
                //PATH: A.B
                List<string> nestedArray = [.. property.Split('.')];
                NestedSortValue nestedSort = default!;
                string name = string.Empty;
                for (int j = 0; j < nestedArray.Count - (isStringPropertyType ? 2 : 1); j++)
                {
                    name += j == 0 ? nestedArray[j] : $".{nestedArray[j]}";
                    nestedSort = nestedSort.Nested = new NestedSortValue() { Path = name! };
                }
                SortOptions sortOptions =
                    new()
                    {
                        Field = new FieldSort()
                        {
                            Field = property.ToCamelCase(),
                            Order = sortOrder,
                            Nested = nestedSort,
                        },
                    };
                results.Add(sortOptions);
            }
        }

        return sortQuery.Sort(results);
    }

    public static QueryDescriptor<T> Search<T>(
        this QueryDescriptor<T> search,
        string keyword,
        int deep = 1,
        IEnumerable<string>? searchProperties = null
    )
    {
        List<Query> queries = [];
        List<KeyValuePair<PropertyType, string>> stringProperties =
            searchProperties?.Any() == true
                ? GivenStringProperties(typeof(T), searchProperties)
                : StringProperties(typeof(T), deep);
        queries.AddRange(MultiMatchQuery(stringProperties, keyword));

        return search.Bool(b => b.Should(queries));
    }

    public static QueryDescriptor<T> BuildFilterItemQuery<T>(QueryDescriptor<T> q, FilterItem f)
        where T : class
    {
        // Nested path support
        if (!string.IsNullOrEmpty(f.Path))
        {
            return q.Nested(n => n.Path(f.Path).Query(nq => BuildSimpleQuery(nq, f)));
        }

        // Normal field
        return BuildSimpleQuery(q, f);
    }

    private static QueryDescriptor<T> BuildSimpleQuery<T>(QueryDescriptor<T> q, FilterItem f)
        where T : class
    {
        return f.Operator switch
        {
            "=" => q.Term(t => t.Field(f.Field).Value(f.Value!)),
            "!=" => q.Bool(b => b.MustNot(n => n.Term(t => t.Field(f.Field).Value(f.Value!)))),
            "contains" => q.Match(m => m.Field(f.Field).Query(f.Value!)),
            "phrase" => q.MatchPhrase(m => m.Field(f.Field).Query(f.Value!)),
            "in" => q.Terms(t =>
                t.Field(f.Field!)
                    .Terms(new TermsQueryField(f.Values!.ConvertAll(value => (FieldValue)value)))
            ),
            "gte" => BuildRange(q, f, RangeOperator.Gte),
            "lte" => BuildRange(q, f, RangeOperator.Lte),
            "gt" => BuildRange(q, f, RangeOperator.Gt),
            "lt" => BuildRange(q, f, RangeOperator.Lt),
            "exists" => q.Exists(e => e.Field(f.Field)),
            "missing" => q.Bool(b => b.MustNot(n => n.Exists(e => e.Field(f.Field!)))),
            _ => q.MatchAll(new MatchAllQuery()),
        };
    }

    private static QueryDescriptor<T> BuildRange<T>(
        QueryDescriptor<T> q,
        FilterItem f,
        RangeOperator op
    )
        where T : class
    {
        return f.FieldType switch
        {
            FieldType.Number => BuildNumberRange(q, f, op),
            FieldType.Date => BuildDateRange(q, f, op),
            _ =>
                q.MatchAll() // wrong type → ignore
            ,
        };
    }

    private static QueryDescriptor<T> BuildNumberRange<T>(
        QueryDescriptor<T> q,
        FilterItem f,
        RangeOperator op
    )
        where T : class
    {
        double? from = TryDouble(f.From);
        double? to = TryDouble(f.To);

        return q.Range(r =>
            r.Number(x =>
            {
                var a = op switch
                {
                    RangeOperator.Gte => x.Gte(from).Field(f.Field),
                    RangeOperator.Lte => x.Lte(to).Field(f.Field),
                    RangeOperator.Gt => x.Gt(from).Field(f.Field),
                    RangeOperator.Lt => x.Lt(to).Field(f.Field),
                    _ => x,
                };
            })
        );
    }

    private static QueryDescriptor<T> BuildDateRange<T>(
        QueryDescriptor<T> q,
        FilterItem f,
        RangeOperator op
    )
        where T : class
    {
        DateTime? from = TryDate(f.From);
        DateTime? to = TryDate(f.To);

        return q.Range(r =>
            r.Date(x =>
            {
                var a = op switch
                {
                    _ => op switch
                    {
                        RangeOperator.Gte => x.Gte(from).Field(f.Field),
                        RangeOperator.Lte => x.Lte(to).Field(f.Field),
                        RangeOperator.Gt => x.Gt(from).Field(f.Field),
                        RangeOperator.Lt => x.Lt(to).Field(f.Field),
                        _ => x,
                    },
                };
            })
        );
    }

    private static List<Query> MultiMatchQuery(
        List<KeyValuePair<PropertyType, string>> stringProperties,
        string keyword
    )
    {
        //*search for the same level property
        List<KeyValuePair<PropertyType, string>> properties = stringProperties.FindAll(x =>
            x.Key == PropertyType.Property
        );
        MultiMatchQuery multiMatchQuery =
            new()
            {
                Query = $"{keyword}",
                Fields = Fields.FromFields([.. properties.ConvertAll(x => new Field(x.Value))]),
                //Fuzziness = new Fuzziness(2),
            };
        List<Query> queries = [multiMatchQuery];

        // //* search nested properties
        // //todo: [{"A" ,["A.A1","A.A2"]}, {"A.B", ["A.B.B1","A.B.B2"]}]
        List<KeyValuePair<string, string>> nestedProperties =
        [
            .. stringProperties
                .Except(properties)
                .Select(x =>
                {
                    string value = x.Value;
                    int lastDot = value.LastIndexOf('.');
                    return new KeyValuePair<string, string>(value[..lastDot], value);
                }),
        ];

        // * group and sort with the deeper and deeper of nesting
        var nestedSearch = nestedProperties
            .GroupBy(x => x.Key)
            .Select(x => new { key = x.Key, primaryProperty = x.Select(p => p.Value).ToList() })
            .OrderBy(x => x.key)
            .ToList();

        //* create nested multi_match search
        foreach (var nested in nestedSearch)
        {
            string key = nested.key;
            string[] parts = key.Trim().Split(".");
            NestedQuery nestedQuery = new(string.Empty, new Query());

            string path = string.Empty;
            for (int i = 0; i < parts.Length; i++)
            {
                if (i == 0)
                {
                    path += $"{parts[i]}";
                    nestedQuery.Path = path!;
                }
                else
                {
                    path += $".{parts[i]}";
                    NestedQuery nest = new(path, new Query());
                    nestedQuery.Query = nest;
                    nestedQuery = nest;
                }
            }
            nestedQuery.Query = new MultiMatchQuery()
            {
                Query = $"{keyword}",
                Fields = nested.primaryProperty.ConvertAll(x => new Field(x)).ToArray(),
                //Fuzziness = new Fuzziness(2),
            };
            queries.Add(nestedQuery);
        }

        return queries;
    }

    static List<KeyValuePair<PropertyType, string>> StringProperties(
        Type type,
        int deep = 1,
        string? parentName = null,
        PropertyType? propertyType = null
    )
    {
        parentName = parentName?.Trim().ToCamelCase();
        if (deep < 0)
        {
            return [];
        }

        IEnumerable<PropertyInfo> properties = type.GetProperties();

        List<KeyValuePair<PropertyType, string>> stringProperties =
        [
            .. properties
                .Where(x => x.PropertyType == typeof(string))
                .Select(x =>
                {
                    string propertyName = x.Name.ToCamelCase();
                    return new KeyValuePair<PropertyType, string>(
                        propertyType ?? PropertyType.Property,
                        parentName != null ? $"{parentName}.{propertyName}" : propertyName
                    );
                }),
        ];

        List<PropertyInfo> collectionObjectProperties =
        [
            .. properties.Where(x =>
                (x.IsUserDefineType() || x.IsArrayGenericType()) && x.PropertyType != typeof(string)
            ),
        ];

        foreach (var propertyInfo in collectionObjectProperties)
        {
            string propertyName = propertyInfo.Name.ToCamelCase();
            string currentName = parentName != null ? $"{parentName}.{propertyName}" : propertyName;

            if (propertyInfo.IsArrayGenericType())
            {
                Type genericType = propertyInfo.PropertyType.GetGenericArguments()[0];
                stringProperties.AddRange(
                    StringProperties(genericType, deep - 1, currentName, PropertyType.Array)
                );
            }
            else if (propertyInfo.IsUserDefineType())
            {
                stringProperties.AddRange(
                    StringProperties(
                        propertyInfo.PropertyType,
                        deep - 1,
                        currentName,
                        PropertyType.Object
                    )
                );
            }
        }

        return stringProperties;
    }

    static List<KeyValuePair<PropertyType, string>> GivenStringProperties(
        Type type,
        IEnumerable<string> properties
    )
    {
        List<KeyValuePair<PropertyType, string>> result = [];
        foreach (string propertyPath in properties)
        {
            string propertyName = propertyPath.Trim('.');
            if (type.GetNestedPropertyInfo(propertyName).PropertyType != typeof(string))
            {
                continue;
            }

            if (!propertyPath.Contains('.'))
            {
                result.Add(new(PropertyType.Property, propertyName));
                continue;
            }

            string parentPropertyName = propertyName[..propertyName.LastIndexOf('.')];
            PropertyInfo propertyInfo = type.GetNestedPropertyInfo(parentPropertyName);

            if (propertyInfo.IsArrayGenericType())
            {
                result.Add(new(PropertyType.Array, propertyName));
                continue;
            }

            if (propertyInfo.IsUserDefineType())
            {
                result.Add(new(PropertyType.Object, propertyName));
            }
        }

        return result;
    }

    private static List<SortItemResult> GetSortItems<T>(string[] sortItems)
        where T : class =>
        [
            .. sortItems.Select(sortItem =>
            {
                string[] items = sortItem.Split(OrderTerm.DELIMITER);
                string propertyName = items[0];
                PropertyInfo propertyInfo = typeof(T).GetNestedPropertyInfo(propertyName);

                if (items.Length == 1)
                {
                    return new SortItemResult(propertyName, propertyInfo, OrderTerm.ASC);
                }

                return new SortItemResult(propertyName, propertyInfo, items[1].Trim());
            }),
        ];

    private static double? TryDouble(string? input)
    {
        if (double.TryParse(input, out var d))
        {
            return d;
        }
        return null;
    }

    private static DateTime? TryDate(string? input)
    {
        if (DateTime.TryParse(input, out var dt))
        {
            return dt;
        }
        return null;
    }

    internal record SortItemResult(string PropertyName, PropertyInfo PropertyInfo, string Order);
}
