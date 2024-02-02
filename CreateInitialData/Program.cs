using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using SeleniumTests;
using SeleniumTests.PageObjectModels;
using SeleniumTests.PageObjectModels.Admin;

namespace SbtMultiDB.CreateInitialData;

// note - these gh repo comments are meant for the to-be-created PUBLIC repo,
// okay to check everything in to my private version first


// NOTE TO SELF - NOT !!! part of GH Repo!!! - do not check in !!!
// (and if i ever do, too much duplicated code between here and selenium tests)
// (and probably lowercase comments, etc)

// - remove this project from solution before checking that in,


internal class Program
{
    static void Main(string[] args)
    {
        string nationalScheduleFile = "2019 NVSS National Fall SBT Schedule.csv";
        int[] nationalGameIDs = [1, 3, 5, 7, 10, 12, 14, 16, 19, 21];
        int[] nationalGameIDsHockey = [1, 2, 3, 4, 6, 7, 8, 9, 11, 12];
        string americanScheduleFile = "2019 NVSS American Fall SBT Schedule.csv";
        int[] americanGameIDs = [1, 3, 5, 7, 10, 12, 14, 16, 19, 21];
        int[] americanGameIDsHockey = [1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15, 16, 17, 19, 20, 21];
        string continentalScheduleFile = "2019 NVSS Continental Fall SBT Schedule.csv";
        int[] continentalGameIDs = [1, 3, 5, 7, 10, 12, 14, 16, 19, 21];
        int[] continentalGameIDsHockey = [1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15, 16, 17, 19, 20, 21];

        string baseURL = "https://localhost:7125";
        IWebDriver driver = default!;
        var firefoxOptions = new FirefoxOptions();
        firefoxOptions.AcceptInsecureCertificates = true;
        driver = new FirefoxDriver(firefoxOptions);

        string currentDirectory = Environment.CurrentDirectory;
        string relativeFilePath = Path.Combine("ScheduleFiles", nationalScheduleFile);
        string fullPath = Path.Combine(currentDirectory, relativeFilePath);
        int[] gameIDs = nationalGameIDs;
        int[] gameIDsHockey = nationalGameIDsHockey;

        //string localOrAzure = "Local ";
        string localOrAzure = "Azure "; // need to make sure my SbtMulti site is accessing remote servers
        string organization = "Demo Softball";
        string database = "Cosmos";
        string abbreviation = "CS01";
        string league = localOrAzure + database + " MultiDB";
        string nameOrNumber = "Softball Division 1";

        // cosmos
        LoadScheduleAndAddRandomScores(driver, baseURL, fullPath, gameIDs, gameIDsHockey, organization, abbreviation, database, league, nameOrNumber);
        // now load a 2nd division
        abbreviation = "CS02";
        nameOrNumber = "Softball Division 2";
        relativeFilePath = Path.Combine("ScheduleFiles", nationalScheduleFile);
        fullPath = Path.Combine(currentDirectory, relativeFilePath);
        gameIDs = nationalGameIDs;
        gameIDsHockey = nationalGameIDsHockey;
        LoadScheduleAndAddRandomScores(driver, baseURL, fullPath, gameIDs, gameIDsHockey, organization, abbreviation, database, league, nameOrNumber);
        // reset back to div 1
        nameOrNumber = "Softball Division 1";

        // todo - can load scores for remote servers now (see below) but I want to get
        // my test project working first just to be sure no errors occur from tests
        // i think i should add nunit tests here and re-familiarize myself with how these work again,
        // since the whole page model thing is now a mystery to me? maybe that was a razor page thing?
        // maybe I left myself a link somewhere here or on LI about what tutorial i followed for these???

        // looks like i am duping a lot of this code in standings page test - switch to use those methods?

        // note - can wait on below until unit tests above are done
        // todo - change to azure versions and do some quick manual tests, then can do loads
        // for azure servers (change localOrAzure string above first)
        // note - no need to do Postgre, already done

        // sql server
        database = "SqlServer";
        abbreviation = "SS01";
        league = localOrAzure + database + " MultiDB";
        relativeFilePath = Path.Combine("ScheduleFiles", americanScheduleFile);
        fullPath = Path.Combine(currentDirectory, relativeFilePath);
        gameIDs = americanGameIDs;
        gameIDsHockey = americanGameIDsHockey;
        LoadScheduleAndAddRandomScores(driver, baseURL, fullPath, gameIDs, gameIDsHockey, organization, abbreviation, database, league, nameOrNumber);
        // now load a 2nd division
        abbreviation = "SS02";
        nameOrNumber = "Softball Division 2";
        relativeFilePath = Path.Combine("ScheduleFiles", nationalScheduleFile);
        fullPath = Path.Combine(currentDirectory, relativeFilePath);
        gameIDs = nationalGameIDs;
        gameIDsHockey = nationalGameIDsHockey;
        LoadScheduleAndAddRandomScores(driver, baseURL, fullPath, gameIDs, gameIDsHockey, organization, abbreviation, database, league, nameOrNumber);
        // reset back to div 1
        nameOrNumber = "Softball Division 1";


        // mysql
        database = "MySql";
        abbreviation = "MS01";
        league = localOrAzure + database + " MultiDB";
        relativeFilePath = Path.Combine("ScheduleFiles", continentalScheduleFile);
        fullPath = Path.Combine(currentDirectory, relativeFilePath);
        gameIDs = continentalGameIDs;
        gameIDsHockey = continentalGameIDsHockey;
        //LoadScheduleAndAddRandomScores(driver, baseURL, fullPath, gameIDs, gameIDsHockey, organization, abbreviation, database, league, nameOrNumber);

        // postgre sql
        // note - no local server for postgre sql
        database = "PostgreSql";
        abbreviation = "PS01";
        league = "Azure " + database + " MultiDB";
        relativeFilePath = Path.Combine("ScheduleFiles", nationalScheduleFile);
        fullPath = Path.Combine(currentDirectory, relativeFilePath);
        gameIDs = nationalGameIDs;
        gameIDsHockey = nationalGameIDsHockey;
        //LoadScheduleAndAddRandomScores(driver, baseURL, fullPath, gameIDs, gameIDsHockey, organization, abbreviation, database, league, nameOrNumber);

        driver.Quit();
    }

