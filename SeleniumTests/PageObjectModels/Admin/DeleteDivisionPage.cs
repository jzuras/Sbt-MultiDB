using OpenQA.Selenium;

namespace SeleniumTests.PageObjectModels.Admin;

public class DeleteDivisionPage
{
    #region Properties
    private IWebDriver Driver { get; init; }
    private string FullURL { get; init; }
    private string Title { get; init; }
    #endregion

    #region Read Only Constants
    private readonly string PageURL = "Admin/Divisions/Delete";

    #region Page Elements
    private readonly By ByDeleteButton = By.Id("DeleteButton");
    #endregion
    #endregion

    internal DeleteDivisionPage(IWebDriver driver, string baseURL, string organization, string abbreviation)
    {
        this.Driver = driver;
        this.FullURL = $"{baseURL}/{this.PageURL}/{organization}/{abbreviation}";
        this.Title = $"{organization}: Admin: Delete Division - SoftballTech";
        this.Load();
        if (driver.Title != this.Title)
        {
            throw new InvalidDataException(
                "Unable to navigate to the Admin Delete Division Page for the provided Organization.");
        }
    }

    internal void Load()
    {
        this.Driver.Navigate().GoToUrl(this.FullURL);
    }

    #region Action Methods
    internal void ClickDeleteButton()
    {
        SeleniumWrapper.ClickWithRetry(this.Driver, this.ByDeleteButton);
    }
    #endregion
}
