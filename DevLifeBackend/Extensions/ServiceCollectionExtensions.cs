
using DevLifeBackend.Services;
using DevLifeBackend.Settings;
using FluentValidation;
using Microsoft.Extensions.Options;
using OpenAI;
using SixLabors.ImageSharp;

namespace DevLifeBackend.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            services.AddHostedService<GameLoopService>();
            services.AddSingleton(new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
            services.AddHttpClient("HoroscopeClient", client => { client.Timeout = TimeSpan.FromSeconds(3); });
            services.AddHttpClient("CodewarsClient", client => { client.DefaultRequestHeaders.Add("User-Agent", "DevLifePortal/1.0"); });
            services.AddHttpClient("Judge0Client", (serviceProvider, client) => {
                var judge0Settings = serviceProvider.GetRequiredService<IOptions<Judge0Settings>>().Value;
                client.BaseAddress = new Uri(judge0Settings.BaseUrl);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", Environment.GetEnvironmentVariable("JUDGE0_API_KEY"));
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", Environment.GetEnvironmentVariable("JUDGE0_HOST"));
            });

            services.AddScoped<IZodiacService, ZodiacService>();
            services.AddScoped<IHoroscopeService, HoroscopeService>();
            services.AddScoped<ICasinoService, CasinoService>();
            services.AddScoped<ICodewarsService, CodewarsService>();
            services.AddScoped<ICodeRoastService, CodeRoastService>();
            services.AddScoped<IJudge0Service, Judge0Service>();
            services.AddScoped<IAiSnippetGeneratorService, AiSnippetGeneratorService>();
            services.AddScoped<IDailyFeatureService, DailyFeatureService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IExcuseService, ExcuseService>();
            services.AddScoped<IDatingService, DatingService>();
            services.AddScoped<IGitHubAnalyzerService, GitHubAnalyzerService>();
            services.Configure<Judge0Settings>(configuration.GetSection(Judge0Settings.SectionName));
            services.Configure<ApiSettings>(configuration.GetSection("ApiEndpoints"));

            return services;
        }
    }
}