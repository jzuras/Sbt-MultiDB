using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement;
using SbtMultiDB.Data.Repositories;
using SbtMultiDB.Services;
using SbtMultiDB.Shared;

namespace SbtMultiDB;

/// <summary>
/// This action filter attribute will verify that the "organization" parameter,
/// and optionally the "abbreviation" parameter, are not null or whitespace.
/// 
/// Forces a NotFound result if the abbreviation is null or whitespace.
/// </summary>
public class ParametersNotNullActionFilter : ActionFilterAttribute
{
    private readonly bool CheckAbbreviation;

    public ParametersNotNullActionFilter(bool checkAbbreviation = true)
    {
        this.CheckAbbreviation = checkAbbreviation;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var organization = context.HttpContext.Request.RouteValues["organization"] as string;
        var abbreviation = context.HttpContext.Request.RouteValues["abbreviation"] as string;

        if (string.IsNullOrWhiteSpace(organization) ||
            (this.CheckAbbreviation == true && string.IsNullOrEmpty(abbreviation)))
        {
            context.Result = new NotFoundResult();
        }
    }

    public override void OnActionExecuted(ActionExecutedContext context) { }
}

/// <summary>
/// This filter runs before every action to extract the "organization"
/// from the Route and set the value in TempData.
/// </summary>
public class SetCurrentOrganizationActionFilter : IActionFilter
{
    private readonly string ViewDataOrgDefault = "Demo Softball";

    public SetCurrentOrganizationActionFilter()
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        string? organization = context.RouteData.Values["organization"] as string;

        if (string.IsNullOrWhiteSpace(organization))
        {
            organization = this.ViewDataOrgDefault;
        }

        if (context.Controller is Controller controller)
        {
            controller.TempData.CurrentOrganization(organization);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

/// <summary>
/// This filter runs before every action to prevent access to actions
/// that should not be called if the current DB is unvailable/invalid.
/// 
/// Availability is determined at startup by the DatabaseAvailabilityService.
/// If the database is not available, the filter redirects to Home/Index
/// after setting a flag in TempData to be used by that page.
/// 
/// The filter is NOT run for methods that change the current database:
/// "HomeController.ChangeDatabase" and "HomeController.InitializeSessionState".
/// 
/// This is a Resource Filter, so it will run before Action Filters.
/// </summary>
public class DatabaseAvailabilityFilter : IResourceFilter
{
    private DatabaseAvailabilityService Service { get; init; } = default!;
    private ISession Session { get; init; } = default!;

    public DatabaseAvailabilityFilter(DatabaseAvailabilityService databaseAvailabilityService, 
        IHttpContextAccessor httpContextAccessor)
    {
        this.Service = databaseAvailabilityService;
        this.Session = httpContextAccessor.HttpContext!.Session;
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        // Bypass the filter when changing/setting the DB.
        if (context.ActionDescriptor.DisplayName!.Contains("HomeController.ChangeDatabase") ||
            context.ActionDescriptor.DisplayName!.Contains("HomeController.InitializeSessionState"))
        {
            return;
        }

        var currentDatabase = this.Session.CurrentDatabase();

        if (currentDatabase == IDivisionRepository.RepositoryType.Unavailable ||
            (currentDatabase == IDivisionRepository.RepositoryType.SqlServer && !this.Service.IsSqlServerAvailable) ||
            (currentDatabase == IDivisionRepository.RepositoryType.Cosmos && !this.Service.IsCosmosAvailable) ||
            (currentDatabase == IDivisionRepository.RepositoryType.MySql && !this.Service.IsMySqlAvailable) ||
            (currentDatabase == IDivisionRepository.RepositoryType.PostgreSql && !this.Service.IsPostgreSqlAvailable))
        {
            this.Session.CurrentDatabase(IDivisionRepository.RepositoryType.Unavailable);

            // Set a flag so the home page knows that the current DB is not available,
            // then re-direct to the home page (if not already headed there).
            // (All other pages expect a valid DB, and are unable to handle an invalid DB.)

            this.Session.SetCurrentDatabaseIsUnavailableFlag();

            // Redirect (or continue) to home page which will
            // indicate to the user that the current DB is unavailable.
            if (context.ActionDescriptor.DisplayName!.Contains("HomeController.Index") == false)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}
