using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using DevLifeBackend.Services;
using DevLifeBackend.Settings; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Sprache;
using System.Net.Http.Headers;
using DevLifeBackend.Enums;

namespace DevLifeBackend.Endpoints;

public record AnalyzeRepoRequest(string Owner, string RepoName, string? PersonalAccessToken);

public static class GitHubEndpoints
{
    public static WebApplication MapGitHubEndpoints(this WebApplication app)
    {
        var gitHubAuthGroup = app.MapGroup("/api/auth/github").WithTags("Authentication");

        gitHubAuthGroup.MapGet("/redirect", ([FromServices] IOptions < ApiSettings > apiSettings) =>
        {
            var clientId = Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID");
            Log.Information("Generated GitHub redirect URL for client_id: {ClientId}", clientId);
            var redirectUrl = $"{apiSettings.Value.GitHubAuthorizeEndpoint}?client_id={clientId}&scope=public_repo";
            return Results.Ok(new { RedirectUrl = redirectUrl });
        });

        gitHubAuthGroup.MapGet("/callback", async (string code, IHttpClientFactory clientFactory, ApplicationDbContext db, HttpContext httpContext, IOptions<ApiSettings> apiSettings) =>
        {
            Log.Information("Received GitHub callback with a code.");
            var httpClient = clientFactory.CreateClient();
            var tokenRequest = new GitHubAccessTokenRequestDto
            {
                ClientId = Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")!,
                ClientSecret = Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET")!,
                Code = code
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiSettings.Value.GitHubTokenEndpoint)
            {
                Content = JsonContent.Create(tokenRequest)
            };
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Failed to exchange code for access token. GitHub returned {StatusCode}", response.StatusCode);
                return Results.BadRequest("Could not get access token from GitHub.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubAccessTokenResponseDto>();
            var accessToken = tokenResponse?.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
            {
                Log.Error("Exchanged code for token, but the access token was empty.");
                return Results.Problem("Access token was empty.");
            }
            Log.Information("Successfully received GitHub access token.");

            var userClient = clientFactory.CreateClient();
            userClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DevLifePortal", "1.0"));
            userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var gitHubUser = await userClient.GetFromJsonAsync<GitHubUserDto>(apiSettings.Value.GitHubUserEndpoint);
            if (gitHubUser == null)
            {
                Log.Error("Failed to retrieve user info from GitHub using the access token.");
                return Results.Problem("Could not retrieve user info from GitHub.");
            }
            Log.Information("Successfully retrieved GitHub user profile for {GitHubLogin}", gitHubUser.Login);

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == gitHubUser.Login);
            if (user == null)
            {
                Log.Information("User {GitHubLogin} is new. Creating a new profile.", gitHubUser.Login);
                user = new User
                {
                    Username = gitHubUser.Login,
                    Name = gitHubUser.Name ?? gitHubUser.Login,
                    Surname = "From GitHub",
                    DateOfBirth = DateTime.UtcNow.AddYears(-20),
                    Stacks = new[] { "Unknown" },
                    ExperienceLevel = ExperienceLevel.Unknown,
                    ZodiacSign = "Unknown"
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            else
            {
                Log.Information("Found existing user {Username} for GitHub login {GitHubLogin}", user.Username, gitHubUser.Login);
            }

            httpContext.Session.SetString("UserId", user.Id.ToString());
            httpContext.Session.SetString("GitHubToken", accessToken);
            Log.Information("Created DevLife session for user {Username} (ID: {UserId})", user.Username, user.Id);

            return Results.Ok(new { Message = $"Successfully authenticated as {user.Username}." });
        });

        var analysisGroup = app.MapGroup("/api/github").WithTags("GitHub Analyzer");

        analysisGroup.MapGet("/my-repos", async (HttpContext httpContext, [FromServices] IGitHubAnalyzerService analyzer) =>
        {
            var token = httpContext.Session.GetString("GitHubToken");
            if (string.IsNullOrEmpty(token))
            {
                Log.Warning("Unauthorized request for user repositories.");
                return Results.Unauthorized();
            }
            Log.Information("Fetching repositories for user with active session.");
            var repos = await analyzer.GetUserRepositoriesAsync(token);
            if (repos == null)
            {
                Log.Error("Failed to retrieve user repositories.");
                return Results.Problem("Failed to retrieve repositories.");
            }
            return Results.Ok(repos);
        })
        .WithName("GetUserRepositories")
        .Produces<List<RepoDto>>();

        analysisGroup.MapPost("/analyze", async ([FromBody] AnalyzeRepoRequest request, HttpContext httpContext, [FromServices] IGitHubAnalyzerService analyzer) =>
        {
            var token = httpContext.Session.GetString("GitHubToken") ?? request.PersonalAccessToken;
            if (string.IsNullOrEmpty(token))
            {
                Log.Warning("Unauthorized analysis request for repo {Owner}/{RepoName}", request.Owner, request.RepoName);
                return Results.Unauthorized();
            }

            Log.Information("Starting analysis for repo {Owner}/{RepoName}", request.Owner, request.RepoName);
            var result = await analyzer.AnalyzeRepoAsync(request.Owner, request.RepoName, token);
            if (result == null)
            {
                Log.Error("Analysis failed for repo {Owner}/{RepoName}", request.Owner, request.RepoName);
                return Results.Problem("Analysis failed.");
            }

            Log.Information("Successfully completed analysis for repo {Owner}/{RepoName}. Personality Type: {Personality}", request.Owner, request.RepoName, result.PersonalityType);
            return Results.Ok(result);
        });

        analysisGroup.MapPost("/analyze/card", async ([FromBody] AnalyzeRepoRequest request, HttpContext httpContext, [FromServices] IGitHubAnalyzerService analyzer, [FromServices] IImageService imageService) =>
        {
            var token = httpContext.Session.GetString("GitHubToken") ?? request.PersonalAccessToken;
            if (string.IsNullOrEmpty(token))
            {
                Log.Warning("Unauthorized request for analysis card for repo {Owner}/{RepoName}", request.Owner, request.RepoName);
                return Results.Unauthorized();
            }

            Log.Information("Starting analysis for image card generation for repo {Owner}/{RepoName}", request.Owner, request.RepoName);
            var analysisResult = await analyzer.AnalyzeRepoAsync(request.Owner, request.RepoName, token);
            if (analysisResult == null)
            {
                Log.Error("Analysis for image card failed for repo {Owner}/{RepoName}", request.Owner, request.RepoName);
                return Results.Problem("Analysis failed.");
            }

            Log.Information("Analysis complete, generating image for personality type: {Personality}", analysisResult.PersonalityType);
            var imageBytes = await imageService.GenerateAnalysisCardAsync(analysisResult.PersonalityType);

            if (imageBytes == null)
            {
                Log.Error("Image generation failed for personality type: {Personality}", analysisResult.PersonalityType);
                return Results.Problem("Image generation failed.");
            }

            Log.Information("Image generated successfully.");
            return Results.File(imageBytes, "image/png", "analysis-card.png");
        })
        .WithName("GetAnalysisCard")
        .Produces(200, typeof(File), "image/png");

        return app;
    }
}