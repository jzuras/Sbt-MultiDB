using Microsoft.AspNetCore.Mvc;
using Moq;
using SbtMultiDB.Controllers;
using SbtMultiDB.Models;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Services;

namespace UnitTests;

public class DivisionsControllerTests
{
    #region Read Only Constants
    private readonly string Organization = "SbtMultiUnitTestOrg";
    private readonly string Abbreviation = "Test01";
    #endregion

    [Fact]
    public async Task IsDivisionAbbreviationAvailable_ReturnsJsonBoolean()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        DivisionExistsResponse response = new DivisionExistsResponse();
        response.Success = true;

        mockService.Setup(repo => repo.DivisionExists(It.IsAny<DivisionExistsRequest>()))
            .Returns(Task.FromResult<DivisionExistsResponse>(response))
            .Verifiable();
        var controller = new DivisionsController(mockService.Object);

        // Act
        var result = await controller.IsDivisionAbbreviationAvailable(this.Organization, this.Abbreviation);

        // Assert
        Assert.IsType<JsonResult>(result);

        var jsonResult = result as JsonResult;
        Assert.NotNull(jsonResult);

        Assert.IsType<bool>(jsonResult.Value);

        // Expecting a "false" result because "true" is the response from DivisionExists(),
        // which means the division abbreviation is NOT available when creating a new division.
        Assert.False((bool)jsonResult.Value);

        // Verify that mocked method was called.
        mockService.Verify();
    }

    #region Create Division Tests
    [Fact]
    public async Task CreatePost_Returns_A_RedirectAndCreatesDivision_WhenModelStateIsValid()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        CreateDivisionResponse response = new CreateDivisionResponse();
        response.Success = true;

        mockService.Setup(repo => repo.CreateDivision(It.IsAny<CreateDivisionRequest>()))
            .Returns(Task.FromResult<CreateDivisionResponse>(response))
            .Verifiable();
        var controller = new DivisionsController(mockService.Object);

        var model = new Division();

        // Act
        var result = await controller.Create(model, this.Organization);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Null(redirectToActionResult.ControllerName);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        
        // Verify that mocked method was called.
        mockService.Verify();
    }

    [Fact]
    public async Task CreatePost_ReturnsViewResult_WhenModelStateIsInvalid()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        var controller = new DivisionsController(mockService.Object);

        controller.ModelState.AddModelError("Abbreviation", "Required");
        var model = new Division();

        // Act
        var result = await controller.Create(model, this.Organization);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelResult = Assert.IsAssignableFrom<Division>(
            viewResult.ViewData.Model);
        Assert.Empty(modelResult.Abbreviation);
    }

    [Fact]
    public async Task CreatePost_ReturnsViewResult_WhenCreateDivisionFails()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        CreateDivisionResponse response = new CreateDivisionResponse();
        response.Success = false;
        response.Message = "error";

        mockService.Setup(repo => repo.CreateDivision(It.IsAny<CreateDivisionRequest>()))
            .Returns(Task.FromResult<CreateDivisionResponse>(response))
            .Verifiable();
        var controller = new DivisionsController(mockService.Object);

        var model = new Division();

        // Act
        var result = await controller.Create(model, this.Organization);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelResult = Assert.IsAssignableFrom<Division>(
            viewResult.ViewData.Model);
        Assert.Empty(modelResult.Abbreviation);

        // Verify that mocked method was called.
        mockService.Verify();
    }
    #endregion

    #region Edit Division Tests
    [Fact]
    public async Task EditPost_ReturnsARedirectAndEditsDivision_WhenModelStateIsValid()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        UpdateDivisionResponse response = new UpdateDivisionResponse();
        response.Success = true;

        mockService.Setup(repo => repo.UpdateDivision(It.IsAny<UpdateDivisionRequest>()))
            .Returns(Task.FromResult<UpdateDivisionResponse>(response))
            .Verifiable();
        var controller = new DivisionsController(mockService.Object);

        var model = new Division();

        // Act
        var result = await controller.Edit(model, this.Organization, this.Abbreviation);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Null(redirectToActionResult.ControllerName);
        Assert.Equal("Index", redirectToActionResult.ActionName);

        // Verify that mocked method was called.
        mockService.Verify();
    }

    [Fact]
    public async Task EditPost_ReturnsViewResult_WhenModelStateIsInvalid()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        var controller = new DivisionsController(mockService.Object);

        controller.ModelState.AddModelError("Abbreviation", "Required");
        var model = new Division();

        // Act
        var result = await controller.Edit(model, this.Organization, this.Abbreviation);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelResult = Assert.IsAssignableFrom<Division>(
            viewResult.ViewData.Model);
        Assert.Empty(modelResult.Abbreviation);
    }

    [Fact]
    public async Task EditPost_ReturnsViewModel_WhenEditDivisionFails()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        UpdateDivisionResponse response = new UpdateDivisionResponse();
        response.Success = false;
        response.Message = "error";

        mockService.Setup(repo => repo.UpdateDivision(It.IsAny<UpdateDivisionRequest>()))
            .Returns(Task.FromResult<UpdateDivisionResponse>(response))
            .Verifiable();
        var controller = new DivisionsController(mockService.Object);

        var model = new Division();

        // Act
        var result = await controller.Edit(model, this.Organization, this.Abbreviation);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelResult = Assert.IsAssignableFrom<Division>(
            viewResult.ViewData.Model);
        Assert.Empty(modelResult.Abbreviation);

        // Verify that mocked method was called.
        mockService.Verify();
    }
    #endregion

    #region Delete Division Tests
    [Fact]
    public async Task DeleteConfirmed_Returns_A_RedirectAndDeletesDivision_WhenModelStateIsValid()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        DeleteDivisionResponse response = new DeleteDivisionResponse();
        response.Success = true;

        mockService.Setup(repo => repo.DeleteDivision(It.IsAny<DeleteDivisionRequest>()))
            .Returns(Task.FromResult<DeleteDivisionResponse>(response))
            .Verifiable();
        var controller = new DivisionsController(mockService.Object);

        var model = new Division();

        // Act
        var result = await controller.DeleteConfirmed(model, this.Organization, this.Abbreviation);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Null(redirectToActionResult.ControllerName);
        Assert.Equal("Index", redirectToActionResult.ActionName);

        // Verify that mocked method was called.
        mockService.Verify();
    }

    [Fact]
    public async Task DeleteConfirmed_ReturnsViewResult_WhenDeleteDivisionFails()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        DeleteDivisionResponse response = new DeleteDivisionResponse();
        response.Success = false;
        response.Message = "error";

        mockService.Setup(repo => repo.DeleteDivision(It.IsAny<DeleteDivisionRequest>()))
            .Returns(Task.FromResult<DeleteDivisionResponse>(response))
            .Verifiable();
        var controller = new DivisionsController(mockService.Object);

        var model = new Division();

        // Act
        var result = await controller.DeleteConfirmed(model, this.Organization, this.Abbreviation);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelResult = Assert.IsAssignableFrom<Division>(
            viewResult.ViewData.Model);
        Assert.Empty(modelResult.Abbreviation);

        // Verify that mocked method was called.
        mockService.Verify();
    }
    #endregion
}