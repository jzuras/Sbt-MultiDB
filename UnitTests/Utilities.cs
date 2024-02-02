using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SbtMultiDB.Data;

namespace UnitTests;

internal class Utilities
{
    private static IConfiguration Configuration { get; }
    private static string DataFileDirectory { get; } = "Test Data Files";

    static Utilities()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
        .Build();
    }

    #region Context Creation Methods
    public static SqlServerContext CreateSqlServerContext(string connectionStringKey)
    {
        string connectionString = GetConnectionString(connectionStringKey);

        var optionsBuilder = new DbContextOptionsBuilder<SqlServerContext>()
            .UseSqlServer(connectionString);

        return new SqlServerContext(optionsBuilder.Options);
    }

    public static CosmosContext CreateCosmosContext(string connectionStringKey)
    {
        string connectionString = GetConnectionString(connectionStringKey);

        var optionsBuilder = new DbContextOptionsBuilder<CosmosContext>()
            .UseCosmos(connectionString, databaseName: "Sbt-EF");

        return new CosmosContext(optionsBuilder.Options);
    }

    public static MySqlContext CreateMySqlContext(string connectionStringKey)
    {
        string connectionString = GetConnectionString(connectionStringKey);

        var optionsBuilder = new DbContextOptionsBuilder<MySqlContext>()
            .UseMySQL(connectionString);

        return new MySqlContext(optionsBuilder.Options);
    }

    public static PostgreSqlContext CreatePostgreSqlContext(string connectionStringKey)
    {
        string connectionString = GetConnectionString(connectionStringKey);

        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlContext>()
            .UseNpgsql(connectionString);

        return new PostgreSqlContext(optionsBuilder.Options);
    }
    #endregion

    #region Helper Methods
    private static string GetConnectionString(string connectionStringKey)
    {
        return Configuration.GetConnectionString(connectionStringKey) ??
            throw new InvalidOperationException($"Connection string '{connectionStringKey}' not found.");
    }

    public static MemoryStream GetMemoryStreamForDataFile(string dataFilename)
    {
        var currentDirectory = Environment.CurrentDirectory;
        var relativeFilePath = Path.Combine(DataFileDirectory, dataFilename);
        string fullPath = Path.Combine(currentDirectory, relativeFilePath);

        var fileContent = File.ReadAllBytes(fullPath);
     
        return new MemoryStream(fileContent);
    }
    #endregion
}
