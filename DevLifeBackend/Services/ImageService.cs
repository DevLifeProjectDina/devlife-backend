using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using DevLifeBackend.Settings;
using Microsoft.Extensions.Options; 

namespace DevLifeBackend.Services
{
    public class OpenAIImageResponse
    {
        [JsonPropertyName("data")]
        public List<ImageData> Data { get; set; } = new();
    }

    public class ImageData
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = default!;
    }

    public interface IImageService
    {
        Task<byte[]?> GenerateAnalysisCardAsync(string personalityType);
    }

    public class ImageService : IImageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly ILogger<ImageService> _logger;
        private readonly string _dalleUrl;

        public ImageService(IHttpClientFactory httpClientFactory, ILogger<ImageService> logger, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
            _logger = logger;
            _dalleUrl = apiSettings.Value.OpenAiImageGeneration;
        }

        public async Task<byte[]?> GenerateAnalysisCardAsync(string personalityType)
        {
            _logger.LogInformation("Starting AI image generation for personality type: {PersonalityType}", personalityType);

            var prompt = $"A funny, expressive character of software developer, representing the personality '{personalityType}'. " +
                         "Modern Tim Burton style" +
                         "The character should be quirky and fun, with exaggerated features, maybe interacting with a laptop or a coffee mug. " +
                         "Simple, vibrant background.";

            try
            {
                var httpClient = _httpClientFactory.CreateClient("OpenAI_Direct");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var requestBody = new
                {
                    model = "dall-e-3",
                    prompt = prompt,
                    n = 1,
                    size = "1024x1024"
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending image generation request to DALL-E 3 for personality: {PersonalityType}", personalityType);
                var response = await httpClient.PostAsync(_dalleUrl, stringContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AI Image Generation Failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                    return null;
                }

                _logger.LogInformation("Successfully received response from DALL-E 3.");

                var responseJson = await response.Content.ReadAsStringAsync();
                var imageResponse = JsonSerializer.Deserialize<OpenAIImageResponse>(responseJson);

                if (imageResponse != null && imageResponse.Data.Count > 0)
                {
                    var imageUrl = imageResponse.Data[0].Url;
                    _logger.LogInformation("Image URL received. Downloading image...");

                    var imageDownloaderClient = _httpClientFactory.CreateClient();
                    var imageBytes = await imageDownloaderClient.GetByteArrayAsync(imageUrl);

                    _logger.LogInformation("Image downloaded successfully ({SizeInKB} KB).", imageBytes.Length / 1024);
                    return imageBytes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during AI image generation.");
            }

            return null;
        }
    }
}