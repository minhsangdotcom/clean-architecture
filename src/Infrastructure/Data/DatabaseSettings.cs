namespace Infrastructure.Data;

public class DatabaseSettings
{
    public CurrentProvider Provider { get; set; }
    public RelationalProvider? Relational { get; set; }
    public NonRelationalProvider? NonRelational { get; set; }
}

public enum CurrentProvider
{
    PostgreSQL = 1,
    MySQL = 2,
    SQLServer = 3,
    Oracle = 4,
    MongoDB = 5,
}

public class RelationalProvider
{
    public RelationalProviderSetting? PostgreSQL { get; set; }
}

public class RelationalProviderSetting
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class NonRelationalProvider
{
    public NonRelationalProviderSetting? MongoDB { get; set; }
}

public class NonRelationalProviderSetting
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
