using DevLifeBackend.Data;
using DevLifeBackend.DTOs;
using DevLifeBackend.Models;
using DevLifeBackend.Services;
using Microsoft.EntityFrameworkCore;

namespace DevLifeBackend.Endpoints;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/register", async (UserRegistrationDto userDto, ApplicationDbContext db, IZodiacService zodiacService) => {
            if (await db.Users.AnyAsync(u => u.Username == userDto.Username)) { return Results.Conflict("User with this username already exists."); }

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
            return Results.Created($"/users/{user.Username}", user);
        });

        app.MapPost("/login", async (LoginDto loginDto, HttpContext httpContext, ApplicationDbContext db) => {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null) return Results.NotFound("User not found.");
            httpContext.Session.SetString("UserId", user.Id.ToString());
            return Results.Ok($"Welcome back, {user.Name}!");
        });

        return app;
    }
}