namespace SbtMultiDB.Shared;

#nullable disable annotations

public class Settings
{
    public Settings() { }

    public AllowedDatabases AllowedDatabases { get; set; }
    public Email Email { get; set; }
}

public class AllowedDatabases
{
    public string IsCosmosAllowed { get; set; }
    public string IsSqlServerAllowed { get; set; }
    public string IsMySqlAllowed { get; set; }
    public string IsPostgreSqlAllowed { get; set; }
}

public class Email
{
    public string UseSendGridEmailService { get; set; }
    public string UseAzureEmailService { get; set; }
    public string SendToAddress { get; set; }
    public string SendToName { get; set; }
    public string FromAddress { get; set; }
    public string FromName { get; set; }
    public string Subject { get; set; }
    public string SendGridKey { get; set; }
    public string AzureEmailConnectionString { get; set; }
}
