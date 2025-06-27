using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using DevLifeBackend.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DevLifeBackend.Endpoints;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var authGroup = app.MapGroup("/auth").WithTags("Authentication");

        authGroup.MapPost("/register",
            async (UserRegistrationDto userDto, IValidator<UserRegistrationDto> validator, ApplicationDbContext db, IZodiacService zodiacService) =>
            {
                var validationResult = await validator.ValidateAsync(userDto);
                if (!validationResult.IsValid)
                {
                    Log.Warning("User registration failed validation for username {Username}. Errors: {@Errors}", userDto.Username, validationResult.ToDictionary());
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                if (await db.Users.AnyAsync(u => u.Username == userDto.Username))
                {
                    Log.Warning("User registration failed: username {Username} already exists.", userDto.Username);
                    return Results.Conflict("User with this username already exists.");
                }

                var user = new User
                {
                    Username = userDto.Username,
                    Name = userDto.Name,
                    Surname = userDto.Surname,
                    DateOfBirth = userDto.DateOfBirth,
                    Stacks = userDto.Stacks,
                    ExperienceLevel = userDto.ExperienceLevel,
                    ZodiacSign = zodiacService.GetZodiacSign(userDto.DateOfBirth)
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                Log.Information("New user registered successfully: {Username}, UserID: {UserId}", user.Username, user.Id);

                return Results.Created($"/users/{user.Username}", user);
            });

        authGroup.MapPost("/login", async (LoginDto loginDto, HttpContext httpContext, ApplicationDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null)
            {
                Log.Warning("Login failed: user with username {Username} not found.", loginDto.Username);
                return Results.NotFound("User not found.");
            }

            httpContext.Session.SetString("UserId", user.Id.ToString());

            Log.Information("User {Username} (ID: {UserId}) logged in successfully.", user.Username, user.Id);

            return Results.Ok($"Welcome back, {user.Name}!");
        });

        return app;
    }
}