using Microsoft.AspNetCore.Http;
using SbtMultiDB.Data.Repositories;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Services;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace UnitTests;

public class DatabaseServiceTests : IClassFixture<TestFixture>
{
    #region Properties
    private TestFixture Fixture { get; init; }
    private string Organization { get; init; }
    private ITestOutputHelper Output { get; init; }
    #endregion

    #region Read Only Constants
    private readonly string FilenameForValidData = "TestDataValid.csv";
    private readonly string FilenameForInvalidData = "TestData-Bad.csv";
    private readonly string FilenameForEmptyData = "TestData-Empty.csv";
    #endregion

    public static IEnumerable<object[]> TestDatabases()
    {
        yield return new object[] { IDivisionRepository.RepositoryType.SqlServer };
        yield return new object[] { IDivisionRepository.RepositoryType.Cosmos };
        yield return new object[] { IDivisionRepository.RepositoryType.MySql };
        yield return new object[] { IDivisionRepository.RepositoryType.PostgreSql };
    }

    public DatabaseServiceTests(TestFixture fixture, ITestOutputHelper output)
    {
        this.Output = output;
        this.Fixture = fixture;
        this.Organization = Fixture.Organization;
    }

    [Theory]
    [MemberData(nameof(TestDatabases))]
    public async Task LoadSchedule_ReturnsFailure_WhenFileIsEmpty(IDivisionRepository.RepositoryType database)
    {
        // Arrange
        var service = this.Fixture.GetService(database);
        var abbreviation = this.CreateUniqueAbbreviation();

        // Act
        var result = (LoadScheduleResponse)await this.LoadScheduleFileAsync(service, abbreviation,
            true, this.FilenameForEmptyData);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("line number: 4", result.Message.ToLower());
    }

    [Theory]
    [MemberData(nameof(TestDatabases))]
    public async Task LoadSchedule_ReturnsFailure_WhenFileIsInvalid(IDivisionRepository.RepositoryType database)
    {
        // Arrange
        var service = this.Fixture.GetService(database);
        var abbreviation = this.CreateUniqueAbbreviation();

        // Act
        var result = (LoadScheduleResponse)await this.LoadScheduleFileAsync(service, abbreviation,
            true, this.FilenameForInvalidData);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("line number: 13", result.Message.ToLower());
    }

    [Theory]
    [MemberData(nameof(TestDatabases))]
    public async Task LoadSchedule_ReturnsFailure_WhenNoDivision(IDivisionRepository.RepositoryType database)
    {
        // Arrange
        var service = this.Fixture.GetService(database);
        var abbreviation = this.CreateUniqueAbbreviation();

        // Act
        var result = (LoadScheduleResponse)await this.LoadScheduleFileAsync(service, abbreviation,
            true, this.FilenameForInvalidData,
            skipCreatingDivsion: true);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("division not found", result.Message.ToLower());
    }

    [Theory]
    [MemberData(nameof(TestDatabases))]
    public async Task LoadSchedule_ReturnsSuccess_WhenFileIsValid(IDivisionRepository.RepositoryType database)
    {
        // Arrange
        var expectedResultFirstGameDate =
            DateTime.ParseExact("9/3/2023", "M/d/yyyy", CultureInfo.InvariantCulture);
        var expectedResultLastGameDate =
            DateTime.ParseExact("10/24/2023", "MM/dd/yyyy", CultureInfo.InvariantCulture);
        var abbreviation = this.CreateUniqueAbbreviation();

        var service = this.Fixture.GetService(database);

        // Act
        var result = (LoadScheduleResponse)await this.LoadScheduleFileAsync(service, abbreviation,
            true, this.FilenameForValidData);

        // Assert
        this.AssertTrue(result);

        Assert.Equal(expectedResultFirstGameDate, result.FirstGameDate);
        Assert.Equal(expectedResultLastGameDate, result.LastGameDate);
        Assert.Null(result.Message);
    }

