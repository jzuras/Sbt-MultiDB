using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Globalization;

namespace SeleniumTests.PageObjectModels;

internal class StandingsPage
{
    #region Properties
    private IWebDriver Driver { get; init; }
    private string FullURL { get; init; }
    private string Organization { get; init; }
    private string Abbreviation { get; init; }
    #endregion

    #region Read Only Constants
    private readonly string StandingsColumnSelector = ".standings-table td:nth-child({0})";
    private readonly string SchedulesColumnSelector = ".schedule-table td:nth-child({0})";
    private readonly int VisitorScoreColumnIndex = 2;
    private readonly int HomeScoreColumnIndex = 4;

    #region Page Elements
    private readonly By BbyUpdated = By.Id("spanUpdated");
    private readonly By ByStandingsTable = By.ClassName("standings-table");
    private readonly By BySelectionDropdown = By.Id("teamNameSelect");
    #endregion
    #endregion

    internal StandingsPage(IWebDriver driver, string baseURL, string organization, string abbreviation)
    {
        this.Driver = driver;
        this.Organization = organization;
        this.Abbreviation = abbreviation;
        this.FullURL = $"{baseURL}/{organization}/{abbreviation}";
        this.Load();

        if (driver.Title != $"{organization}: Standings: {abbreviation} - SoftballTech")
        {
            throw new InvalidDataException(
                "Unable to navigate to the Standings Page for the provided Organization");
        }

        // Both tables should have at least one row of data.
        var columnCells = Driver.FindElements(By.CssSelector(string.Format(
            this.StandingsColumnSelector, 1)));
        if (columnCells.Count == 0)
        {
            throw new InvalidDataException("No data in Standings Table.");
        }
        columnCells = Driver.FindElements(By.CssSelector(string.Format(
            this.SchedulesColumnSelector, 1)));
        if (columnCells.Count == 0)
        {
            throw new InvalidDataException("No data in Schedule Table.");
        }
    }

    internal void Load()
    {
        this.Driver.Navigate().GoToUrl(this.FullURL);
    }

    internal bool IsDriverOnPage()
    {
        return (this.Driver.Url == this.FullURL);
    }

    #region Get Data Methods
    internal DateTime GetUpdated()
    {
        var elementUpdated = this.Driver.FindElement(BbyUpdated);
        var updatedText = elementUpdated.Text;

        // Format is "Updated:  " followed by a date which we need to return.
        return DateTime.ParseExact(updatedText.Substring(10), "M/dd/yyyy h:mm tt", CultureInfo.InvariantCulture);
    }

    internal bool IsStandingsColumnZero(int columnIndex, bool isWinPct)
    {
        // Check if all cell values in the specified standings column are zero.
        var columnCells = Driver.FindElements(By.CssSelector(string.Format(
            this.StandingsColumnSelector, columnIndex)));

        if (isWinPct)
        {
            return columnCells.All(cell => cell.Text == ".000");

        }
        return columnCells.All(cell => cell.Text == "0");
    }

    internal bool AreAllScoresEmpty()
    {
        // Check if all cell values in both home and visitor score columns are empty.
        var columnCells = Driver.FindElements(By.CssSelector(string.Format(
            this.SchedulesColumnSelector, this.VisitorScoreColumnIndex)));

        bool visitorScoresEmpty = columnCells.All(cell => cell.Text == "");

        columnCells = Driver.FindElements(By.CssSelector(string.Format(
            this.SchedulesColumnSelector, this.HomeScoreColumnIndex)));

        bool homeScoresEmpty = columnCells.All(cell => cell.Text == "");

        return visitorScoresEmpty && homeScoresEmpty;
    }

    internal int GetNumberOfGamesInSchedule()
    {
        // Any column will do, using visitor scores here.
        var columnCells = Driver.FindElements(By.CssSelector(string.Format(
            this.SchedulesColumnSelector, this.VisitorScoreColumnIndex)));
        return columnCells.Count;
    }

    internal string GetSelectedTeam()
    {
        var dropdown = this.Driver.FindElement(this.BySelectionDropdown);
        SelectElement select = new SelectElement(dropdown);
        return select.SelectedOption.Text;
    }

    internal GameRecord[] GetGameRecords(int gameID, bool isDoubleHeader)
    {
        // Parse one or two HTML <tr> elements, pulling data to create
        // one or two game records.

        // First, find the game requested using the "href" attribute in the anchor tag.
        string href = $"{this.Organization}/{this.Abbreviation}/{gameID}".Replace(" ", "%20");
        var gameLink = this.Driver.FindElement(
            By.CssSelector($"a[href*='{href}']"));

        // Navigate back to the parent <tr> element.
        var parentTrElement = gameLink.FindElement(By.XPath("./ancestor::tr"));

        // Get the entire HTML string of the <tr> element.
        string trHtml = parentTrElement.GetAttribute("outerHTML");

        if (isDoubleHeader)
        {
            // Repeat for the next sibling <tr> element.
            IWebElement nextTrElement = parentTrElement.FindElement(By.XPath("following-sibling::tr"));

            string nextTrHtml = nextTrElement.GetAttribute("outerHTML");

            return new GameRecord[]
            {
                Utilities.HtmlToGameRecord(trHtml),
                Utilities.HtmlToGameRecord(nextTrHtml)
            };
        }
        else
        {
            // Single game.
            return new GameRecord[] { Utilities.HtmlToGameRecord(trHtml) };
        }
    }

    internal StandingsRecord[] GetStandingsRecords()
    {
        // Parse the Standings Table HTML to pull date to create a standings record.
        var htmlContent = this.Driver.FindElement(this.ByStandingsTable).GetAttribute("outerHTML");

        return Utilities.HtmlToStandingsRecord(htmlContent);
    }
    #endregion

    #region Action Methods
    internal StandingsPage SelectTeamInDropdownList(string teamName)
    {
        string initialUrl = this.Driver.Url;
        IWebElement dropdown = this.Driver.FindElement(this.BySelectionDropdown);

        SelectElement select = new SelectElement(dropdown);
        select.SelectByText(teamName);
        
        return this;
    }

    internal ReportScoresPage ClickOnGameToReportScores(int gameID)
    {
        string href = $"{this.Organization}/{this.Abbreviation}/{gameID}".Replace(" ", "%20");

        SeleniumWrapper.ClickWithRetry(this.Driver, By.CssSelector($"a[href*='{href}']"));

        return new ReportScoresPage(this.Driver, this.Organization, this.Abbreviation);
    }
    #endregion
}
