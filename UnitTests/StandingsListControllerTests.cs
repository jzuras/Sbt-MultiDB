using Microsoft.AspNetCore.Mvc;
using Moq;
using SbtMultiDB.Controllers;
using SbtMultiDB.Models;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Services;

namespace UnitTests;

public class StandingsListControllerTests
{
    #region Read Only Constants
    private readonly string Organization = "SbtMultiUnitTestOrg";
    #endregion

    [Fact]
    public async Task Index_ReturnsAllDivisions()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetDivisionList(It.IsAny<GetDivisionListRequest>()))
           .ReturnsAsync((GetDivisionListRequest request) => GetTestDivisions(request))
           .Verifiable();
        var controller = new StandingsListController(mockService.Object);

        // Act
        var result = await controller.Index(this.Organization);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IList<Division>>(
            viewResult.ViewData.Model);
        Assert.Equal(2, model.Count());

        // Verify that mocked method was called.
        mockService.Verify();
    }

    [Fact]
    public async Task Index_ReturnsNotFound()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetDivisionList(It.IsAny<GetDivisionListRequest>()))
           .ReturnsAsync((GetDivisionListRequest request) => GetTestDivisions(request));
        var controller = new StandingsListController(mockService.Object);

        // Act
        var result = await controller.Index("");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_ReturnsNotFoundAfterCatchingException()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetDivisionList(It.IsAny<GetDivisionListRequest>()))
            .Throws<Exception>();
        var controller = new StandingsListController(mockService.Object);

        // Act
        var result = await controller.Index("");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #region Helper Method
    private GetDivisionListResponse GetTestDivisions(GetDivisionListRequest request)
    {
        if (request.Organization == this.Organization)
        {
            List<Division> divisions = new List<Division>();
            divisions.Add(new Division());
            divisions.Add(new Division());
            GetDivisionListResponse response = new GetDivisionListResponse();
            response.DivisionList = divisions;
            response.Success = true;
            return response;
        }

        return new GetDivisionListResponse();
    }
    #endregion
}