    [Theory]
    [MemberData(nameof(TestDatabases))]
    public async Task LoadSchedule_ReturnsSuccess_LoadingTwoDivisions(IDivisionRepository.RepositoryType database)
    {
        // Arrange
        var expectedResultFirstGameDate =
            DateTime.ParseExact("9/3/2023", "M/d/yyyy", CultureInfo.InvariantCulture);
        var expectedResultLastGameDate =
            DateTime.ParseExact("10/24/2023", "MM/dd/yyyy", CultureInfo.InvariantCulture);
        var abbreviation = this.CreateUniqueAbbreviation();

        var service = this.Fixture.GetService(database);

        // Act
        var result = (LoadScheduleResponse) await this.LoadScheduleFileAsync(service, abbreviation + "1", 
            true, this.FilenameForValidData);
        var result2 = (LoadScheduleResponse) await this.LoadScheduleFileAsync(service, abbreviation + "2", 
            true, this.FilenameForValidData);

        // Assert
        this.AssertTrue(result);
        this.AssertTrue(result2);

        Assert.Equal(expectedResultFirstGameDate, result.FirstGameDate);
        Assert.Equal(expectedResultLastGameDate, result.LastGameDate);
        Assert.Equal(expectedResultFirstGameDate, result2.FirstGameDate);
        Assert.Equal(expectedResultLastGameDate, result2.LastGameDate);
        Assert.Null(result.Message);
        Assert.Null(result2.Message);
    }

    [Theory]
    [MemberData(nameof(TestDatabases))]
    public async Task CanCreateService_AndCreateDivision(IDivisionRepository.RepositoryType database)
    {
        // This is a simple proof-of-concept test to quickly ensure that
        // the database services are properly created in the fixture.
        // The test creates a division to ensure the service is functional.

        // Arrange
        var service = this.Fixture.GetService(database);
        var abbreviation = this.CreateUniqueAbbreviation();

        // Act
        var result = await this.CreateDivisionAsync(service, abbreviation);

        // Assert
        this.AssertTrue(result);
    }

    [Theory]
    [MemberData(nameof(TestDatabases))]
    public async Task DeleteDivision_DeletesStandingsAndSchedule(IDivisionRepository.RepositoryType database)
    {
        // This test confirms that any standings and schedule data
        // are deleted when the division is deleted. This is done
        // by loading a schedule, deleting the division, and
        // loading the same schedule again for the same division.
        // The division will be re-created, but adding entries
        // to the standings or schedule tables will fail it
        // they were not previously cleared out by the delete.

        // Arrange
        var abbreviation = this.CreateUniqueAbbreviation();
        var request = new DeleteDivisionRequest
        {
            Organization = this.Organization,
            Abbreviation = abbreviation
        };
        var service = this.Fixture.GetService(database);
        await this.LoadScheduleFileAsync(service, abbreviation, true, this.FilenameForValidData);
        await service.DeleteDivision(request);

        // Act
        var result = await this.LoadScheduleFileAsync(service, abbreviation, true, this.FilenameForValidData);

        // Assert
        this.AssertTrue(result);
    }

    #region Helper Methods
    private async Task<IResponse> CreateDivisionAsync(IDivisionService service, string abbreviation)
    {
        // Arrange
        var request = new CreateDivisionRequest
        {
            Organization = this.Organization,
            Abbreviation = abbreviation,
            League = "testLeague",
            NameOrNumber = "13"
        };

        // Act
        var result = await service.CreateDivision(request);

        // Assert
        Assert.IsType<CreateDivisionResponse>(result);
        this.AssertTrue(result);

        return result;
    }

    private async Task<IResponse> LoadScheduleFileAsync(IDivisionService service, string abbreviation, 
        bool useDoubleHeaders, string fileName, bool skipCreatingDivsion = false)
    {
        // Arrange
        var memoryStream = Utilities.GetMemoryStreamForDataFile(fileName);
        var scheduleFile = new FormFile(memoryStream, 0, memoryStream.Length, "", "");
        var request = new LoadScheduleRequest
        {
            Organization = this.Organization,
            Abbreviation = abbreviation,
            UsesDoubleHeaders = useDoubleHeaders,
            ScheduleFile = scheduleFile
        };
        if (skipCreatingDivsion == false)
        {
            await this.CreateDivisionAsync(service, abbreviation);
        }

        // Act
        var result = await service.LoadScheduleFileAsync(request);

        // Assert
        Assert.IsType<LoadScheduleResponse>(result);

        return result;
    }

    private string CreateUniqueAbbreviation([CallerMemberName] string callerMemberName = "")
    {
        return Guid.NewGuid().ToString("N");
    }

    private void AssertTrue(IResponse result)
    {
        if (result.Success == false)
        {
            Output.WriteLine(result.Message);
        }
        Assert.True(result.Success);
    }
    #endregion
}
