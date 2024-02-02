using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SbtMultiDB.Data.Repositories;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Services;

namespace UnitTests;

public class TestFixture: IDisposable
{
    #region Properties
    private IDivisionRepository.RepositoryType DatabaseToCreate { get; set; }
    private IDivisionService CosmosService { get; set; }
    private IDivisionService SqlServerService { get; set; }
    private IDivisionService MySqlService { get; set; }
    private IDivisionService PostgreSqlService { get; set; }
    #endregion

    #region Read Only Constants
    public readonly string Organization = "SbtMultiUnitTestOrganization";
    private readonly string ConnectionStringKeySqlServer = "Azure_Sql_ConnectionString";
    private readonly string ConnectionStringKeyCosmos = "Azure_Cosmos_ConnectionString";
    private readonly string ConnectionStringKeyMySql = "Azure_MySql_ConnectionString";
    private readonly string ConnectionStringKeyPostgreSql = "Azure_PostgreSql_ConnectionString";
    #endregion

    public TestFixture()
    {
        // Create each database service here.
        this.CosmosService = this.CreateService(IDivisionRepository.RepositoryType.Cosmos);
        this.SqlServerService = this.CreateService(IDivisionRepository.RepositoryType.SqlServer);
        this.MySqlService = this.CreateService(IDivisionRepository.RepositoryType.MySql);
        this.PostgreSqlService = this.CreateService(IDivisionRepository.RepositoryType.PostgreSql);
    }

    public IDivisionService GetService(IDivisionRepository.RepositoryType database)
    {
        switch (database)
        {
            case IDivisionRepository.RepositoryType.Cosmos:
                return this.CosmosService;

            case IDivisionRepository.RepositoryType.MySql:
                return this.MySqlService;

            case IDivisionRepository.RepositoryType.PostgreSql:
                return this.PostgreSqlService;

            default:
            case IDivisionRepository.RepositoryType.SqlServer:
                return this.SqlServerService;
        }
    }

    private IDivisionService CreateService(IDivisionRepository.RepositoryType database)
    {
        // I was unable to find a way to mock my Session Extension (in a static class),
        // so I am setting a value here to tell the mocked method which database to use.
        this.DatabaseToCreate = database;

        var mockRepoFactory = new Mock<IDivisionRepositoryFactory>();
        mockRepoFactory.Setup(repo => repo.CreateRepository(It.IsAny<IDivisionRepository.RepositoryType>()))
            .Returns((IDivisionRepository.RepositoryType currentDatabase) => CreateRepository(currentDatabase));

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(h => h.HttpContext).Returns((HttpContext)null!);

        var mockLogger = new Mock<ILogger<DivisionService>>();

        return new DivisionService(mockRepoFactory.Object, mockHttpContextAccessor.Object, mockLogger.Object);
    }

    public void Dispose()
    {
        this.CleanUp(this.CosmosService);
        this.CleanUp(this.SqlServerService);
        this.CleanUp(this.MySqlService);
        this.CleanUp(this.PostgreSqlService);
    }

    private void CleanUp(IDivisionService service)
    {
        var deleteRequest = new DeleteDivisionRequest
        {
            Organization = this.Organization
        };

        var request = new GetDivisionListRequest
        {
            Organization = this.Organization,
        };

        var divisions = service.GetDivisionList(request).GetAwaiter().GetResult();

        foreach (var division in divisions.DivisionList)
        {
            deleteRequest.Abbreviation = division.Abbreviation;
            var result = service.DeleteDivision(deleteRequest).GetAwaiter().GetResult();
            if (result == null || result.Success == false)
            {
                Console.WriteLine("Dispose() cleanup failed.");
            }
        }
    }

    #region Mocked Method
    public IDivisionRepository CreateRepository(IDivisionRepository.RepositoryType currentDatabase)
    {
        switch (this.DatabaseToCreate)
        {
            case IDivisionRepository.RepositoryType.Cosmos:
                    var contextCosmos = Utilities.CreateCosmosContext(this.ConnectionStringKeyCosmos);
                    return new CosmosRepository(contextCosmos);

            case IDivisionRepository.RepositoryType.MySql:
                var contextMySql = Utilities.CreateMySqlContext(this.ConnectionStringKeyMySql);
                return new MySqlRepository(contextMySql);

            case IDivisionRepository.RepositoryType.PostgreSql:
                var contextPostgreSql = Utilities.CreatePostgreSqlContext(this.ConnectionStringKeyPostgreSql);
                return new PostgreSqlRepository(contextPostgreSql);

            default:
            case IDivisionRepository.RepositoryType.SqlServer:
                    var context = Utilities.CreateSqlServerContext(this.ConnectionStringKeySqlServer);
                    return new SqlServerRepository(context);
        }
    }
    #endregion
}
