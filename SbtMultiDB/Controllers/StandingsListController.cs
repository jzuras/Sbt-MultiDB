using Microsoft.AspNetCore.Mvc;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Services;

namespace SbtMultiDB.Controllers;

public class StandingsListController : Controller
{
    private IDivisionService Service { get; init; } = default!;

    public StandingsListController(IDivisionService service)
    {
        this.Service = service;
    }

    // GET: StandingsList/{organization}
    [ParametersNotNullActionFilter(checkAbbreviation: false)]
    public async Task<IActionResult> Index(string organization)
    {
        try
        {
            var request = new GetDivisionListRequest
            {
                Organization = organization,
            };

            var response = await this.Service.GetDivisionList(request);

            if (response.Success == false || response.DivisionList == null)
            {
                return NotFound();
            }

            return View(response.DivisionList);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

}
