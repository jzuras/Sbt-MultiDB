using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using SbtMultiDB.Models;
using System.Net;

namespace SbtMultiDB.Data.Repositories;

public class DivisionRepositoryFactory : IDivisionRepositoryFactory
{
    private readonly IServiceProvider ServiceProvider;

    public DivisionRepositoryFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    public IDivisionRepository CreateRepository(IDivisionRepository.RepositoryType repositoryType)
    {
        // Use the DI container to resolve the appropriate repository
        switch (repositoryType)
        {
            case IDivisionRepository.RepositoryType.SqlServer:
                return this.ServiceProvider.GetRequiredService<SqlServerRepository>();

            case IDivisionRepository.RepositoryType.MySql:
                return this.ServiceProvider.GetRequiredService<MySqlRepository>();

            case IDivisionRepository.RepositoryType.PostgreSql:
                return this.ServiceProvider.GetRequiredService<PostgreSqlRepository>();

            default:
            case IDivisionRepository.RepositoryType.Cosmos:
                return this.ServiceProvider.GetRequiredService<CosmosRepository>();
        }
    }
}

public class MySqlRepository : DivisionEfCoreRepository
{
    public MySqlRepository(MySqlContext context) : base(context)
    {

    }
}

public class PostgreSqlRepository : DivisionEfCoreRepository
{
    public PostgreSqlRepository(PostgreSqlContext context) : base(context)
    {

    }
}

public class SqlServerRepository : DivisionEfCoreRepository
{
    public SqlServerRepository(SqlServerContext context) : base(context)
    {

    }
}

public class CosmosRepository : DivisionEfCoreRepository
{
    public CosmosRepository(CosmosContext context) : base(context)
    {

    }
}

public class DivisionEfCoreRepository : IDivisionRepository
{
    protected DivisionContext DbContext { get; init; } = default!;

    public DivisionEfCoreRepository(DivisionContext context)
    {
        this.DbContext = context;
    }

    /// <inheritdoc/>
    public async Task<bool> DivisionExists(string organization, string abbreviation)
    {
        var division = await this.GetDivision(organization, abbreviation);

        return (division != null);
    }

    /// <inheritdoc/>
    public async Task<Division> GetDivision(string organization, string abbreviation)
    {
        try
        {
            var division = await this.DbContext.Divisions
                .Include(d => d.Schedule)
                .Include(d => d.Standings)
                .Where(d => d.Organization.ToLower() == organization.ToLower()
                    && d.Abbreviation.ToLower() == abbreviation.ToLower())
                .FirstOrDefaultAsync();

            return division!;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Division>> GetDivisionList(string organization)
    {
        try
        {
            var division = await this.DbContext.Divisions
                .Where(d => d.Organization.ToLower() == organization.ToLower()).ToListAsync();

            if (division != null)
                return division;
            else
                return new List<Division>();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // No divisions - return empty list
            return new List<Division>();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Schedule>> GetGames(string organization, string abbreviation, int gameID)
    {
        var list = new List<Schedule>();

        try
        {
            // Step 1: do a query returning 1 game based on the game id.
            // Step 2: do a second query using that game's day and field.

            // var schedule = await this.Divisions
            //    .Where(d => d.Organization == organization && d.ID == divisionID.ToLower())
            //    .SelectMany(d => d.Schedule)
            //    .Where(s => s.GameID == gameID)
            //    .FirstOrDefaultAsync();

            // EF Core could not handle the query above (threw exception about inability to translate)
            // so now we just get the entire division and query the schedule list directly.

            var division = await this.GetDivision(organization, abbreviation);

            var games = division.Schedule
                .Where(s => s.GameID == gameID)
                .SelectMany(s => division.Schedule.Where(inner => inner.Day == s.Day && inner.Field == s.Field))
                .ToList();

            if (games != null)
            {
                foreach (var game in games)
                {
                    list.Add(game);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }

        return list;
    }

    /// <inheritdoc/>
    public async Task SaveDivision(Division division,
                                   bool deleteDivision = false, bool createDivision = false)
    {
        try
        {
            if (deleteDivision)
            {
                this.DbContext.Divisions.Remove(division);
            }
            else if (createDivision)
            {
                this.DbContext.Divisions.Add(division);
            }
            else
            {
                this.DbContext.Update(division);
            }

            await this.DbContext.SaveChangesAsync();
            return;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
