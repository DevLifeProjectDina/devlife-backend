
using DevLifeBackend.DTOs;
using DevLifeBackend.Services;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Serilog;

namespace DevLifeBackend.Endpoints;

public static class ExcuseEndpoints
{
    public static WebApplication MapExcuseEndpoints(this WebApplication app)
    {
        var excuseGroup = app.MapGroup("/api/excuses").WithTags("Excuse Generator");

        excuseGroup.MapGet("/", ([FromQuery] string meetingCategory, [FromServices] IExcuseService excuseService) =>
        {
            Log.Information("Requesting a random excuse for meeting category: {MeetingCategory}", meetingCategory);
            var excuse = excuseService.GetRandomExcuse(meetingCategory);

            if (excuse == null)
            {
                Log.Warning("No excuse found for meeting category: {MeetingCategory}", meetingCategory);
                return Results.NotFound($"No excuses found for meeting category '{meetingCategory}'.");
            }

            return Results.Ok(excuse);
        });

        excuseGroup.MapPost("/favorite", async (FavoriteExcuseRequestDto request, HttpContext httpContext, [FromServices] IDistributedCache cache) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized attempt to add excuse {ExcuseId} to favorites.", request.ExcuseId);
                return Results.Unauthorized();
            }

            var cacheKey = $"favorites_{userIdString}";
            var favoriteIdsJson = await cache.GetStringAsync(cacheKey);
            var favoriteIds = string.IsNullOrEmpty(favoriteIdsJson)
                ? new HashSet<int>()
                : JsonSerializer.Deserialize<HashSet<int>>(favoriteIdsJson);

            if (favoriteIds!.Add(request.ExcuseId))
            {
                Log.Information("User {UserId} added excuse {ExcuseId} to favorites.", userIdString, request.ExcuseId);
            }

            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(favoriteIds));

            return Results.Ok();
        });

        excuseGroup.MapGet("/favorites", async (HttpContext httpContext, [FromServices] IDistributedCache cache, [FromServices] IExcuseService excuseService) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized request for favorite excuses.");
                return Results.Unauthorized();
            }

            Log.Information("User {UserId} is fetching their favorite excuses.", userIdString);
            var cacheKey = $"favorites_{userIdString}";
            var favoriteIdsJson = await cache.GetStringAsync(cacheKey);
            if (string.IsNullOrEmpty(favoriteIdsJson)) return Results.Ok(new List<ExcuseDto>());

            var favoriteIds = JsonSerializer.Deserialize<HashSet<int>>(favoriteIdsJson);
            var allExcuses = excuseService.GetAllExcuses();
            var favoriteExcuses = allExcuses.Where(e => favoriteIds!.Contains(e.Id)).ToList();

            return Results.Ok(favoriteExcuses);
        });

        return app;
    }
}