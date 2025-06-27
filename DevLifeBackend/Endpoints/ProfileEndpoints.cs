using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using DevLifeBackend.Services;
using Serilog;

namespace DevLifeBackend.Endpoints;

public static class ProfileEndpoints
{
    public static WebApplication MapProfileEndpoints(this WebApplication app)
    {
        var profileGroup = app.MapGroup("/api/profile").WithTags("Profile");

        profileGroup.MapGet("/customization", async (HttpContext httpContext, IProfileService profileService) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized request for profile customization.");
                return Results.Unauthorized();
            }

            Log.Information("Fetching customization for User {UserId}", userIdString);
            var customization = await profileService.GetCustomizationAsync(int.Parse(userIdString));
            return Results.Ok(customization);
        });

        profileGroup.MapPut("/customization", async (CharacterCustomizationDto dto, HttpContext httpContext, IProfileService profileService) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                Log.Warning("Unauthorized attempt to update profile customization.");
                return Results.Unauthorized();
            }

            var userId = int.Parse(userIdString);

            var customizationModel = new CharacterCustomization
            {
                UserId = userId,
                Hat = dto.Hat,
                ShirtColor = dto.ShirtColor,
                Pet = dto.Pet
            };

            await profileService.SaveCustomizationAsync(userId, customizationModel);

            Log.Information("User {UserId} updated their character customization: Hat -> {Hat}, ShirtColor -> {ShirtColor}, Pet -> {Pet}", userId, dto.Hat, dto.ShirtColor, dto.Pet);

            return Results.Ok(customizationModel);
        });

        return app;
    }
}