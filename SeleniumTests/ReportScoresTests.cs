using NUnit.Framework;
using OpenQA.Selenium.Firefox;
using SeleniumTests.PageObjectModels;

namespace SeleniumTests;

internal class ReportScoresTests : PageTests
{
    #region Read Only Constants
    private readonly int FirstDoubleHeaderGameID = 1;
    private readonly int FirstSingleGameID = 36;
    #endregion

    [Test]
    public void CorrectGameInfoDisplayedForSingleGameTest()
    {
        // Verify that clicking on a team name (in a few places)
        // will move to the ReportScores Page and shows the correct game info:
        //    - verify that all read-only fields are correct;
        //    - verify that the scores are empty and the forfeit checkboxes are unchecked.

        TestGame(this.FirstSingleGameID);
        TestGame(this.FirstSingleGameID + 1);
        TestGame(this.FirstSingleGameID + 2);

        void TestGame(int gameID)
        {
            var gameRecords = this.StandingsPage.GetGameRecords(gameID, false);
            var reportScoresPage = this.StandingsPage.ClickOnGameToReportScores(gameID);

            var gameRecordActual = reportScoresPage.GetGameRecord(1);
            this.CompareGameRecords(gameRecordActual, gameRecords[0]);

            this.StandingsPage.Load();
        }
    }

    [Test]
    public void CorrectGameInfoDisplayedForDoubleHeaderTest()
    {
        // Verify that clicking on a team name (in a few places)
        // will move to the ReportScores Page and shows the correct game info:
        //    - verify that all read-only fields are correct;
        //    - verify that the scores are empty and the forfeit checkboxes are unchecked.
        // Do this for both games of doubleheader.

        TestGame(this.FirstDoubleHeaderGameID);
        TestGame(this.FirstDoubleHeaderGameID + 2);
        TestGame(this.FirstDoubleHeaderGameID + 4);

        TestGame(this.FirstDoubleHeaderGameID + 10);
        TestGame(this.FirstDoubleHeaderGameID + 12);
        TestGame(this.FirstDoubleHeaderGameID + 14);

        void TestGame(int gameID)
        {
            var gameRecords = this.StandingsPage.GetGameRecords(gameID, true);
            var reportScoresPage = this.StandingsPage.ClickOnGameToReportScores(gameID);

            var gameRecordActual = reportScoresPage.GetGameRecord(1);
            this.CompareGameRecords(gameRecordActual, gameRecords[0]);
            gameRecordActual = reportScoresPage.GetGameRecord(2);
            this.CompareGameRecords(gameRecordActual, gameRecords[1]);

            this.StandingsPage.Load();
        }
    }

    [Test]
    public void InvalidScoreReportForSingleGameTest()
    {
        // This method checks client-side validation, single-game scenario.

        // Check validation messages when an empty score report is attempted,
        // then enter a valid score, which should clear the validation message,
        // then enter an invalid (non-integer) score - validation message should say that.

        var gameID = this.FirstSingleGameID;
        var reportScoresPage = this.StandingsPage.ClickOnGameToReportScores(gameID);

        // Validation messages should be empty.
        var message = reportScoresPage.GetValidationMessage(1, true);
        Assert.That(message, Is.Empty);
        message = reportScoresPage.GetValidationMessage(1, false);
        Assert.That(message, Is.Empty);

        // Scores should be empty.
        var score = reportScoresPage.GetScore(1, true);
        Assert.That(score, Is.Empty);
        score = reportScoresPage.GetScore(1, false);
        Assert.That(score, Is.Empty);

        // Attempt to save score report ...
        // it should fail with valiation message due to empty scores.
        reportScoresPage.ClickSaveButton();
        message = reportScoresPage.GetValidationMessage(1, true);
        Assert.That(message, Contains.Substring("required"));
        message = reportScoresPage.GetValidationMessage(1, false);
        Assert.That(message, Contains.Substring("required"));

        // Entering a valid score should clear the validation message.
        reportScoresPage.EnterScore(1, "3", false);
        message = reportScoresPage.GetValidationMessage(1, false);
        Assert.That(message, Is.Empty);

        // Entering an invalid score should display a new and different validation message.
        reportScoresPage.EnterScore(1, "3.5", true);
        message = reportScoresPage.GetValidationMessage(1, true);
        Assert.That(message, Contains.Substring("integer"));
    }

