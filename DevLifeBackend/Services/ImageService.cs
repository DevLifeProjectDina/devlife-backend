// File: Services/ImageService.cs
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevLifeBackend.Services
{
    // DTO for the response from the OpenAI Images API
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
        // We no longer need OpenAIClient for this, just the key and HttpClientFactory
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public ImageService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
        }

        public async Task<byte[]?> GenerateAnalysisCardAsync(string personalityType)
        {
            var prompt = $"A funny, comical cartoon character of a software developer representing the personality '{personalityType}'. " +
             "Use a slightly exaggerated, caricature style, with expressive eyes. " +
             "The character could be holding a comically oversized coffee mug or interacting with a tiny, confused-looking bug. " +
             "Simple, colorful background.";
            try
            {
                var httpClient = _httpClientFactory.CreateClient("OpenAI_Direct");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                // Manually create the JSON payload
                var requestBody = new
                {
                    model = "dall-e-3",
                    prompt = prompt,
                    n = 1,
                    size = "1024x1024"
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Make the direct POST request
                var response = await httpClient.PostAsync("https://api.openai.com/v1/images/generations", stringContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"AI Image Generation Failed with status {response.StatusCode}: {errorContent}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var imageResponse = JsonSerializer.Deserialize<OpenAIImageResponse>(responseJson);

                if (imageResponse != null && imageResponse.Data.Count > 0)
                {
                    var imageUrl = imageResponse.Data[0].Url;

                    // Download the generated image
                    var imageDownloaderClient = _httpClientFactory.CreateClient();
                    var imageBytes = await imageDownloaderClient.GetByteArrayAsync(imageUrl);

                    return imageBytes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Image Generation Exception: {ex.Message}");
            }

            return null;
        }
    }
}