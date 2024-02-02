using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SbtMultiDB.Models;

namespace SbtMultiDB.Data;

/// <summary>
/// Base class for db contexts for various types of databases.
/// </summary>
public class DivisionContext : DbContext
{
    public DbSet<SbtMultiDB.Models.Division> Divisions { get; set; } = default!;

    public DivisionContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Common model builder code for all databases.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // For all DBs.
        modelBuilder.Entity<Division>()
            .HasKey(d => new { d.Organization, d.Abbreviation });
    }

    /// <summary>
    /// Common model builder code for SQL databases.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected void OnModelCreatingSqlCommon(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Standings>()
            .HasKey(s => new { s.Organization, s.Abbreviation, s.TeamID });
        modelBuilder.Entity<Schedule>()
            .HasKey(s => new { s.Organization, s.Abbreviation, s.GameID });
    }

    /// <summary>
    /// Common SaveChanges() code for SQL databases.
    /// </summary>
    protected async Task<int> SaveChangesSqlCommonAsync(CancellationToken cancellationToken = default)
    {
        if (ChangeTracker.HasChanges())
        {
            var divisionsToDelete = new List<Division>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Deleted && entry.Entity is Division division)
                {
                    divisionsToDelete.Add(division);
                }
            }

            foreach (var division in divisionsToDelete)
            {
                // Manually delete related Standings and Schedule.
                var relatedStandings = Set<Standings>().Where(s => s.Organization == division.Organization && s.Abbreviation == division.Abbreviation);
                var relatedSchedule = Set<Schedule>().Where(s => s.Organization == division.Organization && s.Abbreviation == division.Abbreviation);

                Set<Standings>().RemoveRange(relatedStandings);
                Set<Schedule>().RemoveRange(relatedSchedule);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        return 0;
    }
}

#region Database-Specific Context Classes
public class PostgreSqlContext : DivisionContext
{
    public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        base.OnModelCreatingSqlCommon(modelBuilder);
     
        // Postgre-specific (DB requires UTC kind for DateTime).
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new SetDateTimeKindForPostgreSql());
                }
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesSqlCommonAsync(cancellationToken);
    }
}

public class MySqlContext : DivisionContext
{
    public MySqlContext(DbContextOptions<MySqlContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        base.OnModelCreatingSqlCommon(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesSqlCommonAsync(cancellationToken);
    }
}

public class SqlServerContext : DivisionContext
{
    public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        base.OnModelCreatingSqlCommon(modelBuilder);

        modelBuilder.Entity<Division>().ToTable("SbtMultiDB_Division");
        modelBuilder.Entity<Standings>().ToTable("SbtMultiDB_Standings");
        modelBuilder.Entity<Schedule>().ToTable("SbtMultiDB_Schedule");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesSqlCommonAsync(cancellationToken);
    }
}

public class CosmosContext : DivisionContext
{
    protected readonly string _containerName = "organizations";

    public CosmosContext(DbContextOptions<CosmosContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // for cosmos
        modelBuilder.HasDefaultContainer(this._containerName);

        modelBuilder.Entity<Division>().HasPartitionKey(d => d.Organization);

        modelBuilder.Entity<Division>()
            .HasDiscriminator<string>("SbtMultiDB");
    }
}
#endregion

/// <summary>
/// Postgre SQL DB throws an exception when trying to save DateTime fields
/// which do not have a setting for DateTimeKind, and it requires that
/// setting to be UTC. This class sets the Kind to UTC to make Postgre happy.
/// (None of the other DBs seem to care, so only Postgre uses this.)
/// </summary>
public class SetDateTimeKindForPostgreSql : ValueConverter<DateTime, DateTime>
{
    public SetDateTimeKindForPostgreSql() : base(
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    { }
}
