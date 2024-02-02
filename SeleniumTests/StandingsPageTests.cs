using NUnit.Framework;

namespace SeleniumTests;

public class StandingsPageTests : PageTests
{
    #region Read Only Constants
    private readonly int MaxNumberOfGames = 39;
    private readonly string AlLTeams = "All Teams";
    private readonly string TeamNameToVerifyScheduleGameCount = "Green";
    private readonly int ScheduleGameCountForOneTeam = 9;
    #endregion

    [Test]
    public void StandingsPageAfterInitializationTest()
    {
        // Test the page immediately after data was created to make sure it is empty
        // (no scores reported, standings are all zero values, updated time very close to now).

        // Verify data was updated within the last minute.
        var updated = this.StandingsPage.GetUpdated();
        var diff = this.GetEasternTime() - updated;
        Assert.IsTrue(Math.Abs(diff.Seconds) < 60,
            "Data not recently updated. Difference found was " + diff.Seconds + " seconds.");

        // Check for all zero values in standings.
        for (int i = 2; i < 10; i++)
        {
            Assert.IsTrue(this.StandingsPage.IsStandingsColumnZero(i, i == 5),
                "Column " + i + " not all zero values in Standings Table.");
        }

        // Schedule should have no scores reported.
        Assert.IsTrue(this.StandingsPage.AreAllScoresEmpty(), "Schedule has at least one score reported.");

        // Number of games should match expected count.
        Assert.That(actual: this.StandingsPage.GetNumberOfGamesInSchedule(), Is.EqualTo(expected: this.MaxNumberOfGames),
            "Number of games does not match expected value.");
    }

    [Test]
    public void StandingsPageFilterByTeamTest()
    {
        // Verify ability to display the schedule for a single team and back again to all teams.

        // Switch to a team, verify the team was selected and that fewer games are displayed.
        var green = this.StandingsPage.SelectTeamInDropdownList(this.TeamNameToVerifyScheduleGameCount)
            .GetSelectedTeam();
        Assert.That(actual: green, Is.EqualTo(expected: this.TeamNameToVerifyScheduleGameCount));
        Assert.That(actual: this.StandingsPage.GetNumberOfGamesInSchedule(),
            Is.EqualTo(expected: this.ScheduleGameCountForOneTeam));

        // Reset to all teams. verify and count games again.
        var allTeams = this.StandingsPage.SelectTeamInDropdownList(this.AlLTeams).GetSelectedTeam();
        Assert.That(allTeams, Is.EqualTo(this.AlLTeams));
        Assert.That(this.StandingsPage.GetNumberOfGamesInSchedule(),
            Is.EqualTo(this.MaxNumberOfGames));
    }

    #region Helper Method
    private DateTime GetEasternTime()
    {
        DateTime utcTime = DateTime.UtcNow;

        TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternTimeZone);
    }
    #endregion
}
