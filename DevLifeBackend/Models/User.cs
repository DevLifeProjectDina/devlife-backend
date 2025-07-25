﻿using DevLifeBackend.Enums;

namespace DevLifeBackend.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required TechStack Stacks { get; set; }
    public required ExperienceLevel ExperienceLevel { get; set; }
    public string? ZodiacSign { get; set; }
    public int Score { get; set; } = 100;
    public int WinStreak { get; set; } = 0;
    public int BugChaseHighScore { get; set; } = 0;
}