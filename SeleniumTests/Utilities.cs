using HtmlAgilityPack;

namespace SeleniumTests;

public record GameRecord(
    string VisitorTeam, string VisitorScore, string HomeTeam, string HomeScore, string Date, string Field, string Time,
    bool VisitorForfeit = false, bool HomeForfeit = false, int gameID = 0);

public record StandingsRecord(
    string Team, string Wins, string Losses, string Ties, string WinPercentage, string GamesBehind,
    string RunsScored, string RunsAgainst, string Forfeits);

internal class Utilities
{
    internal static GameRecord HtmlToGameRecord(string htmlFromStandingsPage)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(htmlFromStandingsPage);

        var tdElements = doc.DocumentNode.SelectNodes("//td");

        // Team names are inside <a> tags.
        string visitorTeam = tdElements[0].InnerText.Trim();
        string homeTeam = tdElements[2].InnerText.Trim();

        string visitorScore = tdElements[1].InnerText.Trim();
        string homeScore = tdElements[3].InnerText.Trim();
        string date = tdElements[4].InnerText.Trim();
        string field = tdElements[5].InnerText.Trim();
        string time = tdElements[6].InnerText.Trim();

        return new GameRecord(visitorTeam, visitorScore, homeTeam, homeScore, date, field, time);
    }

    internal static StandingsRecord[] HtmlToStandingsRecord(string htmlFromStandingsPage)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(htmlFromStandingsPage);

        var records = new List<StandingsRecord>();
        var rows = doc.DocumentNode.SelectNodes("//table/tbody/tr");
        foreach (var row in rows)
        {
            var columns = row.SelectNodes("td");
            var record = new StandingsRecord(
                columns[0].InnerText.Trim(), // team name
                columns[1].InnerText.Trim(), // wins
                columns[2].InnerText.Trim(), // losses
                columns[3].InnerText.Trim(), // ties
                columns[4].InnerText.Trim(), // win %
                columns[5].InnerText.Trim(), // games behind
                columns[6].InnerText.Trim(), // runs scored
                columns[7].InnerText.Trim(), // runs against
                columns[8].InnerText.Trim() // forfeits
            );
            records.Add(record);
        }

        return records.ToArray();
    }

    internal static GameRecord[] GetTestGameRecords()
    {
        // Create and return game records with scores and forfeits for test purposes - all double-headers.
        // Note - this data must exactly match the test data cvs file.

        return new GameRecord[]
        {
            new GameRecord("Green", "6", "Royal Blue", "14", "Sep-10", "BR2", "9:30 AM", false, false, 18),
            new GameRecord("Royal Blue", "12", "Green", "15", "Sep-10", "BR2", "11:00 AM", false, false, 19),
            new GameRecord("Navy Blue", "7", "Light Blue", "12", "Sep-10", "BR3", "9:30 AM", false, false, 20),
            new GameRecord("Light Blue", "16", "Navy Blue", "10", "Sep-10", "BR3", "11:00 AM", false, false, 21),
            new GameRecord("Silver", "3", "Red", "9", "Sep-10", "RV1", "9:30 AM", false, false, 24),
            new GameRecord("Red", "17", "Silver", "6", "Sep-10", "RV1", "11:00 AM", false, false, 25),

            new GameRecord("Royal Blue", "9", "Silver", "14", "Sep-12", "BR3", "9:30 AM", false, false, 26),
            new GameRecord("Silver", "7", "Royal Blue", "0", "Sep-12", "BR3", "11:00 AM", false, true, 27),
            new GameRecord("Green", "11", "Dark Green", "17", "Sep-12", "BR5", "9:30 AM", false, false, 30),
            new GameRecord("Dark Green", "15", "Green", "15", "Sep-12", "BR5", "11:00 AM", false, false, 31),
            new GameRecord("Light Blue", "8", "Red", "14", "Sep-12", "BR6", "9:30 AM", false, false, 32),
            new GameRecord("Red", "12", "Light Blue", "1", "Sep-12", "BR6", "11:00 AM", false, false, 33),

            new GameRecord("Dark Green", "9", "Navy Blue", "16", "Sep-17", "BR3", "9:30 AM", false, false, 35),
            new GameRecord("Black", "19", "Gold", "11", "Sep-17", "NO2", "9:30 AM", false, false, 36),
            new GameRecord("Royal Blue", "17", "Red", "10", "Sep-17", "NO3", "9:30 AM", false, false, 37),
            
            // Note - scores for game below should be ignored due to double-forfeit;
            // comparing standings column for runs scored will confirm that scores are properly ignored.
            new GameRecord("Green", "999", "Light Blue", "888", "Sep-17", "RV1", "9:30 AM", true, true, 38)
        };
    }

    internal static StandingsRecord[] GetTestStandingsRecords()
    {
        // This data is what the standings results should be after the scores are reported
        // for ALL games defined in GetTestGameRecords().

        return new StandingsRecord[]
        {
            new StandingsRecord("Red", "4", "1", "0", ".800", "0", "62", "35", "0"),
            new StandingsRecord("Black", "1", "0", "0", "1.000", "1", "19", "11", "0"),
            new StandingsRecord("Silver", "2", "2", "0", ".500", "1.5", "30", "35", "0"),
            new StandingsRecord("Dark Green", "1", "1", "1", ".333", "1.5", "41", "42", "0"),
            new StandingsRecord("Light Blue", "2", "3", "0", ".400", "2", "37", "43", "1"),
            new StandingsRecord("Royal Blue", "2", "3", "0", ".400", "2", "52", "52", "1"),
            new StandingsRecord("Navy Blue", "1", "2", "0", ".333", "2", "33", "37", "0"),
            new StandingsRecord("Gold", "0", "1", "0", ".000", "2", "11", "19", "0"),
            new StandingsRecord("Green", "1", "3", "1", ".200", "2.5", "47", "58", "1")
        };
    }
}
