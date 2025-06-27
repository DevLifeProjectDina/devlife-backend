
using DevLifeBackend.DTOs;
using DevLifeBackend.Services;
using Microsoft.AspNetCore.Mvc; 
using Serilog;

namespace DevLifeBackend.Endpoints;

public static class DatingEndpoints
{
    public static WebApplication MapDatingEndpoints(this WebApplication app)
    {
        var datingGroup = app.MapGroup("/api/dating").WithTags("Dating Room");

        datingGroup.MapGet("/profiles", async ([FromServices] IDatingService datingService) =>
        {
            Log.Information("Fetching random dating profiles");
            var profiles = await datingService.GetRandomProfilesAsync(5);
            return Results.Ok(profiles);
        });

        datingGroup.MapPost("/chat", async ([FromBody] ChatRequestDto chatRequest, [FromServices] IDatingService datingService, HttpContext httpContext) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized chat attempt to profile {ProfileId}", chatRequest.ProfileId);
                return Results.Unauthorized();
            }

            Log.Information("User {UserId} sent a chat message to profile {ProfileId}: '{Message}'", userIdString, chatRequest.ProfileId, chatRequest.Message);

            var response = await datingService.GetChatResponseAsync(chatRequest.ProfileId, chatRequest.Message);

            Log.Information("AI for profile {ProfileId} replied to user {UserId}", chatRequest.ProfileId, userIdString);

            return Results.Ok(new { Reply = response });
        });

        return app;
    }
}