using System.Data.Common;

namespace Application.SubcutaneousTests;

public interface IDatabase
{
    Task InitializeAsync();

    DbConnection GetConnection();

    string GetConnectionString();

    Task ResetAsync();

    Task DisposeAsync();
}
