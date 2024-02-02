using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using SbtMultiDB.Models;
using SbtMultiDB.Models.Requests;
using SbtMultiDB.Models.ViewModels;
using SbtMultiDB.Services;

namespace SbtMultiDB.Controllers;

public class DivisionsController : Controller
{
    private IDivisionService Service { get; init; } = default!;

    public DivisionsController(IDivisionService service)
    {
        this.Service = service;
    }

    #region Get Action Methods
    // GET: Divisions/Create/{organization}
    [ParametersNotNullActionFilter(checkAbbreviation: false)]
    public IActionResult Create(string organization)
    {
        var model = new Division { Organization = organization };

        return View(model);
    }

    // GET: Divisions/Delete/{organization}/{abbreviation}
    [ParametersNotNullActionFilter(checkAbbreviation: true)]
    public async Task<IActionResult> Delete(string organization, string abbreviation)
    {
        return await this.PopulateModel(organization, abbreviation);
    }

    // GET: Divisions/Details/{organization}/{abbreviation}
    [ParametersNotNullActionFilter(checkAbbreviation: true)]
    public async Task<IActionResult> Details(string organization, string abbreviation)
    {
        return await this.PopulateModel(organization, abbreviation);
    }

    // GET: Divisions/Edit/{organization}/{abbreviation}
    [ParametersNotNullActionFilter(checkAbbreviation: true)]
    public async Task<IActionResult> Edit(string organization, string abbreviation)
    {
        return await this.PopulateModel(organization, abbreviation);
    }

    // GET: Divisions/{organization}
    [ParametersNotNullActionFilter(checkAbbreviation: false)]
    public async Task<IActionResult> Index(string organization)
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

        var divisionsList = response.DivisionList;

        DivisionListViewModel model = new();
        model.Organization = organization;
        model.DivisionsList = divisionsList;

        return View(model);
    }
    #endregion

    #region Post Action Methods
    [AcceptVerbs("Get", "Post")]
    [Route("/Divisions/IsDivisionAbbreviationAvailable")]
    public async Task<IActionResult> IsDivisionAbbreviationAvailable(string organization, string abbreviation)
    {
        // This action method handles (via AJAX) the Remote attribute on an Abbreviation input,
        // and will return false if the abbreviation is already in use, true if not.

        var request = new DivisionExistsRequest
        {
            Organization = organization,
            Abbreviation = abbreviation,
        };

        var response = await this.Service.DivisionExists(request);

        // Remote attribute displays error message message if we return false,
        // so we need to reverse the result from DivisionExists().
        bool abbreviationIsAvailable = (response.Success == false);

        return Json(abbreviationIsAvailable);
    }

    // POST: Divisions/Create/{organization}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ParametersNotNullActionFilter(checkAbbreviation: false)]
    [FeatureGate("EnableSubmitButton")]
    public async Task<IActionResult> Create(Division model, string organization)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var request = new CreateDivisionRequest
                {
                    Organization = model.Organization,
                    Abbreviation = model.Abbreviation,
                    League = model.League,
                    NameOrNumber = model.NameOrNumber,
                };

                var response = await this.Service.CreateDivision(request);

                if (response.Success == false)
                {
                    ModelState.AddModelError(string.Empty, response.Message);
                    return View(model);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction(nameof(Index), new { organization = organization });
        }

        return View(model);
    }

    // POST: Divisions/Edit/{organization}/{abbreviation}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ParametersNotNullActionFilter(checkAbbreviation: true)]
    [FeatureGate("EnableSubmitButton")]
    public async Task<IActionResult> Edit(Division model, string organization, string abbreviation)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var request = new UpdateDivisionRequest
                {
                    Organization = model.Organization,
                    Abbreviation = model.Abbreviation,
                    League = model.League,
                    NameOrNumber = model.NameOrNumber,
                    Locked = model.Locked,
                };

                var response = await this.Service.UpdateDivision(request);

                if (response.Success == false)
                {
                    ModelState.AddModelError(string.Empty, response.Message);
                    return View(model);
                }
            }
            catch (Exception)
            {
                throw;
            }
        
            return RedirectToAction(nameof(Index), new { organization = organization });
        }

        return View(model);
    }

    // POST: Divisions/Delete/{organization}/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [ParametersNotNullActionFilter(checkAbbreviation: true)]
    [FeatureGate("EnableSubmitButton")]
    public async Task<IActionResult> DeleteConfirmed(Division model, string organization, string abbreviation)
    {
        try
        {
            var request = new DeleteDivisionRequest
            {
                Organization = model.Organization,
                Abbreviation = model.Abbreviation,
            };

            var response = await this.Service.DeleteDivision(request);

            if (response.Success == false)
            {
                ModelState.AddModelError(string.Empty, response.Message);
                return View(model);
            }

            return RedirectToAction(nameof(Index), new { organization = organization });
        }
        catch (Exception)
        {
            throw;
        }
    }
    #endregion

    #region Helper Method
    /// <summary>
    /// Common code used by Delete/Details/Edit Action Methods
    /// </summary>
    /// <param name="organization">Organization to search for.</param>
    /// <param name="abbreviation">ID of Division to search for.</param>
    /// <returns>View(model) if found, NotFound() otherwise.</returns>
    private async Task<IActionResult> PopulateModel(string organization, string abbreviation)
    {
        var request = new GetDivisionRequest
        {
            Organization = organization,
            Abbreviation = abbreviation,
        };

        var response = await this.Service.GetDivision(request);

        var division = response.Division;

        if (division == null)
        {
            return NotFound();
        }
        
        return View(division);
    }
    #endregion
}
