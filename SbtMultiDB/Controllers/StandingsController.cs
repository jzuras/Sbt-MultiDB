using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SbtMultiDB.Models;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Models.ViewModels;
using SbtMultiDB.Services;

namespace SbtMultiDB.Controllers;

public class StandingsController : Controller
{
    private IDivisionService Service { get; init; } = default!;

    public StandingsController(IDivisionService service)
    {
        this.Service = service;
    }

    // GET: Standings/{organization}/{abbreviation}/{teamName?}
    // This method also handles partial rendering for AJAX
    [ParametersNotNullActionFilter(checkAbbreviation: true)]
    public async Task<IActionResult> Index(string organization, string abbreviation, string? teamName)
    {
        try
        {
            var request = new GetDivisionRequest
            {
                Organization = organization,
                Abbreviation = abbreviation,
            };

            var response = await this.Service.GetDivision(request);

            if (response.Success == false || response.Division == null)
            {
                return NotFound();
            }

            var division = response.Division;

            StandingsViewModel model = new();

            model.Division = division;
            model.Division.Standings = model.Division.Standings
                .OrderBy(s => s.GB).ThenByDescending(s => s.Percentage).ToList();

            if (!string.IsNullOrEmpty(teamName))
            {
                bool teamExists = model.Division.Standings.Any(standing => standing.Name.ToLower() == teamName.ToLower());
                if (teamExists)
                {
                    model.TeamName = teamName;
                    model.Division.Schedule = model.Division.Schedule
                        .Where<Schedule>(s => s.Home.ToLower() == teamName.ToLower() ||
                                         s.Visitor.ToLower() == teamName.ToLower()).ToList();
                }
            }

            model.ShowOvertimeLosses = division.Organization.ToLower().Contains("hockey");

            if (base.Request != null && base.Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_SchedulePartial", model);
            }

            return View(model);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}
