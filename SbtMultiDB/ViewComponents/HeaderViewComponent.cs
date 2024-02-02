using Microsoft.AspNetCore.Mvc;

namespace SbtMultiDB.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    public record HeaderRecord(string SubHeading, string PartialTitle = "");

    public IViewComponentResult Invoke(string subHeading, string partialTitle = "")
    {
        var model = new HeaderRecord(subHeading, partialTitle);

        return View(model);
    }
}
