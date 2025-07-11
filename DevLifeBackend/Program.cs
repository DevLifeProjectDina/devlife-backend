using DevLifeBackend.Data;
using DevLifeBackend.Endpoints;
using DevLifeBackend.Extensions;
using DevLifeBackend.Hubs;
using DevLifeBackend.Services;
using DevLifeBackend.Settings;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenAI;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/devlife-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

Log.Information("Starting up DevLife Portal");

try
{
    DotNetEnv.Env.Load();
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("DATABASE_URL")));
    builder.Services.AddSingleton<MongoDbContext>();
    builder.Services.AddStackExchangeRedisCache(options => {
        options.Configuration = Environment.GetEnvironmentVariable("REDIS_URL");
        options.InstanceName = "DevLife_";
    });
    builder.Services.AddSession(options => {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
    builder.Services.AddSignalR();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddApplicationServices();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            var postgresContext = services.GetRequiredService<ApplicationDbContext>();
            postgresContext.Database.Migrate();

            var mongoContext = services.GetRequiredService<MongoDbContext>();
            DevLifeBackend.Data.Seed.SeedData.Initialize(mongoContext);
            logger.LogInformation("Database seeding routines completed.");
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01")
        {
            logger.LogWarning("Migration failed because a table was not found. This is expected if the table was already removed. Ignoring. Error: {message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during startup seeding.");
        }
    }

  
    app.UseSwagger();
    app.UseSwaggerUI();
    

    app.UseSession();
    app.UseSerilogRequestLogging();

    app.MapGet("/", () => "DevLife API is running!");
    app.MapAuthEndpoints();
    app.MapDashboardEndpoints();
    app.MapCasinoEndpoints();
    app.MapRoastEndpoints();
    app.MapAdminEndpoints();
    app.MapBugChaseEndpoints();
    app.MapProfileEndpoints();
    app.MapGitHubEndpoints();
    app.MapExcuseEndpoints(); 
    app.MapDatingEndpoints(); 

    app.MapHub<BugChaseHub>("/hubs/bugchase");

    Log.Information("Application configuration complete. Starting web host.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}