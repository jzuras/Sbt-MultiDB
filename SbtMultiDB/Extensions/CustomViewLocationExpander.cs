using Microsoft.AspNetCore.Mvc.Razor;

namespace SbtMultiDB;

/// <summary>
/// This modifies the AspNet Core defaults for where it should search for Views.
/// </summary>
public class CustomViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        // May add to the route values here if needed.
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        // Add to the viewLocations list to search Divisions within Admin.
        var additionalLocation = "/Views/Admin/Divisions/{0}.cshtml";

        return viewLocations.Concat(new[] { additionalLocation });
    }
}
