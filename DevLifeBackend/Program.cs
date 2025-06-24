// File: Program.cs
using DevLifeBackend.Data;
using DevLifeBackend.Endpoints; // <-- Главный using для наших эндпоинтов
using DevLifeBackend.Services;
using DevLifeBackend.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenAI;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

// --- Services Configuration ---
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("DATABASE_URL")));
builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = Environment.GetEnvironmentVariable("REDIS_URL"); options.InstanceName = "DevLife_"; });
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; });

builder.Services.AddHttpClient("HoroscopeClient", client => { client.Timeout = TimeSpan.FromSeconds(3); });
builder.Services.AddHttpClient("CodewarsClient", client => { client.DefaultRequestHeaders.Add("User-Agent", "DevLifePortal/1.0"); });
builder.Services.AddHttpClient("Judge0Client", client => {
    client.BaseAddress = new Uri("https://judge0-ce.p.rapidapi.com/");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", Environment.GetEnvironmentVariable("JUDGE0_API_KEY"));
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", Environment.GetEnvironmentVariable("JUDGE0_HOST"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
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


var app = builder.Build();

// --- Seeding and Middleware ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        DevLifeBackend.Data.Seed.SeedData.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSession();

// --- API Endpoints Registration ---

app.MapAuthEndpoints();
app.MapDashboardEndpoints();
app.MapCasinoEndpoints();
app.MapRoastEndpoints();
app.MapAdminEndpoints();

app.Run();