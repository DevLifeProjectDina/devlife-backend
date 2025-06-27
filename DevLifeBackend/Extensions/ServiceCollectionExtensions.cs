
using DevLifeBackend.Services;
using OpenAI;
using FluentValidation;

namespace DevLifeBackend.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
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
            services.AddValidatorsFromAssemblyContaining<Program>();
            services.AddSingleton(new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
            services.AddScoped<IExcuseService, ExcuseService>();

            return services;
        }
    }
}