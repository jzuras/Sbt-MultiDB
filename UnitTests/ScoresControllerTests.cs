using Microsoft.AspNetCore.Mvc;
using Moq;
using SbtMultiDB.Controllers;
using SbtMultiDB.Models;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Models.ViewModels;
using SbtMultiDB.Services;

namespace UnitTests;

public class ScoresControllerTests
{
    #region Read Only Constants
    private readonly string Organization = "SbtMultiUnitTestOrg";
    private readonly string Abbreviation = "Test01";
    private readonly int GameID = 3;
    #endregion

    [Fact]
    public async Task Index_ReturnsGame()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetGames(It.IsAny<GetScoresRequest>()))
           .ReturnsAsync((GetScoresRequest request) => GetTestGames(request))
           .Verifiable();
        var controller = new ScoresController(mockService.Object);

        // Act
        var result = await controller.Index(this.Organization, this.Abbreviation, this.GameID);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ScoresViewModel>(
            viewResult.ViewData.Model);
        Assert.Equal(this.GameID, model.Schedule[0].GameID);

        // Verify that SaveScores was called.
        mockService.Verify();
    }

    [Fact]
    public async Task Index_ReturnsNotFound()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetGames(It.IsAny<GetScoresRequest>()))
           .ReturnsAsync((GetScoresRequest request) => GetTestGames(request));
        var controller = new ScoresController(mockService.Object);

        // Act
        var result = await controller.Index(this.Organization, this.Abbreviation, 0);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_ReturnsNotFoundAfterCatchingException()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.GetGames(It.IsAny<GetScoresRequest>()))
            .Throws<Exception>();
        var controller = new ScoresController(mockService.Object);

        // Act
        var result = await controller.Index(this.Organization, this.Abbreviation, 0);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task IndexPost_Returns_A_RedirectAndSavesScores_WhenModelStateIsValid()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        UpdateScoresResponse response = new UpdateScoresResponse();
        response.Success = true;

        mockService.Setup(repo => repo.SaveScores(It.IsAny<UpdateScoresRequest>()))
            .Returns(Task.FromResult<UpdateScoresResponse>(response))
            .Verifiable();
        var controller = new ScoresController(mockService.Object);

        var model = new ScoresViewModel();
        model.Schedule = new List<ScheduleSubsetForScoresViewModel>();

        // Act
        var result = await controller.Index(model, this.Organization, this.Abbreviation);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Standings", redirectToActionResult.ControllerName);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        
        // Verify that SaveScores was called.
        mockService.Verify();
    }

    [Fact]
    public async Task IndexPost_ReturnsViewResult_WhenModelStateIsInvalid()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        var controller = new ScoresController(mockService.Object);

        controller.ModelState.AddModelError("HomeScore", "Required");
        var model = new ScoresViewModel();
        model.Schedule = new List<ScheduleSubsetForScoresViewModel>();

        // Act
        var result = await controller.Index(model, this.Organization, this.Abbreviation);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelResult = Assert.IsAssignableFrom<ScoresViewModel>(
            viewResult.ViewData.Model);
        Assert.Equal(0, modelResult.Schedule.Count);
    }

    [Fact]
    public async Task IndexPost_ReturnsViewResult_WhenSavesScoresFails()
    {
        // Arrange
        var mockService = new Mock<IDivisionService>();
        mockService.Setup(repo => repo.SaveScores(It.IsAny<UpdateScoresRequest>()))
            .ReturnsAsync((UpdateScoresRequest request) => 
                new UpdateScoresResponse { Success = false })
            .Verifiable();
        var controller = new ScoresController(mockService.Object);

        var model = new ScoresViewModel();
        model.Schedule = new List<ScheduleSubsetForScoresViewModel>();

        // Act
        var result = await controller.Index(model, this.Organization, this.Abbreviation);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelResult = Assert.IsAssignableFrom<ScoresViewModel>(
            viewResult.ViewData.Model);
        Assert.Equal(0, modelResult.Schedule.Count);

        // Verify that SaveScores was called.
        mockService.Verify();
    }

    #region Helper Method
    private GetScoresResponse GetTestGames(GetScoresRequest request)
    {
        if (request.GameID == this.GameID)
        {
            List<Schedule> games = new List<Schedule>();
            Schedule schedule = new Schedule();
            schedule.GameID = this.GameID;
            games.Add(schedule);
            GetScoresResponse response = new GetScoresResponse();
            response.Games = games;
            response.Success = true;
            return response;
        }

        return new GetScoresResponse();
    }
    #endregion
}