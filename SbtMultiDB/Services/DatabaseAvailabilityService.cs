using Microsoft.Extensions.Options;
using SbtMultiDB.Data;
using SbtMultiDB.Shared;

namespace SbtMultiDB.Services;

/// <summary>
/// This singleton checks once, at startup, to determine which databases are
/// available. Also checked is the Setting for each database, which may
/// disallow a database for use. (Doing so will mark the DB as unavailable to the user.)
/// </summary>
public class DatabaseAvailabilityService
{
    public bool IsCosmosAvailable { get; init; } = false;
    public bool IsSqlServerAvailable { get; init; } = false;
    public bool IsMySqlAvailable { get; init; } = false;
    public bool IsPostgreSqlAvailable { get; init; } = false;

    public DatabaseAvailabilityService(IServiceProvider serviceProvider, 
        ILogger<DatabaseAvailabilityService> logger, IOptions<Settings> options)
    {
        logger.LogInformationExt("DatabaseAvailabilityService called.");
        AllowedDatabases allowed = options.Value.AllowedDatabases;

        // Each DB is assumed to be unavailable until otherwise determined.
        // If the Settings indicate that the DB is allowed, the success
        // of the EnsureCreated() call will determine availability for it.
        using (var scope = serviceProvider.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                if (allowed.IsCosmosAllowed == "true")
                {
                    var context = services.GetRequiredService<CosmosContext>();
                    context.Database.EnsureCreated();
                    this.IsCosmosAvailable = true;
                }
            }
            catch (Exception ex) 
            {
                logger.LogErrorExt(ex, "EnsureCreated() failed for Cosmos DB.");
            }

            try
            {
                if (allowed.IsSqlServerAllowed == "true")
                {
                    var context = services.GetRequiredService<SqlServerContext>();
                    context.Database.EnsureCreated();
                    this.IsSqlServerAvailable = true;
                }
            }
            catch (Exception ex)
            {
                logger.LogErrorExt(ex, "EnsureCreated() failed for Sql Server.");
            }

            try
            {
                if (allowed.IsMySqlAllowed == "true")
                {
                    var context = services.GetRequiredService<MySqlContext>();
                    context.Database.EnsureCreated();
                    this.IsMySqlAvailable = true;
                }
            }
            catch (Exception ex)
            {
                logger.LogErrorExt(ex, "EnsureCreated() failed for MySql.");
            }

            try
            {
                if (allowed.IsPostgreSqlAllowed == "true")
                {
                    var context = services.GetRequiredService<PostgreSqlContext>();
                    context.Database.EnsureCreated();
                    this.IsPostgreSqlAvailable = true;
                }
            }
            catch (Exception ex)
            {
                logger.LogErrorExt(ex, "EnsureCreated() failed for PostgreSql.");
            }

            // Log the information if all are available, or log a warning if one or more
            // are not available (possibly due to not being allowed).
            if (this.IsSqlServerAvailable && this.IsCosmosAvailable && this.IsPostgreSqlAvailable && this.IsMySqlAvailable)
            {
                logger.LogInformationExt($"DB availability: SqlServer: {IsSqlServerAvailable} Cosmos: {IsCosmosAvailable} PostgreSql: {IsPostgreSqlAvailable} MySql: {IsMySqlAvailable}.");
            }
            else
            {
                logger.LogWarningExt($"Allowed Databases: SqlServer: {allowed.IsSqlServerAllowed} Cosmos: {allowed.IsCosmosAllowed} PostgreSql: {allowed.IsPostgreSqlAllowed} MySql: {allowed.IsMySqlAllowed}.");
                logger.LogWarningExt($"DB availability: SqlServer: {IsSqlServerAvailable} Cosmos: {IsCosmosAvailable} PostgreSql: {IsPostgreSqlAvailable} MySql: {IsMySqlAvailable}.");
            }
        }
    }
}