    [Test]
    public void ScoreReportsWithJavaScriptDisabledTest()
    {
        // Test features with JS disabled:
        //    - turn off JavaScript (verify with forfeit checkbox click);
        //    - add scores then click a forfeit checkbox and save scores:
        //        (server-side should override reported scores with 7-0 forfeit score instead);
        //    - verify a game with no scores is not reported, and shows validation message.

        // IMPORTANT NOTE:
        // The switching of databases requires JS, so this method will work against
        // whichever database is the current one when the browser loads the site.
        // This test must re-create the test data to make sure it exists in the current database,
        // because the base class created the test data after switching databases.

        var gameID = this.FirstSingleGameID;
        var homeScore = "9";
        var visitorScore = "4";

        // Minimize the setup-created browser to avoid visual confusion.
        this.Driver.Manage().Window.Minimize();

        var firefoxOptions = new FirefoxOptions();
        firefoxOptions.AcceptInsecureCertificates = true;
        firefoxOptions.SetPreference("javascript.enabled", false);

        // Note - a new driver is being created (necessary to disable JS),
        // so a try-finally is needed to Quit() it.
        // (See note near top of method about which database to use.)
        var driverNoJs = new FirefoxDriver(firefoxOptions);
        base.InitializeTestData(driverNoJs);
        try
        {
            var standingsPage = new StandingsPage(driverNoJs, this.BaseURL, this.Organization, this.Abbreviation);
            var reportScoresPage = standingsPage.ClickOnGameToReportScores(gameID);

            // Confirm that JavaScript is disabled by adding scores, then
            // click on forfeit checkbox. Other tests have confirmed
            // that doing so should zero-out a score when JS is enabled.
            this.EnterAndVerrifyScore(1, false, visitorScore, reportScoresPage);
            this.EnterAndVerrifyScore(1, true, homeScore, reportScoresPage);
            reportScoresPage.ClickForfeitCheckbox(1, true);
            var score = reportScoresPage.GetScore(1, true);
            Assert.That(score, Is.EqualTo(homeScore));

            // Attempt to save scores, which should not work because
            // validataion flags the inconsistency between the forfiet 
            // checkbox and the scores.
            reportScoresPage.ClickSaveButton();

            // Verify that the driver is still on the Report Scores Page.
            Assert.That(reportScoresPage.IsDriverOnPage(), Is.True);

            // Go back to the Standings Page to set up for the next steps in the test.
            standingsPage.Load();

            // Verify a game with no scores is not reported, and shows validation message.
            gameID = this.FirstSingleGameID + 1;
            reportScoresPage = standingsPage.ClickOnGameToReportScores(gameID);

            // Clear out scores from prior test steps.
            this.EnterAndVerrifyScore(1, false, string.Empty, reportScoresPage);
            this.EnterAndVerrifyScore(1, true, string.Empty, reportScoresPage);

            // Verify empty validation messages.
            var message = reportScoresPage.GetValidationMessage(1, true);
            Assert.That(message, Is.Empty);
            message = reportScoresPage.GetValidationMessage(1, false);
            Assert.That(message, Is.Empty);
            reportScoresPage.ClickSaveButton();

            // Now validation messages should not be empty.
            message = reportScoresPage.GetValidationMessage(1, true);
            Assert.That(message, Contains.Substring("required"));
            message = reportScoresPage.GetValidationMessage(1, false);
            Assert.That(message, Contains.Substring("required"));
        }
        finally
        {
            driverNoJs.Quit();
        }
    }

