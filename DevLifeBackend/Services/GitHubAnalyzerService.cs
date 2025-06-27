using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI;
using OpenAI.Chat;
using Serilog;

namespace DevLifeBackend.Services
{
    // DTOs and Models for GraphQL Responses
    public class GitHubRepoData { public Repository? Repository { get; set; } }
    public class Repository { public DefaultBranchRef? DefaultBranchRef { get; set; } public FileContent? Readme { get; set; } }
    public class FileContent { public string? Text { get; set; } }
    public class DefaultBranchRef { public Target? Target { get; set; } }
    public class Target { [JsonPropertyName("history")] public CommitHistory? History { get; set; } public Tree? Tree { get; set; } }
    public class Tree { public List<TreeEntry>? Entries { get; set; } }
    public class TreeEntry { public string? Name { get; set; } }
    public class CommitHistory { public List<CommitEdge>? Edges { get; set; } }
    public class CommitEdge { public CommitNode? Node { get; set; } }
    public class CommitNode { public string? Message { get; set; } public string? MessageHeadline { get; set; } public int Additions { get; set; } public int Deletions { get; set; } }
    public class GitHubUserReposData { public UserViewer? Viewer { get; set; } }
    public class UserViewer { public RepositoryConnection? Repositories { get; set; } }
    public class RepositoryConnection { public List<RepositoryNode>? Nodes { get; set; } }
    public class RepositoryNode { public string? Name { get; set; } public string? Url { get; set; } public OwnerInfo? Owner { get; set; } }
    public class OwnerInfo { public string? Login { get; set; } }

    // Service Logic
    public record AnalysisResult(string PersonalityType, string AnalysisSummary, List<string> Strengths, List<string> Weaknesses, string CelebrityDeveloper);
    public record RepoDto(string Owner, string Name, string Url);

    public interface IGitHubAnalyzerService
    {
        Task<AnalysisResult?> AnalyzeRepoAsync(string owner, string repoName, string token);
        Task<List<RepoDto>?> GetUserRepositoriesAsync(string token);
    }

    public class GitHubAnalyzerService : IGitHubAnalyzerService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly ILogger<GitHubAnalyzerService> _logger;

        public GitHubAnalyzerService(OpenAIClient openAIClient, ILogger<GitHubAnalyzerService> logger)
        {
            _openAIClient = openAIClient;
            _logger = logger;
        }

        public async Task<AnalysisResult?> AnalyzeRepoAsync(string owner, string repoName, string token)
        {
            _logger.LogInformation("Starting GitHub repository analysis for {Owner}/{RepoName}", owner, repoName);
            var graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = new Uri("https://api.github.com/graphql") }, new SystemTextJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("User-Agent", "DevLifePortal");

            var request = new GraphQLRequest
            {
                Query = @"
                    query($owner: String!, $name: String!) {
                      repository(owner: $owner, name: $name) {
                        readme: object(expression: ""HEAD:README.md"") { ... on Blob { text } }
                        defaultBranchRef {
                          target {
                            ... on Commit {
                              history(first: 20) {
                                edges { node { message messageHeadline additions deletions } }
                              }
                              tree { entries { name } }
                            }
                          }
                        }
                      }
                    }",
                Variables = new { owner, name = repoName }
            };

