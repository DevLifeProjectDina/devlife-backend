// File: Endpoints/ProfileEndpoints.cs
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using DevLifeBackend.Services;

namespace DevLifeBackend.Endpoints;

public static class ProfileEndpoints
{
    public static WebApplication MapProfileEndpoints(this WebApplication app)
    {
        var profileGroup = app.MapGroup("/api/profile").WithTags("Profile");

        profileGroup.MapGet("/customization", async (HttpContext httpContext, IProfileService profileService) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

            var customization = await profileService.GetCustomizationAsync(int.Parse(userIdString));
            return Results.Ok(customization);
        });

        profileGroup.MapPut("/customization", async (CharacterCustomizationDto dto, HttpContext httpContext, IProfileService profileService) =>
        {
            var userIdString = httpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();

            var userId = int.Parse(userIdString);

            // Map DTO to Model
            var customizationModel = new CharacterCustomization
            {
                UserId = userId,
                Hat = dto.Hat,
                ShirtColor = dto.ShirtColor,
                Pet = dto.Pet
            };

            await profileService.SaveCustomizationAsync(userId, customizationModel);
            return Results.Ok(customizationModel);
        });

        return app;
    }
}