using OpenQA.Selenium;

namespace SeleniumTests.PageObjectModels.Admin;

internal class LoadSchedulePage
{
    #region Properties
    private IWebDriver Driver { get; init; }
    private string FullURL { get; init; }
    private string Title { get; init; }
    #endregion


    #region Read Only Constants
    private readonly string PageURL = "Admin/LoadSchedule";

    #region Page Elements
    private readonly By ByAbbreviation = By.Id("Abbreviation");
    private readonly By ByUsesDoubleHeaders = By.Id("UsesDoubleHeaders");
    private readonly By ByScheduleFile = By.Id("ScheduleFile");
    private readonly By ByProcessFileButton = By.Id("ProcessFile");
    #endregion
    #endregion

    internal LoadSchedulePage(IWebDriver driver, string baseURL, string organization)
    {
        this.Driver = driver;
        this.FullURL = $"{baseURL}/{this.PageURL}/{organization}";
        this.Title = $"{organization}: Admin: Load Schedule - SoftballTech";
        this.Load();
        if (driver.Title != this.Title)
        {
            throw new InvalidDataException(
                "Unable to navigate to the Admin Load Schedule Page for the provided Organization.");
        }
    }

    internal void Load()
    {
        this.Driver.Navigate().GoToUrl(this.FullURL);
    }

    #region Action Methods
    internal void ClickProcessFileButton()
    {
        this.Driver.FindElement(this.ByProcessFileButton).Click();
    }

    internal void SetUseDoubleHeaders()
    {
        var checkbox = SeleniumWrapper.FindElementWithRetry(this.Driver, this.ByUsesDoubleHeaders);
        if (checkbox!.Selected == false)
        {
            checkbox.Click();
        }
    }

    internal void ClearUseDoubleHeaders()
    {
        var checkbox = SeleniumWrapper.FindElementWithRetry(this.Driver, this.ByUsesDoubleHeaders);
        if (checkbox!.Selected == true)
        {
            checkbox.Click();
        }
    }

    internal void EnterDivisionAndScheduleInformation(string abbreviation, string fileName)
    {
        SeleniumWrapper.SendKeys(this.Driver, this.ByAbbreviation, abbreviation);
        SeleniumWrapper.SendKeys(this.Driver, this.ByScheduleFile, fileName);
    }
    #endregion
}
