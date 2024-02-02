using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SbtMultiDB.Controllers;
using SbtMultiDB.Models;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Models.ViewModels;
using SbtMultiDB.Services;

namespace UnitTests;

public class StandingsControllerTests
{
    #region Read Only Constants
    private readonly string Organization = "SbtMultiUnitTestOrg";
    private readonly string Abbreviation = "Test01";
    #endregion

    [Fact]
    public async Task Index_ReturnsDivision()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetDivision(It.IsAny<GetDivisionRequest>()))
           .ReturnsAsync((GetDivisionRequest request) => GetTestDivision(request))
           .Verifiable();
        var controller = new StandingsController(mockService.Object);

        // Act
        var result = await controller.Index(this.Organization, this.Abbreviation, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<StandingsViewModel>(
            viewResult.ViewData.Model);
        Assert.Equal(this.Abbreviation, model.Division.Abbreviation);

        // Verify that mocked method was called.
        mockService.Verify();
    }

    [Fact]
    public async Task Index_ReturnsPartialViewForAjaxCall()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetDivision(It.IsAny<GetDivisionRequest>()))
            .ReturnsAsync((GetDivisionRequest request) => GetTestDivision(request))
            .Verifiable();
        var controller = new StandingsController(mockService.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var viewResult = await controller.Index(this.Organization, this.Abbreviation, null);

        // Assert
        Assert.IsType<PartialViewResult>(viewResult);

        // Verify that mocked method was called.
        mockService.Verify();
    }

    [Fact]
    public async Task Index_ReturnsNotFound()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetDivision(It.IsAny<GetDivisionRequest>()))
           .ReturnsAsync((GetDivisionRequest request) => GetTestDivision(request))
           .Verifiable();
        var controller = new StandingsController(mockService.Object);

        // Act
        var result = await controller.Index("", "", "");

        // Assert
        Assert.IsType<NotFoundResult>(result);

        // Verify that mocked method was called.
        mockService.Verify();
    }

    [Fact]
    public async Task Index_ReturnsNotFoundAfterCatchingException()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetDivision(It.IsAny<GetDivisionRequest>()))
            .Throws<Exception>();
        var controller = new StandingsController(mockService.Object);

        // Act
        var result = await controller.Index("", "", "");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #region Helper Method
    private GetDivisionResponse GetTestDivision(GetDivisionRequest request)
    {
        if (request.Organization == this.Organization)
        {
            GetDivisionResponse response = new GetDivisionResponse();
            response.Division = new Division { Organization = this.Organization, Abbreviation = this.Abbreviation };
            response.Success = true;
            return response;
        }

        return new GetDivisionResponse();
    }
    #endregion
}