using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using SbtMultiDB.Controllers.Admin;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Services;

namespace UnitTests;

public class AdminControllerTests
{
    #region Read Only Constants
    private readonly string Organization = "SbtMultiUnitTestOrg";
    private readonly string Abbreviation = "Test01";
    #endregion

    [Fact]
    public async Task DivisionExists_ReturnsJsonBoolean()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        var mockService = new Mock<IDivisionService>();
        DivisionExistsResponse response = new DivisionExistsResponse();
        response.Success = false;

        mockService.Setup(repo => repo.DivisionExists(It.IsAny<DivisionExistsRequest>()))
            .Returns(Task.FromResult<DivisionExistsResponse>(response))
            .Verifiable();
        var controller = new AdminController(mockConfiguration.Object, mockService.Object);

        // Act
        var result = await controller.DivisionExists(this.Organization, this.Abbreviation);

        // Assert
        Assert.IsType<JsonResult>(result);

        var jsonResult = result as JsonResult;
        Assert.NotNull(jsonResult);

        Assert.IsType<bool>(jsonResult.Value);
        Assert.False((bool)jsonResult.Value);

        // Verify that mocked method was called.
        mockService.Verify();
    }
}
