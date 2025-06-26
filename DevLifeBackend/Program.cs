// File: Program.cs
using DevLifeBackend.Data;
using DevLifeBackend.Endpoints;
using DevLifeBackend.Services;
using DevLifeBackend.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql; // Required for the specific exception handling
using OpenAI;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

// --- Services Configuration ---
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("DATABASE_URL")));
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = Environment.GetEnvironmentVariable("REDIS_URL");
    options.InstanceName = "DevLife_";
});
builder.Services.AddSignalR();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("HoroscopeClient", client => { client.Timeout = TimeSpan.FromSeconds(3); });
builder.Services.AddHttpClient("CodewarsClient", client => { client.DefaultRequestHeaders.Add("User-Agent", "DevLifePortal/1.0"); });
builder.Services.AddHttpClient("Judge0Client", client => {
    client.BaseAddress = new Uri("https://judge0-ce.p.rapidapi.com/");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", Environment.GetEnvironmentVariable("JUDGE0_API_KEY"));
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", Environment.GetEnvironmentVariable("JUDGE0_HOST"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// This line registers all our validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddHostedService<GameLoopService>();

// Register the OpenAI client
builder.Services.AddSingleton(new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));

// Register our custom services
builder.Services.AddScoped<IZodiacService, ZodiacService>();
builder.Services.AddScoped<IHoroscopeService, HoroscopeService>();
builder.Services.AddScoped<ICasinoService, CasinoService>();
builder.Services.AddScoped<ICodewarsService, CodewarsService>();
builder.Services.AddScoped<ICodeRoastService, CodeRoastService>();
builder.Services.AddScoped<IJudge0Service, Judge0Service>();
builder.Services.AddScoped<IAiSnippetGeneratorService, AiSnippetGeneratorService>();
builder.Services.AddScoped<IDailyFeatureService, DailyFeatureService>();
builder.Services.AddScoped<IDailyFeatureService, DailyFeatureService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IGitHubAnalyzerService, GitHubAnalyzerService>();
builder.Services.AddScoped<IImageService, ImageService>();


var app = builder.Build();

// --- Seeding and Middleware ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        // First, handle PostgreSQL migration separately and catch the specific error
        try
        {
            var postgresContext = services.GetRequiredService<ApplicationDbContext>();
            postgresContext.Database.Migrate();
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01") // 42P01 is the code for "undefined_table"
        {
            logger.LogWarning("Migration failed because a table was not found. This is expected if the table was already removed. Ignoring. Error: {message}", ex.Message);
        }

        // Second, seed MongoDB. This code will now always be reached.
        var mongoContext = services.GetRequiredService<MongoDbContext>();
        DevLifeBackend.Data.Seed.SeedData.Initialize(mongoContext);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An unexpected error occurred during seeding.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSession();

// --- API Endpoints Registration ---
app.MapGet("/", () => "DevLife API is running!");
app.MapAuthEndpoints();
app.MapDashboardEndpoints();
app.MapCasinoEndpoints();
app.MapRoastEndpoints();
app.MapAdminEndpoints();
app.MapHub<DevLifeBackend.Hubs.BugChaseHub>("/hubs/bugchase");
app.MapBugChaseEndpoints();
app.MapProfileEndpoints();
app.MapGitHubEndpoints();

app.Run();