    [Test]
    public void InvalidScoreReportdForDoubleHeaderTest()
    {
        // This method checks client-side validation, double-header scenario.

        // Check validation messages when an empty score report is attempted,
        // then enter a valid score, which should clear the validation message,
        // then enter an invalid (non-integer) score - validation message should say that.

        var gameID = this.FirstDoubleHeaderGameID;
        var reportScoresPage = this.StandingsPage.ClickOnGameToReportScores(gameID);

        // Validation messages should be empty.
        var message = reportScoresPage.GetValidationMessage(1, true);
        Assert.That(message, Is.Empty);
        message = reportScoresPage.GetValidationMessage(1, false);
        Assert.That(message, Is.Empty);

        // Check game 2.
        message = reportScoresPage.GetValidationMessage(2, true);
        Assert.That(message, Is.Empty);
        message = reportScoresPage.GetValidationMessage(2, false);
        Assert.That(message, Is.Empty);

        // Scores should be empty.
        var score = reportScoresPage.GetScore(1, true);
        Assert.That(score, Is.Empty);
        score = reportScoresPage.GetScore(1, false);
        Assert.That(score, Is.Empty);

        // Check game 2.
        score = reportScoresPage.GetScore(2, true);
        Assert.That(score, Is.Empty);
        score = reportScoresPage.GetScore(2, false);
        Assert.That(score, Is.Empty);

        // Attempt to save score report ...
        // it should fail with valiation message due to empty scores.
        reportScoresPage.ClickSaveButton();
        message = reportScoresPage.GetValidationMessage(1, true);
        Assert.That(message, Contains.Substring("required"));
        message = reportScoresPage.GetValidationMessage(1, false);
        Assert.That(message, Contains.Substring("required"));

        // Check game 2.
        message = reportScoresPage.GetValidationMessage(2, true);
        Assert.That(message, Contains.Substring("required"));
        message = reportScoresPage.GetValidationMessage(2, false);
        Assert.That(message, Contains.Substring("required"));

        // Entering a valid score (game 1) should clear the validation message.
        reportScoresPage.EnterScore(1, "3", false);

        message = reportScoresPage.GetValidationMessage(1, false);
        Assert.That(message, Is.Empty);
        message = reportScoresPage.GetValidationMessage(1, true);
        Assert.That(message, Contains.Substring("required"));

        // Double-header specific: scores entered for one game should not clear
        // validation messages for the other game (scores should remain empty as well).
        // Check game 2.
        message = reportScoresPage.GetValidationMessage(2, true);
        Assert.That(message, Contains.Substring("required"));
        message = reportScoresPage.GetValidationMessage(2, false);
        Assert.That(message, Contains.Substring("required"));

        // Check game 2.
        score = reportScoresPage.GetScore(2, true);
        Assert.That(score, Is.Empty);
        score = reportScoresPage.GetScore(2, false);
        Assert.That(score, Is.Empty);

        // Entering an invalid score should display a new and different validation message.
        // Use game 2 home team for this, then verify game 1 did not change.
        reportScoresPage.EnterScore(2, "3.5", true);

        message = reportScoresPage.GetValidationMessage(2, true);
        Assert.That(message, Contains.Substring("integer"));
        message = reportScoresPage.GetValidationMessage(2, false);
        Assert.That(message, Contains.Substring("required"));
        score = reportScoresPage.GetScore(2, false);
        Assert.That(score, Is.Empty);

        // Check game 1.
        message = reportScoresPage.GetValidationMessage(1, true);
        Assert.That(message, Contains.Substring("required"));
        score = reportScoresPage.GetScore(1, true);
        Assert.That(score, Is.Empty);
        message = reportScoresPage.GetValidationMessage(1, false);
        Assert.That(message, Is.Empty);
        score = reportScoresPage.GetScore(1, false);
        Assert.That(score, Is.EqualTo("3"));
    }

