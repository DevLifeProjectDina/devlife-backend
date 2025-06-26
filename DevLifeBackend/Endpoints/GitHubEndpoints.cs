// File: Endpoints/GitHubEndpoints.cs
using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using DevLifeBackend.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace DevLifeBackend.Endpoints;

public record AnalyzeRepoRequest(string Owner, string RepoName, string? PersonalAccessToken);

public static class GitHubEndpoints
{
    public static WebApplication MapGitHubEndpoints(this WebApplication app)
    {
        var gitHubGroup = app.MapGroup("/api/auth/github").WithTags("Authentication");

        gitHubGroup.MapGet("/redirect", () =>
        {
            var clientId = Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID");
            var redirectUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&scope=public_repo";
            return Results.Ok(new { RedirectUrl = redirectUrl });
        });

        gitHubGroup.MapGet("/callback", async (string code, IHttpClientFactory clientFactory, ApplicationDbContext db, HttpContext httpContext) =>
        {
            var httpClient = clientFactory.CreateClient();
            var tokenRequest = new GitHubAccessTokenRequestDto
            {
                ClientId = Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")!,
                ClientSecret = Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET")!,
                Code = code
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
            {
                Content = JsonContent.Create(tokenRequest)
            };
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                return Results.BadRequest("Could not get access token from GitHub.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubAccessTokenResponseDto>();
            var accessToken = tokenResponse?.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
            {
                return Results.Problem("Access token was empty.");
            }

            var userClient = clientFactory.CreateClient();
            userClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DevLifePortal", "1.0"));
            userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var gitHubUser = await userClient.GetFromJsonAsync<GitHubUserDto>("https://api.github.com/user");
            if (gitHubUser == null) { return Results.Problem("Could not retrieve user info from GitHub."); }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == gitHubUser.Login);
            if (user == null)
            {
                user = new User
                {
                    Username = gitHubUser.Login,
                    Name = gitHubUser.Name ?? gitHubUser.Login,
                    Surname = "From GitHub",
                    DateOfBirth = DateTime.UtcNow.AddYears(-20),
                    Stacks = new[] { "Unknown" },
                    ExperienceLevel = "Unknown",
                    ZodiacSign = "Unknown"
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            httpContext.Session.SetString("UserId", user.Id.ToString());
            httpContext.Session.SetString("GitHubToken", accessToken);
            return Results.Ok(new { Message = $"Successfully authenticated as {user.Username}." });
        });

        var analysisGroup = app.MapGroup("/api/github").WithTags("GitHub Analyzer");

        analysisGroup.MapGet("/my-repos", async (HttpContext httpContext, IGitHubAnalyzerService analyzer) =>
        {
            var token = httpContext.Session.GetString("GitHubToken");
            if (string.IsNullOrEmpty(token)) return Results.Unauthorized();

            var repos = await analyzer.GetUserRepositoriesAsync(token);
            if (repos == null) return Results.Problem("Failed to retrieve repositories.");

            return Results.Ok(repos);
        })
        .WithName("GetUserRepositories")
        .Produces<List<RepoDto>>();

        analysisGroup.MapPost("/analyze", async (AnalyzeRepoRequest request, HttpContext httpContext, IGitHubAnalyzerService analyzer) =>
        {
            var token = httpContext.Session.GetString("GitHubToken") ?? request.PersonalAccessToken;
            if (string.IsNullOrEmpty(token)) return Results.Unauthorized();

            var result = await analyzer.AnalyzeRepoAsync(request.Owner, request.RepoName, token);
            if (result == null) return Results.Problem("Analysis failed.");
            return Results.Ok(result);
        });
        analysisGroup.MapPost("/analyze/card", async (AnalyzeRepoRequest request, HttpContext httpContext, IGitHubAnalyzerService analyzer, IImageService imageService) =>
        {
            var token = httpContext.Session.GetString("GitHubToken") ?? request.PersonalAccessToken;
            if (string.IsNullOrEmpty(token)) return Results.Unauthorized();

            // 1. Get the text analysis first
            var analysisResult = await analyzer.AnalyzeRepoAsync(request.Owner, request.RepoName, token);
            if (analysisResult == null) return Results.Problem("Analysis failed.");

            // 2. Pass the personality type to the image service
            var imageBytes = await imageService.GenerateAnalysisCardAsync(analysisResult.PersonalityType);

            // 3. Return the generated image as a file
            return Results.File(imageBytes, "image/png", "analysis-card.png");
        })
           .WithName("GetAnalysisCard")
           .Produces(200, typeof(File), "image/png");

        return app;
    }
}