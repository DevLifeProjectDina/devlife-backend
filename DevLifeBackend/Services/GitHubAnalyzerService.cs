// File: Services/GitHubAnalyzerService.cs
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI;
using OpenAI.Chat;

namespace DevLifeBackend.Services
{
    // --- DTOs and Models for GraphQL Responses ---
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

    // --- Service Logic ---
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

        // This service does not need IHttpClientFactory as GraphQL.Client creates its own client.
        public GitHubAnalyzerService(OpenAIClient openAIClient)
        {
            _openAIClient = openAIClient;
        }

        public async Task<AnalysisResult?> AnalyzeRepoAsync(string owner, string repoName, string token)
        {
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
                var response = await graphQLClient.SendQueryAsync<GitHubRepoData>(request);
                var repoData = response.Data?.Repository;
                if (repoData == null) return new AnalysisResult("Ghost", "Could not find this repository.", new(), new(), "Unknown");

                var commits = repoData.DefaultBranchRef?.Target?.History?.Edges?.Select(e => e.Node).ToList();
                if (commits == null || !commits.Any()) return new AnalysisResult("Mystery", "This repository has no commits to analyze!", new(), new(), "Unknown");

                var analysisPoints = new Dictionary<string, int> { { "The Architect", 0 }, { "The Sprinter", 0 }, { "The Documentarian", 0 } };
                if (repoData.Readme?.Text != null) analysisPoints["The Documentarian"]++;
                if (repoData.DefaultBranchRef?.Target?.Tree?.Entries != null && repoData.DefaultBranchRef.Target.Tree.Entries.Any(e => e.Name == "src")) analysisPoints["The Architect"]++;

                foreach (var commit in commits)
                {
                    if (commit == null) continue;
                    if (char.IsUpper(commit.MessageHeadline?.FirstOrDefault() ?? 'a')) analysisPoints["The Architect"]++;
                    if (commit.Message.Length < 50) analysisPoints["The Sprinter"]++;
                    if (commit.Message.ToLower().Contains("fix") || commit.Message.ToLower().Contains("bug")) analysisPoints["The Sprinter"]++;
                    if (commit.Message.Length > 100) analysisPoints["The Documentarian"]++;
                }
                var personalityType = analysisPoints.OrderByDescending(kv => kv.Value).First().Key;

                var prompt =
                    $"You are a fun developer profiler. Based on the determined personality type '{personalityType}', generate a profile. " +
                    "Provide a response in JSON format with the following fields: " +
                    "\"strengths\" (a short, witty list of 2-3 strengths), " +
                    "\"weaknesses\" (a short, witty list of 2-3 weaknesses), " +
                    "\"celebrityDeveloper\" (the name of a famous developer they are similar to), " +
                    "\"analysisSummary\" (a short, funny summary roast based on the personality type). " +
                    "Return ONLY the JSON object and nothing else.";

                var chatRequest = new ChatRequest(new[] { new Message(Role.System, prompt) }, "gpt-3.5-turbo");
                var aiResponse = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                var aiResponseJson = aiResponse.FirstChoice.Message.ToString();

                using var jsonDoc = JsonDocument.Parse(aiResponseJson!);
                var root = jsonDoc.RootElement;

                var strengths = root.GetProperty("strengths").EnumerateArray().Select(e => e.GetString() ?? "").ToList();
                var weaknesses = root.GetProperty("weaknesses").EnumerateArray().Select(e => e.GetString() ?? "").ToList();
                var celebrity = root.GetProperty("celebrityDeveloper").GetString() ?? "Unknown";
                var summary = root.GetProperty("analysisSummary").GetString() ?? "AI is speechless.";

                return new AnalysisResult(personalityType, summary, strengths, weaknesses, celebrity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GitHub Analysis failed: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RepoDto>?> GetUserRepositoriesAsync(string token)
        {
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
                return repos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GitHub Get Repos failed: {ex.Message}");
                return null;
            }
        }
    }
}