    [Test]
    public void ForfeitCheckboxChangesScoresTest()
    {
        // This test verifies that the javascript code for the forfeit checkboxes
        // correctly zeroes out the appropriate score text box(es).
        // This will only test a doubleheader since it is the more complicated test.

        var gameID = this.FirstDoubleHeaderGameID;
        var reportScoresPage = this.StandingsPage.ClickOnGameToReportScores(gameID);

        // Enter scores for both games, verifying along the way.
        EnterAndVerrifyScore(1, false, "3", reportScoresPage);
        EnterAndVerrifyScore(1, true, "8", reportScoresPage);
        EnterAndVerrifyScore(2, false, "15", reportScoresPage);
        EnterAndVerrifyScore(2, true, "12", reportScoresPage);

        // Click forfeit check box for visitor in game 1, verify click,
        // then verify that scores are changed appropriately (and only for game 1).
        reportScoresPage.ClickForfeitCheckbox(1, false);
        var forfeit = reportScoresPage.GetForfeit(1, false);
        Assert.That(forfeit, Is.EqualTo(true));
        var score = reportScoresPage.GetScore(1, false);
        Assert.That(score, Is.EqualTo("0"));
        score = reportScoresPage.GetScore(1, true);
        Assert.That(score, Is.EqualTo("7"));
        score = reportScoresPage.GetScore(2, false);
        Assert.That(score, Is.EqualTo("15"));
        score = reportScoresPage.GetScore(2, true);
        Assert.That(score, Is.EqualTo("12"));

        // Repeat for home in game 2.
        reportScoresPage.ClickForfeitCheckbox(2, true);
        forfeit = reportScoresPage.GetForfeit(2, true);
        Assert.That(forfeit, Is.EqualTo(true));
        score = reportScoresPage.GetScore(1, false);
        Assert.That(score, Is.EqualTo("0"));
        score = reportScoresPage.GetScore(1, true);
        Assert.That(score, Is.EqualTo("7"));
        score = reportScoresPage.GetScore(2, false);
        Assert.That(score, Is.EqualTo("7"));
        score = reportScoresPage.GetScore(2, true);
        Assert.That(score, Is.EqualTo("0"));

        // One more time, for visitor for game 2.
        // Note that both scores are zero for a double-forfeit.
        reportScoresPage.ClickForfeitCheckbox(2, false);
        forfeit = reportScoresPage.GetForfeit(2, false);
        Assert.That(forfeit, Is.EqualTo(true));
        score = reportScoresPage.GetScore(1, false);
        Assert.That(score, Is.EqualTo("0"));
        score = reportScoresPage.GetScore(1, true);
        Assert.That(score, Is.EqualTo("7"));
        score = reportScoresPage.GetScore(2, false);
        Assert.That(score, Is.EqualTo("0"));
        score = reportScoresPage.GetScore(2, true);
        Assert.That(score, Is.EqualTo("0"));
    }

    [Test]
    public void CorrectGameInfoDisplayedWithScoresReportedTest()
    {
        // Verify that doubleheader games with scores reported
        // display correct information.

        var testGameRecords = Utilities.GetTestGameRecords();
        int index = 0;
        var gameID = testGameRecords[index].gameID;
        var reportScoresPage = this.StandingsPage.ClickOnGameToReportScores(gameID);

        this.EnterAndVerifyGameRecord(testGameRecords[index], 1, reportScoresPage);
        this.EnterAndVerifyGameRecord(testGameRecords[index + 1], 2, reportScoresPage);
        reportScoresPage.ClickSaveButton();

        // Verify that the driver is now on the Standings Page.
        Assert.That(this.StandingsPage.IsDriverOnPage(), Is.True);

        // Verify that the standings page shows the scores we just reported for both games.
        var gameRecords = this.StandingsPage.GetGameRecords(gameID, true);
        this.CompareGameRecords(gameRecords[0], testGameRecords[index], true);
        this.CompareGameRecords(gameRecords[1], testGameRecords[index + 1], true);
    }

    [Test]
    public void ScoresCorrectlyReflectedInStandingsTest()
    {
        // Report scores for several games, with and without forfeits, and
        // verify the standings were properly calculated. 

        var testGameRecords = Utilities.GetTestGameRecords();
        for (int index = 0; index < testGameRecords.Length; index = index + 2)
        {

            var gameID = testGameRecords[index].gameID;
            var reportScoresPage = this.StandingsPage.ClickOnGameToReportScores(gameID);

            this.EnterAndVerifyGameRecord(testGameRecords[index], 1, reportScoresPage);
            if (index < testGameRecords.Length - 4)
            {
                // Final 3 records are for single games, so no Game 2.
                this.EnterAndVerifyGameRecord(testGameRecords[index + 1], 2, reportScoresPage);
            }
            else
            {
                // Single games in final 3 records, but loop is advancing by 2,
                // so decrement here to allow for that advance.
                index--;
            }
            reportScoresPage.ClickSaveButton();
        }

        // Verify that the driver is now on the Standings Page.
        Assert.That(this.StandingsPage.IsDriverOnPage(), Is.True);

        // Finished reporting scores, now verify standings.

        // Get standings records from standings page, and test data for comparison.
        var standingsRecordsExpected = Utilities.GetTestStandingsRecords();
        var standingsRecordsActual = this.StandingsPage.GetStandingsRecords();
        Assert.That(standingsRecordsActual.Length, Is.EqualTo(standingsRecordsExpected.Length));

        // Loop through and compare each index in the two arrays.
        for (int index = 0; (index < standingsRecordsActual.Length); index++)
        {
            this.CompareStandingsRecord(standingsRecordsActual[index], standingsRecordsExpected[index]);
        }
    }

