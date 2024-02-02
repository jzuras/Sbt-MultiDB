using Azure;
using Azure.Communication.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using SbtMultiDB.Data.Repositories;
using SbtMultiDB.Models.ViewModels;
using SbtMultiDB.Shared;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Diagnostics;

namespace SbtMultiDB.Controllers;

public class HomeController : Controller
{
    #region Properties
    private ILogger<HomeController> Logger { get; init; }
    private IFeatureManager FeatureManager { get; init; }
    private Settings Settings { get; init; }
    private IHttpContextAccessor HttpContextAccessor { get; init; }
    #endregion

    public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor,
        IFeatureManager featureManager, IOptionsSnapshot<Settings> options)
    {
        this.Logger = logger;
        this.HttpContextAccessor = httpContextAccessor;
        this.Settings = options.Value;
        this.FeatureManager = featureManager;
    
        this.Logger.LogTraceExt("HomeController initialized.");
    }

    // Get
    public async Task<IActionResult> Index()
    {
        this.Logger.LogInformationExt("Index() called.");
        this.HttpContext.Session.ClearCurrentDatabaseIsUnavailableFlag();
        
        var ipv4Address = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        var referringUrl = HttpContext.Request.Headers.TryGetValue("Referer", out var refererValues)
            ? refererValues.ToString() : null;

        this.Logger.LogInformationExt($"Home Controller->Index() - Calling IPv4: {ipv4Address} referringURL: {referringUrl}.");
        
        await this.SendEmail("Sbt MultiDB App", $"Home Page accessed. Calling IPv4: {ipv4Address} referringURL: {referringUrl}.");

        return View();
    }

    // Post
    [Route("ChangeDatabase")]
    public async Task<IActionResult> ChangeDatabase(string? database)
    {
        // This method handles the menu option for switching databases at runtime,
        // which is stored in Session State.
        // No action is taken if the DB is the same,
        // otherwise store the new DB in Session State and redirect to Home Page.

        this.Logger.LogInformationExt("ChangeDatabase() - switch to database: '" + database + "'");

        if (string.IsNullOrEmpty(database))
        {
            return NoContent();
        }

        var repository = IDivisionRepository.RepositoryTypeFromString(database, useDefault: true);

        IDivisionRepository.RepositoryType? currentDatabase = null;
        ISession? session = null;
        
        // Get currently selected database from Session State.
        if (this.HttpContextAccessor.HttpContext != null)
        {
            session = this.HttpContextAccessor.HttpContext.Session;
            currentDatabase = session.CurrentDatabase();
        }

        this.Logger.LogInformationExt("ChangeDatabase() - database from Session: '" + currentDatabase.ToString() + "'");
  
        // Do nothing if no change from current DB.
        if (repository == currentDatabase)
        {
            return NoContent();
        }

        // Store the new DB and redirect to Home page because the current
        // page may be inconsistent with the new DB (e.g., division does not exist).
        if (session != null)
        {
            session.CurrentDatabase(repository);
        }

        var ipv4Address = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        await this.SendEmail("Sbt MultiDB App", 
            $"In ChangeDatabase() - Change to db: {database}, Session db: {currentDatabase.ToString()}. Calling IPv4: {ipv4Address}.");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Route("InitializeSessionState")] // AJAX call
    public IActionResult InitializeSessionState(string? database)
    {
        this.Logger.LogInformationExt("InitializeSessionState() - database from localStorage is '" + database + "'");

        // This method is called via AJAX from JS (in DOM loaded listener)
        // to allow initializing the Session State from local storage
        // if the Session State is empty.
        
        // Returning "true" tells JS to refresh the page because the
        // page was originally loaded with an empty Session State.

        var session = HttpContext.Session;
        var currentDatabase = session.CurrentDatabase(useDefault: false);

        if (currentDatabase == IDivisionRepository.RepositoryType.Uninitialized)
        {
            this.Logger.LogInformationExt("InitializeSessionState() - session state was Uninitialized, setting it to " + database);

            // Make sure the database from local storage is valid, use default if not.
            var repository = IDivisionRepository.RepositoryTypeFromString( 
                database ?? "", useDefault: true );

            this.Logger.LogInformationExt("InitializeSessionState() - forcing reload after setting session db to '" + repository + "'");

            session.CurrentDatabase(repository);
            return Json(true);
        }

        return Json(false);
    }

    // Get
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    #region Helper Method
    /// <summary>
    /// Uses SendGrid API and/or Azure Email Service to send an email using configuration data.
    /// </summary>
    /// <param name="subject">Subject of the email.</param>
    /// <param name="message">Body of the email.</param>
    private async Task SendEmail(string subject, string message)
    {
        if (await this.FeatureManager.IsEnabledAsync(FeatureFlags.SendEmails))
        {
            this.Logger.LogInformationExt("SendEmail() called, feature enabled.");

            if (this.Settings.Email.SendToAddress != null)
            {
                if (this.Settings.Email.UseAzureEmailService == "true")
                {
                    #region Azure Email Service
                    var emailConnectionString = this.Settings.Email.AzureEmailConnectionString;
                    var emailClient = new EmailClient(emailConnectionString);

                    try
                    {
                        EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                            WaitUntil.Started,
                            senderAddress: "DoNotReply@zuras.com",
                            recipientAddress: this.Settings.Email.SendToAddress,
                            subject: this.Settings.Email.Subject,
                            htmlContent: message,
                            plainTextContent: message);
                        this.Logger.LogInformationExt($"Azure email sent.");
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogWarningExt(ex, "Exception attempting to send email with Azure.");
                    }
                    #endregion
                }

                if (this.Settings.Email.UseSendGridEmailService == "true")
                {
                    #region SendGrid Email Service
                    var apiKey = this.Settings.Email.SendGridKey;
                    var client = new SendGridClient(apiKey);
                    var msg = new SendGridMessage()
                    {
                        From = new SendGrid.Helpers.Mail.EmailAddress(this.Settings.Email.FromAddress, this.Settings.Email.FromName),
                        Subject = this.Settings.Email.Subject,
                        PlainTextContent = message
                    };
                    msg.AddTo(new SendGrid.Helpers.Mail.EmailAddress(this.Settings.Email.SendToAddress, this.Settings.Email.SendToName));

                    try
                    {
                        var response = await client.SendEmailAsync(msg);
                        this.Logger.LogInformationExt($"SendGrid email sent - status code: {response.StatusCode}.");
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogWarningExt(ex, "Exception attempting to send email with SendGrid.");
                    }
                    #endregion
                }
            }
        }
        else
        {
            this.Logger.LogInformationExt("SendEmail() called, feature disabled.");
        }
    }
    #endregion
}
