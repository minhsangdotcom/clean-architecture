using System.Security.Cryptography;
using System.Text;
using Domain.Common;
using DynamicQuery.Models;
using Newtonsoft.Json;
using SharedKernel.Constants;
using SharedKernel.Models;

namespace Infrastructure.Data.Repositories;

public static class RepositoryExtension
{
    public static string GetSort(this string? sort)
    {
        string defaultSort = sort.GetDefaultSort();
        return $"{defaultSort},{nameof(AuditableEntity.Id)}";
    }

    public static string GetDefaultSort(this string? sort) =>
        string.IsNullOrWhiteSpace(sort)
            ? $"{nameof(AuditableEntity.CreatedAt)}{OrderTerm.DELIMITER}{OrderTerm.DESC}"
            : sort.Trim();

    public static string HashKey(params object?[] parameters)
    {
        StringBuilder text = new();
        foreach (object? param in parameters)
        {
            switch (param)
            {
                case null:
                    continue;
                case string:
                    AppendParameter(text, param.ToString()!);
                    continue;
                default:
                {
                    var result = JsonConvert.SerializeObject(param);
                    AppendParameter(text, result);
                    break;
                }
            }
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString()));
        return Convert.ToHexString(bytes);
    }

    private static StringBuilder AppendParameter(StringBuilder text, string param)
    {
        if (!string.IsNullOrWhiteSpace(text.ToString()))
        {
            text.Append('_');
        }
        text.Append(param);

        return text;
    }

    public static PaginationResponse<T> ToPaginationResponse<T>(this PaginatedResult<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        PageMetadata<T> metadata =
            source.PageMetadata
            ?? throw new ArgumentException("Paging cannot be null.", nameof(source));

        return new PaginationResponse<T>(
            source.Data ?? [],
            new Paging<T>(
                metadata.CurrentPage,
                metadata.PageSize,
                metadata.TotalPage,
                metadata.HasNextPage,
                metadata.HasPreviousPage,
                metadata.Before,
                metadata.After
            )
        );
    }
}