            try
            {
                _logger.LogInformation("Sending GraphQL query to GitHub for repo {Owner}/{RepoName}", owner, repoName);
                var response = await graphQLClient.SendQueryAsync<GitHubRepoData>(request);

                if (response.Errors != null && response.Errors.Any())
                {
                    foreach (var error in response.Errors) _logger.LogError("GitHub GraphQL Error: {ErrorMessage}", error.Message);
                    return new AnalysisResult("Error", "GitHub API returned errors.", new(), new(), "Unknown");
                }

                var repoData = response.Data?.Repository;
                if (repoData == null)
                {
                    _logger.LogWarning("Could not find repository {Owner}/{RepoName} or it is empty.", owner, repoName);
                    return new AnalysisResult("Ghost", "Could not find this repository.", new(), new(), "Unknown");
                }

                _logger.LogInformation("Successfully fetched data for {Owner}/{RepoName}", owner, repoName);

                var commits = repoData.DefaultBranchRef?.Target?.History?.Edges?.Select(e => e.Node).ToList();
                if (commits == null || !commits.Any())
                {
                    _logger.LogWarning("Repository {Owner}/{RepoName} has no commits to analyze.", owner, repoName);
                    return new AnalysisResult("Mystery", "This repository has no commits to analyze!", new(), new(), "Unknown");
                }

                var analysisPoints = new Dictionary<string, int> { { "The Architect", 0 }, { "The Sprinter", 0 }, { "The Documentarian", 0 } };
                if (repoData.Readme?.Text != null) analysisPoints["The Documentarian"]++;
                if (repoData.DefaultBranchRef?.Target?.Tree?.Entries != null && repoData.DefaultBranchRef.Target.Tree.Entries.Any(e => e.Name == "src")) analysisPoints["The Architect"]++;
                foreach (var commit in commits)
                {
                    if (commit?.Message == null || commit.MessageHeadline == null) continue; 

                    if (char.IsUpper(commit.MessageHeadline.FirstOrDefault())) analysisPoints["The Architect"]++;
                    if (commit.Message.Length < 50) analysisPoints["The Sprinter"]++;
                    if (commit.Message.ToLower().Contains("fix") || commit.Message.ToLower().Contains("bug")) analysisPoints["The Sprinter"]++;
                    if (commit.Message.Length > 100) analysisPoints["The Documentarian"]++;
                }
                var personalityType = analysisPoints.OrderByDescending(kv => kv.Value).First().Key;
                _logger.LogInformation("Determined personality type for {Owner}/{RepoName}: {PersonalityType}", owner, repoName, personalityType);

                var prompt =
                    $"You are a fun developer profiler. Based on the determined personality type '{personalityType}', generate a profile. " +
                    "Provide a response in JSON format with the following fields: " +
                    "\"strengths\" (a short, witty list of 2-3 strengths), " +
                    "\"weaknesses\" (a short, witty list of 2-3 weaknesses), " +
                    "\"celebrityDeveloper\" (the name of a famous developer they are similar to), " +
                    "\"analysisSummary\" (a short, funny summary roast based on the personality type). " +
                    "Return ONLY the JSON object and nothing else.";

                _logger.LogInformation("Requesting personality profile from OpenAI for type: {PersonalityType}", personalityType);
                var chatRequest = new ChatRequest(new[] { new Message(Role.System, prompt) }, "gpt-3.5-turbo");
                var aiResponse = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                var aiResponseJson = aiResponse.FirstChoice.Message.ToString();

                using var jsonDoc = JsonDocument.Parse(aiResponseJson!);
                var root = jsonDoc.RootElement;

                var strengths = root.GetProperty("strengths").EnumerateArray().Select(e => e.GetString() ?? "").ToList();
                var weaknesses = root.GetProperty("weaknesses").EnumerateArray().Select(e => e.GetString() ?? "").ToList();
                var celebrity = root.GetProperty("celebrityDeveloper").GetString() ?? "Unknown";
                var summary = root.GetProperty("analysisSummary").GetString() ?? "AI is speechless.";

                _logger.LogInformation("Successfully generated full analysis for {Owner}/{RepoName}", owner, repoName);
                return new AnalysisResult(personalityType, summary, strengths, weaknesses, celebrity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during GitHub analysis for {Owner}/{RepoName}", owner, repoName);
                return null;
            }
        }

        public async Task<List<RepoDto>?> GetUserRepositoriesAsync(string token)
        {
            _logger.LogInformation("Fetching user repositories from GitHub.");
            var graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = new Uri("https://api.github.com/graphql") }, new SystemTextJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("User-Agent", "DevLifePortal");
            var request = new GraphQLRequest
            {
                Query = @"query { viewer { repositories(first: 50, privacy: PUBLIC, orderBy: {field: PUSHED_AT, direction: DESC}) { nodes { name url owner { login } } } } }"
            };
            try
            {
                var response = await graphQLClient.SendQueryAsync<GitHubUserReposData>(request);
                var repos = response.Data?.Viewer?.Repositories?.Nodes?
                    .Where(r => r.Owner != null && r.Name != null && r.Url != null)
                    .Select(r => new RepoDto(r.Owner!.Login!, r.Name!, r.Url!))
                    .ToList();
                _logger.LogInformation("Successfully fetched {RepoCount} repositories.", repos?.Count ?? 0);
                return repos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user repositories from GitHub.");
                return null;
            }
        }
    }
}