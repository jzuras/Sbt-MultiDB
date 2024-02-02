using OpenQA.Selenium;

namespace SeleniumTests.PageObjectModels.Admin;

internal class CreateDivisionPage
{
    #region Properties
    private IWebDriver Driver { get; init; }
    private string FullURL { get; init; }
    private string Title { get; init; }
    #endregion

    #region Read Only Constants
    private readonly string PageURL = "Admin/Divisions/Create";

    #region Page Elements
    private readonly By ByAbbreviation = By.Id("Abbreviation");
    private readonly By ByLeague = By.Id("League");
    private readonly By ByNameOrNumber = By.Id("NameOrNumber");
    private readonly By ByCreateButton = By.Id("CreateButton");
    #endregion
    #endregion

    internal CreateDivisionPage(IWebDriver driver, string baseURL, string organization)
    {
        this.Driver = driver;
        this.FullURL = $"{baseURL}/{this.PageURL}/{organization}";
        this.Title = $"{organization}: Admin: Create Division - SoftballTech";
        this.Load();
        if (driver.Title != this.Title)
        {
            throw new InvalidDataException(
                "Unable to navigate to the Admin Create Division Page for the provided Organization.");
        }
    }

    internal void Load()
    {
        this.Driver.Navigate().GoToUrl(this.FullURL);
    }

    #region Action Methods
    internal void ClickCreateButton()
    {
        this.Driver.FindElement(this.ByCreateButton).Click();
    }

    internal void EnterDivisionInformation(string abbreviation, string league, string nameOrNumber)
    {
        SeleniumWrapper.SendKeys(this.Driver, this.ByAbbreviation, abbreviation);
        SeleniumWrapper.SendKeys(this.Driver, this.ByLeague, league);
        SeleniumWrapper.SendKeys(this.Driver, this.ByNameOrNumber, nameOrNumber);
    }
    #endregion
}
