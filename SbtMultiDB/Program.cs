using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using SbtMultiDB.Data;
using SbtMultiDB.Data.Repositories;
using SbtMultiDB.Services;
using SbtMultiDB.Shared;

namespace SbtMultiDB;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

        AddDatabaseItems(builder);

        builder.Services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new CustomViewLocationExpander());
        });

        builder.Services.AddSingleton<DatabaseAvailabilityService>();
        builder.Services.AddScoped<DatabaseAvailabilityFilter>();

        builder.Services.AddScoped<SetCurrentOrganizationActionFilter>();

        builder.Services.AddControllersWithViews(options =>
        {
            options.Filters.Add(typeof(SetCurrentOrganizationActionFilter));
            options.Filters.AddService<DatabaseAvailabilityFilter>();
        });

        builder.Services.AddAzureAppConfiguration();

        // Load configuration from Azure App Configuration.
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options.Connect(builder.Configuration["ConnectionStrings:AppConfig"])
                   // Load all keys that start with 'SbtMultiDB:'.
                   .Select("SbtMultiDB:*")
                   // Reload configuration if the registered sentinel key is modified.
                   .ConfigureRefresh(refreshOptions =>
                        refreshOptions.Register("SbtMultiDB:Settings:Sentinel", refreshAll: true));

            options.UseFeatureFlags(featureFlagOptions =>
            {
                featureFlagOptions.Select("SbtMultiDB:*");
            });
        });

        builder.Services.Configure<Settings>(builder.Configuration.GetSection("SbtMultiDB:Settings"));

        // Feature Management must be after Azure App Configuration so local settings takes precedence.
        builder.Services.AddFeatureManagement();

        // Override Azure App Configuration with local appsettings for development environment.
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            LoggingExtensions.UseColoring = true;
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSession();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAzureAppConfiguration();

        app.UseRouting();
        app.UseSession(new SessionOptions()
        {
            Cookie = new CookieBuilder()
            {
                Name = ".AspNetCore.Session.SbtMultiDB"
            }
        });

        app.UseAuthorization();

        // Note - there are explicitly-defined routes on action methods in
        // both the Admin and Divisions controllers, used to process
        // the Remote attribute on model properties.
        app.MapControllerRoute(
            name: "AdminLoadSchedule",
            pattern: "Admin/LoadSchedule/{organization}",
            defaults: new { controller = "Admin", action = "LoadSchedule" });

        app.MapControllerRoute(
            name: "AdminDivisionsList",
            pattern: "Admin/Divisions/{organization}",
            defaults: new { controller = "Divisions", action = "Index" });

        app.MapControllerRoute(
            name: "AdminDivisionsAction",
            pattern: "Admin/Divisions/{action}/{organization}/{abbreviation?}",
            defaults: new { controller = "Divisions" });

        app.MapControllerRoute(
            name: "Admin",
            pattern: "Admin/{organization?}",
            defaults: new { controller = "Admin", action = "Index" });

        app.MapControllerRoute(
            name: "StandingsList",
            pattern: "{organization}",
            defaults: new { controller = "StandingsList", action = "Index" });

        app.MapControllerRoute(
            name: "Scores",
            pattern: "{organization}/{abbreviation}/{gameID:int}",
            defaults: new { controller = "Scores", action = "Index" });

        app.MapControllerRoute(
            name: "Standings",
            pattern: "{organization}/{abbreviation}/{teamName?}",
            defaults: new { controller = "Standings", action = "Index" });

        app.MapControllerRoute(
            name: "custom",
            pattern: "{controller}/{action}/{organization?}/{abbreviation?}",
            defaults: new { controller = "Home", action = "Index" });

        app.Run();
    }

    private static void AddDatabaseItems(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<CosmosRepository>();
        builder.Services.AddScoped<SqlServerRepository>();
        builder.Services.AddScoped<MySqlRepository>();
        builder.Services.AddScoped<PostgreSqlRepository>();

        builder.Services.AddScoped<IDivisionRepositoryFactory, DivisionRepositoryFactory>();

        builder.Services.AddScoped<IDivisionService, DivisionService>();

        builder.Services.AddDbContext<SqlServerContext>(options =>
        {
            string sqlServerConnectionString = builder.Configuration.GetConnectionString("Azure_Sql_ConnectionString")
                ?? throw new InvalidOperationException("SqlServer connection string not found in configuration.");

            options.UseSqlServer(sqlServerConnectionString);
        });

        builder.Services.AddDbContext<CosmosContext>(options =>
        {
            string cosmosConnectionString = builder.Configuration.GetConnectionString("Azure_Cosmos_ConnectionString")
                ?? throw new InvalidOperationException("Cosmos connection string not found in configuration.");

            // to stay within the free limits for Cosmos DB,
            // I am sharing the "database" and Container with the EF Project.
            options.UseCosmos(cosmosConnectionString, databaseName: "Sbt-EF");
        });

        builder.Services.AddDbContext<MySqlContext>(options =>
        {
            string mysqlConnectionString = builder.Configuration.GetConnectionString("Azure_MySql_ConnectionString")
                ?? throw new InvalidOperationException("MySql connection string not found in configuration.");

            options.UseMySQL(mysqlConnectionString);
        });

        builder.Services.AddDbContext<PostgreSqlContext>(options =>
        {
            // note to self - no local install for postgre
            string postgreSqlConnectionString = builder.Configuration.GetConnectionString("Azure_PostgreSql_ConnectionString")
                ?? throw new InvalidOperationException("PostgreSql connection string not found in configuration.");

            options.UseNpgsql(postgreSqlConnectionString);
        });
    }
}
