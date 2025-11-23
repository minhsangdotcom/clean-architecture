using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Converters;

public class UlidToStringConverter(ConverterMappingHints mappingHints = null!)
    : ValueConverter<Ulid, string>(
        convertToProviderExpression: x => x.ToString(),
        convertFromProviderExpression: x => Ulid.Parse(x),
        mappingHints: defaultHints.With(mappingHints)
    )
{
    private static readonly ConverterMappingHints defaultHints = new(size: 26);

    public UlidToStringConverter()
        : this(null!) { }
}
