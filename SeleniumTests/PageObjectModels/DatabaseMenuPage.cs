using OpenQA.Selenium;

namespace SeleniumTests.PageObjectModels;

internal class DatabaseMenuPage
{
    #region Properties
    private IWebDriver Driver { get; init; }
    private string FullURL { get; init; }
    #endregion

    #region Read Only Constants

    #region Page Elements
    private readonly By ByCurrentDatabase = By.Id("currentDB");
    private readonly By ByCosmos = By.Id("selectCosmos");
    private readonly By BySqlServer = By.Id("selectSqlServer");
    private readonly By ByMySql = By.Id("selectMySql");
    private readonly By ByPosgreSql = By.Id("selectPostgreSql");
    #endregion
    #endregion

    internal DatabaseMenuPage(IWebDriver driver, string baseURL)
    {
        this.Driver = driver;
        this.FullURL = $"{baseURL}/";
        this.Load();
    }

    internal void Load()
    {
        this.Driver.Navigate().GoToUrl(this.FullURL);
    }

    #region Action Methods
    internal void SeclectDatabase(string database)
    {
        var currentDB = SeleniumWrapper.FindElementWithRetry(this.Driver, this.ByCurrentDatabase);
        if (currentDB!.Text.Trim().ToLower().Replace("current db: ", "") != database.ToLower())
        {
            SeleniumWrapper.ClickWithRetry(this.Driver, this.ByCurrentDatabase);

            switch (database.ToLower())
            {
                case "sqlserver":
                    SeleniumWrapper.ClickWithRetry(this.Driver, this.BySqlServer);
                    break;

                case "cosmos":
                    SeleniumWrapper.ClickWithRetry(this.Driver, this.ByCosmos);
                    break;

                case "mysql":
                    SeleniumWrapper.ClickWithRetry(this.Driver, this.ByMySql);
                    break;

                case "postgresql":
                    SeleniumWrapper.ClickWithRetry(this.Driver, this.ByPosgreSql);
                    break;
            }

            // Check to see if the switch worked.
            currentDB = SeleniumWrapper.FindElementWithRetry(this.Driver, this.ByCurrentDatabase);
            if (currentDB!.Text.Trim().ToLower().Replace("current db: ", "") != database.ToLower())
            {
                throw new Exception($"Unable to switch to {database}.");
            }
        }
    }
    #endregion
}
