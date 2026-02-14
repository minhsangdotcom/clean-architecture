namespace Application.SubcutaneousTests;

public interface IDatabase
{
    string ConnectionString { get; }
    Task InitializeAsync();
    Task ResetAsync();
    Task DisposeAsync();
}
