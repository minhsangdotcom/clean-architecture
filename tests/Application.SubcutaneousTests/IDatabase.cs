using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace Application.SubcutaneousTests;

public interface IDatabase
{
    DbConnection Connection { get; }
    string ConnectionString { get; }
    string EnvironmentVariable { get; }

    IConfiguration GetConfiguration();

    Task InitializeAsync();
    Task ResetAsync();

    Task DisposeAsync();
}
