using System.Reflection;
using CaseConverter;
using DotNetCoreExtension.Extensions.Reflections;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FluentConfiguration.Configurations;
using Infrastructure.Services.Elasticsearch;
using SharedKernel.Constants;
using SharedKernel.Models;

namespace Infrastructure.Services.Elasticsearch;

public static class ElasticFunctionalityHelper
{
    public static SearchRequestDescriptor<T> OrderBy<T>(
        this SearchRequestDescriptor<T> sortQuery,
        string Sort
    )
        where T : class
    {
        var results = new List<SortOptions>();
        string[] sorts = Sort.Split(',', StringSplitOptions.TrimEntries) ?? [];
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
                // A.ARAW
                results.Add(
                    SortOptions.Field(
                        property.ToCamelCase()!,
                        new FieldSort() { Order = sortOrder }
                    )
                );
            }
            else
            {
                //A.B.C.CRAW
                //PATH: A
                //PATH: A.B
                List<string> nestedArray = [.. property.Split('.')];
                var nestedSort = new NestedSortValue();
                string name = string.Empty;
                for (int j = 0; j < nestedArray.Count - (isStringPropertyType ? 2 : 1); j++)
                {
                    name += j == 0 ? nestedArray[j] : $".{nestedArray[j]}";
                    nestedSort = nestedSort.Nested = new NestedSortValue() { Path = name! };
                }
                var sortOptions = SortOptions.Field(
                    property!,
                    new FieldSort() { Order = sortOrder, Nested = nestedSort }
                );
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
                Fields = Fields.FromFields([.. properties.Select(x => new Field(x.Value))]),
                //Fuzziness = new Fuzziness(2),
            };
        List<Query> queries = [multiMatchQuery];

        //* search nested properties
        //todo: [{"A" ,["A.A1","A.A2"]}, {"A.B", ["A.B.B1","A.B.B2"]}]
        IEnumerable<KeyValuePair<string, string>> nestedProperties = stringProperties
            .Except(properties)
            .Select(x =>
            {
                string value = x.Value;
                int lastDot = value.LastIndexOf('.');
                return new KeyValuePair<string, string>(value[..lastDot], value);
            });

        // * group and sort with the deeper and deeper of nesting
        var nestedsearch = nestedProperties
            .GroupBy(x => x.Key)
            .Select(x => new { key = x.Key, primaryProperty = x.Select(p => p.Value).ToList() })
            .OrderBy(x => x.key)
            .ToList();

        //* create nested multi_match search
        foreach (var nested in nestedsearch)
        {
            var key = nested.key;
            var parts = key.Trim().Split(".");
            NestedQuery nestedQuery = new();

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
                    NestedQuery nest = new() { Path = path! };
                    nestedQuery.Query = nest;
                    nestedQuery = nest;
                }
            }
            nestedQuery.Query = new MultiMatchQuery()
            {
                Query = $"{keyword}",
                Fields = nested.primaryProperty.Select(x => new Field(x)).ToArray(),
                //Fuzziness = new Fuzziness(2),
            };
            queries.Add(nestedQuery);
        }

        return queries;
    }

    static List<KeyValuePair<PropertyType, string>> StringProperties(
        Type type,
        int deep = 1,
        string? parrentName = null,
        PropertyType? propertyType = null
    )
    {
        parrentName = parrentName?.Trim().ToCamelCase();
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
                        parrentName != null ? $"{parrentName}.{propertyName}" : propertyName
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
            string currentName =
                parrentName != null ? $"{parrentName}.{propertyName}" : propertyName;

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

    internal record SortItemResult(string PropertyName, PropertyInfo PropertyInfo, string Order);
}
