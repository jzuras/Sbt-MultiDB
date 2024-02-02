using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Models.ViewModels;
using SbtMultiDB.Services;

namespace SbtMultiDB.Controllers.Admin;

 public class AdminController : Controller
{
    #region Properties
    private IDivisionService Service { get; init; } = default!;
    public List<string> OrganizationList { get; init; } = new List<string>();
    #endregion

    public AdminController(IConfiguration configuration, IDivisionService service)
    {
        // This populates the HTML Select element list on the page.
        // The data should really come from the database,
        // but I used this method for simplicty.
        this.OrganizationList =
            configuration.GetSection("Organizations")?.Get<List<string>>() ?? new List<string>();

        this.Service = service;
    }

    [AcceptVerbs("Get", "Post")]
    [Route("/Admin/DivisionExists")]
    public async Task<IActionResult> DivisionExists(string organization, string abbreviation)
    {
        // This action method handles the Remote attribute for the Abbreviation text box,
        // and will return false if the division can be found, true if not.
        // This allows validating the division without a full postback.

        var request = new DivisionExistsRequest
        {
            Organization = organization,
            Abbreviation = abbreviation,
        };

        var response = await this.Service.DivisionExists(request);

        bool divisionFound = response.Success;

        return Json(divisionFound);
    }

    // GET: Admin/{organization?}
    public IActionResult Index(string? organization)
    {
        var model = (organization, this.OrganizationList);
        return View(model);
    }

    // GET: Admin/LoadSchedule/{organization}
    [ParametersNotNullActionFilter(checkAbbreviation: false)]
    public IActionResult LoadSchedule(string organization)
    {
        LoadScheduleViewModel model = new();

        model.Organization = organization;

        return View(model);
    }

    // POST: Admin/LoadSchedule/{organization}
    [ParametersNotNullActionFilter(checkAbbreviation: false)]
    [FeatureGate("EnableSubmitButton")]
    [HttpPost, ActionName("LoadSchedule")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoadSchedulePost(string organization, LoadScheduleViewModel model)
    {
        var request = new LoadScheduleRequest
        {
            Organization = organization,
            Abbreviation = model.Abbreviation,
            ScheduleFile = model.ScheduleFile,
            UsesDoubleHeaders = model.UsesDoubleHeaders,
        };

        var response = await this.Service.LoadScheduleFileAsync(request);

        if (response.Success)
        {
            model.ResultMessage = DateTime.Now.ToShortTimeString() + ": Success loading schedule from " +
                model.ScheduleFile.FileName + ". <br>Games start on " +
                response.FirstGameDate.ToShortDateString() +
                " and end on " +
                response.LastGameDate.ToShortDateString();
        }
        else
        {
            model.ResultMessage = DateTime.Now.ToShortTimeString() + ": Failure loading schedule from " +
                model.ScheduleFile.FileName + ". <br>Error message: " + response.Message;
        }

        model.ResultSuccess = response.Success;

        return View(model);
    }
}
