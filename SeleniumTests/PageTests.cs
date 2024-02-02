using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using NUnit.Framework;
using SeleniumTests.PageObjectModels;
using SeleniumTests.PageObjectModels.Admin;

namespace SeleniumTests;

public class PageTests
{
    #region Properties
    private protected StandingsPage StandingsPage { get; set; }
    protected IWebDriver Driver { get; set; }
    #endregion

    #region Read Only Constants
    protected readonly string BaseURL = "https://localhost:7125";
    protected readonly string Organization = "SeleniumTestOrg";
    protected readonly string Abbreviation = "STOrg01";
    private readonly string League = "SeleniumTestOrg - Any Server";
    private readonly string NameOrNumber = "1";
    private readonly string ScheduleFile = "TestData.csv";
    #endregion

    [SetUp]
    public void Setup()
    {
        var database = "Cosmos";
        string currentDirectory = Environment.CurrentDirectory;
        string relativeFilePath = Path.Combine("ScheduleFiles", this.ScheduleFile);
        string fullPath = Path.Combine(currentDirectory, relativeFilePath);

        var firefoxOptions = new FirefoxOptions();
        firefoxOptions.AcceptInsecureCertificates = true;
        this.Driver = new FirefoxDriver(firefoxOptions);

        var dbPage = new DatabaseMenuPage(this.Driver, this.BaseURL);
        dbPage.SeclectDatabase(database);

        this.InitializeTestData(this.Driver);

        this.StandingsPage = new StandingsPage(this.Driver, this.BaseURL, this.Organization, this.Abbreviation);
    }

    [TearDown]
    public void TearDown()
    {
        this.DeleteDivision(this.Driver);
        this.Driver.Quit();
    }

    protected void InitializeTestData(IWebDriver driver)
    {
        this.DeleteDivision(driver);
        this.CreateDivision(driver);
        this.LoadScheduleForDivision(driver, false);
    }

    #region Helper Methods
    private void LoadScheduleForDivision(IWebDriver driver, bool useDoubleHeaders)
    {
        string currentDirectory = Environment.CurrentDirectory;
        string relativeFilePath = Path.Combine("ScheduleFiles", this.ScheduleFile);
        string scheduleFile = Path.Combine(currentDirectory, relativeFilePath);

        LoadSchedulePage page = new LoadSchedulePage(driver, this.BaseURL, this.Organization);

        page.EnterDivisionAndScheduleInformation(this.Abbreviation, scheduleFile);

        if (useDoubleHeaders)
        {
            page.SetUseDoubleHeaders();
        }
        else
        {
            page.ClearUseDoubleHeaders();
        }

        page.ClickProcessFileButton();
    }

    private void CreateDivision(IWebDriver driver)
    {
        CreateDivisionPage page = new CreateDivisionPage(driver, this.BaseURL, this.Organization);

        page.EnterDivisionInformation(this.Abbreviation, this.League, this.NameOrNumber);
        page.ClickCreateButton();
    }

    private void DeleteDivision(IWebDriver driver)
    {
        DeleteDivisionPage page;
        try
        {
            page = new DeleteDivisionPage(driver, this.BaseURL, this.Organization, this.Abbreviation);
        }
        catch
        {
            // Failure to load page is expected if division does not already exist.
            // Nothing more to do here.
            return;
        }

        // This is outside the try-catch because a failure here is not expected
        // and needs to pop up to the end user.
        page.ClickDeleteButton();
    }
    #endregion
}