    #region Helper Methods
    private void EnterAndVerifyGameRecord(GameRecord gameRecord, int gameNumber, ReportScoresPage reportScoresPage)
    {
        if (gameRecord.HomeForfeit || gameRecord.VisitorForfeit)
        {
            // We will enter scores even though one or both may be overridden by clicking on forfeit.
            this.EnterAndVerrifyScore(gameNumber, false, gameRecord.VisitorScore, reportScoresPage);
            this.EnterAndVerrifyScore(gameNumber, true, gameRecord.HomeScore, reportScoresPage);
            if (gameRecord.HomeForfeit)
            {
                reportScoresPage.ClickForfeitCheckbox(gameNumber, true);
            }
            if (gameRecord.VisitorForfeit)
            {
                reportScoresPage.ClickForfeitCheckbox(gameNumber, false);
            }
        }
        else
        {
            this.EnterAndVerrifyScore(gameNumber, false, gameRecord.VisitorScore, reportScoresPage);
            this.EnterAndVerrifyScore(gameNumber, true, gameRecord.HomeScore, reportScoresPage);
        }
    }

    private void EnterAndVerrifyScore(int gameNumber, bool home, string scoreToReport, ReportScoresPage reportScoresPage)
    {
        reportScoresPage.EnterScore(gameNumber, scoreToReport, home);
        var score = reportScoresPage.GetScore(gameNumber, home);
        Assert.That(score, Is.EqualTo(scoreToReport));
    }

    private void CompareGameRecords(GameRecord gameRecordActual, GameRecord gameRecordExpected, bool ignoreForfeits = false)
    {
        Assert.That(gameRecordActual.Date, Is.EqualTo(gameRecordExpected.Date));
        Assert.That(gameRecordActual.Field, Is.EqualTo(gameRecordExpected.Field));
        Assert.That(gameRecordActual.Time, Is.EqualTo(gameRecordExpected.Time));
        Assert.That(gameRecordActual.HomeTeam, Is.EqualTo(gameRecordExpected.HomeTeam));
        Assert.That(gameRecordActual.VisitorTeam, Is.EqualTo(gameRecordExpected.VisitorTeam));
        Assert.That(gameRecordActual.HomeScore, Is.EqualTo(gameRecordExpected.HomeScore));
        Assert.That(gameRecordActual.VisitorScore, Is.EqualTo(gameRecordExpected.VisitorScore));
        if (ignoreForfeits == false)
        {
            // Standings page does not show forfeits.
            Assert.That(gameRecordActual.HomeForfeit, Is.EqualTo(gameRecordExpected.HomeForfeit));
            Assert.That(gameRecordActual.VisitorForfeit, Is.EqualTo(gameRecordExpected.VisitorForfeit));
        }
    }

    private void CompareStandingsRecord(StandingsRecord recordActual, StandingsRecord recordExpected)
    {
        Assert.That(recordActual.Team, Is.EqualTo(recordExpected.Team));
        Assert.That(recordActual.Wins, Is.EqualTo(recordExpected.Wins));
        Assert.That(recordActual.Losses, Is.EqualTo(recordExpected.Losses));
        Assert.That(recordActual.Ties, Is.EqualTo(recordExpected.Ties));
        Assert.That(recordActual.WinPercentage, Is.EqualTo(recordExpected.WinPercentage));
        Assert.That(recordActual.GamesBehind, Is.EqualTo(recordExpected.GamesBehind));
        Assert.That(recordActual.RunsScored, Is.EqualTo(recordExpected.RunsScored));
        Assert.That(recordActual.RunsAgainst, Is.EqualTo(recordExpected.RunsAgainst));
        Assert.That(recordActual.Forfeits, Is.EqualTo(recordExpected.Forfeits));
    }
    #endregion
}