    private static void LoadScheduleAndAddRandomScores(IWebDriver driver, string baseURL, string fullPath, int[] gameIDs, int[] gameIDsHockey,
        string organization, string abbreviation, string database, string league,string nameOrNumber)
    {
        if (File.Exists(fullPath))
        {
            // softball
            var dbPage = new DatabaseMenuPage(driver, baseURL);
            dbPage.SeclectDatabase(database);

            DeleteDivision(driver, baseURL, organization, abbreviation);
            CreateDivision(driver, baseURL, organization, abbreviation, league, nameOrNumber);
            LoadScheduleForDivision(driver, baseURL, organization, abbreviation, true, fullPath);

            var standingsPage = new StandingsPage(driver, baseURL, organization, abbreviation);
            EnterRandomScores(driver, standingsPage, gameIDs, true);

            // hockey, but need to change a few items first
            organization = organization.Replace("Softball", "Hockey");
            abbreviation = abbreviation[0] + "H" + abbreviation.Substring(2);
            nameOrNumber = nameOrNumber.Replace("Softball", "Hockey");
            DeleteDivision(driver, baseURL, organization, abbreviation);
            CreateDivision(driver, baseURL, organization, abbreviation, league, nameOrNumber);
            LoadScheduleForDivision(driver, baseURL, organization, abbreviation, false, fullPath);

            standingsPage = new StandingsPage(driver, baseURL, organization, abbreviation);
            EnterRandomScores(driver, standingsPage, gameIDsHockey, false);
        }
        else
        {
            Console.WriteLine("Unable to find file " + fullPath);
        }
    }

    private static void EnterRandomScores(IWebDriver driver, StandingsPage standingsPage, int[] gameIDs, bool isSoftball)
    {
        foreach (var gameID in gameIDs)
        {
            var reportScoresPage = standingsPage.ClickOnGameToReportScores(gameID);

            EnterGameRecord(GenerateRandomGameRecord(isSoftball), 1, reportScoresPage);
            if (isSoftball)
            {
                EnterGameRecord(GenerateRandomGameRecord(isSoftball), 2, reportScoresPage);
            }
            reportScoresPage.ClickSaveButton();
        }
    }

    private static GameRecord GenerateRandomGameRecord(bool isSoftball)
    {
        Random random = new Random();
        int min = isSoftball ? 4 : 1;
        int max = isSoftball ? 18 : 7;
        string homeScore = random.Next(min, max).ToString();
        string visitorScore = random.Next(min, max).ToString();
        bool homeForfeit = random.NextDouble() < 0.1;
        bool visitorForfeit = random.NextDouble() < 0.1;

        var gameRecord = new GameRecord("", visitorScore, "", homeScore , "", "", "", visitorForfeit, homeForfeit, 0);

        return gameRecord;
    }

    private static void EnterGameRecord(GameRecord gameRecord, int gameNumber, ReportScoresPage reportScoresPage)
    {
        if (gameRecord.HomeForfeit || gameRecord.VisitorForfeit)
        {
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
            reportScoresPage.EnterScore(gameNumber, gameRecord.VisitorScore, false);
            reportScoresPage.EnterScore(gameNumber, gameRecord.HomeScore, true);
        }
    }

    private static void LoadScheduleForDivision(IWebDriver driver, string baseURL, string organization, 
        string abbreviation, bool useDoubleHeaders, string scheduleFile)
    {
        LoadSchedulePage page = new LoadSchedulePage(driver, baseURL, organization);

        page.EnterDivisionAndScheduleInformation(abbreviation, scheduleFile);
        
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

    private static void CreateDivision(IWebDriver driver, string baseURL, string organization,
        string abbreviation, string league, string nameOrNumber)
    {
        CreateDivisionPage page = new CreateDivisionPage(driver, baseURL, organization);

        page.EnterDivisionInformation(abbreviation, league, nameOrNumber);
        page.ClickCreateButton();
    }

    private static void DeleteDivision(IWebDriver driver, string baseURL, string organization, string abbreviation)
    {
        DeleteDivisionPage page;
        try
        {
            page = new DeleteDivisionPage(driver, baseURL, organization, abbreviation);
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
}
