using OpenQA.Selenium;

namespace SeleniumTests.PageObjectModels;

internal class ReportScoresPage
{
    #region Properties
    private IWebDriver Driver { get; init; }
    #endregion

    #region Page Elements
    private readonly By BySaveButton = By.Id("saveButton");
    private readonly By ByDay = By.Id("day");
    private readonly By ByField = By.Id("field");
    private readonly By[] ByTime = {
        By.CssSelector("label[for='Schedule_0__Time']"),
        By.CssSelector("label[for='Schedule_1__Time']")
    };
    private readonly By[] ByTeam = {
        By.CssSelector("label[for='Schedule_0__Visitor']"),
        By.CssSelector("label[for='Schedule_1__Visitor']"),
        By.CssSelector("label[for='Schedule_0__Home']"),
        By.CssSelector("label[for='Schedule_1__Home']")
    };
    private readonly By[] ByScore = {
        By.Id("Schedule_0__VisitorScore"),
        By.Id("Schedule_1__VisitorScore"),
        By.Id("Schedule_0__HomeScore"),
        By.Id("Schedule_1__HomeScore")
    };
    private readonly By[] ByForfeit = {
        By.Name("Schedule[0].VisitorForfeit"),
        By.Name("Schedule[1].VisitorForfeit"),
        By.Name("Schedule[0].HomeForfeit"),
        By.Name("Schedule[1].HomeForfeit")
    };
    private readonly By[] ByValidationMessage = {
        By.CssSelector("span[data-valmsg-for='Schedule[0].VisitorScore']"),
        By.CssSelector("span[data-valmsg-for='Schedule[1].VisitorScore']"),
        By.CssSelector("span[data-valmsg-for='Schedule[0].HomeScore']"),
        By.CssSelector("span[data-valmsg-for='Schedule[1].HomeScore']")
    };
    #endregion

    internal ReportScoresPage(IWebDriver driver, string organization, string abbreviation)
    {
        Driver = driver;
        if (driver.Title != $"{organization}: Report Scores: {abbreviation} - SoftballTech")
        {
            throw new InvalidDataException(
                "Unable to navigate to the Report Scores Page for the provided Organization");
        }
    }

    internal bool IsDriverOnPage()
    {
        return (this.Driver.Title.Contains(" Report Scores:"));
    }

    #region Get Data Methods
    internal GameRecord[] GetGameRecords(bool isDoubleHeader)
    {
        // Get game record(s) (2 if a doubleheader).
        var gameRecord = this.GetGameRecord(1);
        if( isDoubleHeader ==  false )
        {
            return new GameRecord[]
            {
                gameRecord
            };
        }

        var gameRecord2 = this.GetGameRecord(2);
        return new GameRecord[]
        {
                gameRecord,
                gameRecord2
        };
    }

    internal string GetScore(int gameNumber, bool forHomeTeam)
    {
        int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
        var element = this.Driver.FindElement(this.ByScore[index]);
        return element.GetAttribute("value");
    }

    internal bool GetForfeit(int gameNumber, bool forHomeTeam)
    {
        int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
        var element = this.Driver.FindElement(this.ByForfeit[index]);
        return element.Selected;
    }

    internal string GetValidationMessage(int gameNumber, bool forHomeTeam)
    {
        int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
        var element = this.Driver.FindElement(this.ByValidationMessage[index]);
        return element.Text;
    }
    #endregion

    #region Action Methods
    internal void ClickSaveButton()
    {
        this.Driver.FindElement(this.BySaveButton).Click();
    }

    internal void ClickForfeitCheckbox(int gameNumber, bool forHomeTeam)
    {
        int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
        this.Driver.FindElement(this.ByForfeit[index]).Click();
    }

    internal void EnterScore(int gameNumber, string score, bool forHomeTeam)
    {
        int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
        SeleniumWrapper.SendKeys(this.Driver, this.ByScore[index], score);
    }
    #endregion

    #region Helper Methods
    internal GameRecord GetGameRecord(int gameNumber)
    {
        // Pull data from several HTML elements to create a single game record.
        var date = this.GetGameDay();
        var field = this.GetField();
        var time = this.GetTime(gameNumber);
        var homeTeam = this.GetTeam(gameNumber, true);
        var visitorTeam = this.GetTeam(gameNumber, false);
        var homeScore = this.GetScore(gameNumber, true);
        var visitorScore = this.GetScore(gameNumber, false);
        var homeForfeit = this.GetForfeit(gameNumber, true);
        var visitorForfeit = this.GetForfeit(gameNumber, false);

        return new GameRecord(visitorTeam, visitorScore, homeTeam, homeScore, date, field, time,
            visitorForfeit, homeForfeit);
    }

    private string GetGameDay()
    {
        var element = this.Driver.FindElement(this.ByDay);
        return element.Text;
    }

    private string GetField()
    {
        var element = this.Driver.FindElement(this.ByField);
        return element.Text;
    }

    private string GetTime(int gameNumber)
    {
        int index = gameNumber - 1;
        var element = this.Driver.FindElements(this.ByTime[index]);
        return element[1].Text;
    }

    private string GetTeam(int gameNumber, bool forHomeTeam)
    {
        int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
        var element = this.Driver.FindElements(this.ByTeam[index]);
        return element[1].Text;
    }
    #endregion
}
