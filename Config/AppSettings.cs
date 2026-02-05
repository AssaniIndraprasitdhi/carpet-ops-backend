namespace CarpetOpsSystem.Config;

public class AppSettings
{
    public string SqlServerHost => GetEnv("SQLSERVER_HOST", "localhost");
    public int SqlServerPort => int.Parse(GetEnv("SQLSERVER_PORT", "1433"));
    public string SqlServerDatabase => GetEnv("SQLSERVER_DATABASE", "FabricSource");
    public string SqlServerUser => GetEnv("SQLSERVER_USER", "sa");
    public string SqlServerPassword => GetEnv("SQLSERVER_PASSWORD", "");

    public string SqlServerConnectionString =>
        $"Server={SqlServerHost},{SqlServerPort};Database={SqlServerDatabase};User Id={SqlServerUser};Password={SqlServerPassword};TrustServerCertificate=True;";

    public string PostgresHost => GetEnv("POSTGRES_HOST", "localhost");
    public int PostgresPort => int.Parse(GetEnv("POSTGRES_PORT", "5432"));
    public string PostgresDatabase => GetEnv("POSTGRES_DATABASE", "fabric_ops");
    public string PostgresUser => GetEnv("POSTGRES_USER", "postgres");
    public string PostgresPassword => GetEnv("POSTGRES_PASSWORD", "");

    public string PostgresConnectionString =>
        $"Host={PostgresHost};Port={PostgresPort};Database={PostgresDatabase};Username={PostgresUser};Password={PostgresPassword};";

    public decimal OuterSpacing => decimal.Parse(GetEnv("LAYOUT_OUTER_SPACING", "0.3"));
    public decimal InnerSpacing => decimal.Parse(GetEnv("LAYOUT_INNER_SPACING", "0.15"));

    public string Environment => GetEnv("APP_ENVIRONMENT", "Development");
    public int Port => int.Parse(GetEnv("APP_PORT", "5000"));

    private static string GetEnv(string key, string defaultValue)
    {
        return System.Environment.GetEnvironmentVariable(key) ?? defaultValue;
    }
}
