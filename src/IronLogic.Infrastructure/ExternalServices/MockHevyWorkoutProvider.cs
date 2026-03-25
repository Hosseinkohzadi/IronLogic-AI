using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;

namespace IronLogic.Infrastructure.ExternalServices;

/// <summary>
/// Lightweight mock provider that simulates the Hevy API for local development and tests.
/// Returns a small set of realistic sessions for a Classic Physique athlete.
/// </summary>
public class MockHevyWorkoutProvider : IWorkoutProvider
{
    public Task<IEnumerable<HevyWorkoutSessionDto>> GetRecentSessionsAsync(int limit = 10)
    {
        var now = DateTime.UtcNow;

        var sessions = new List<HevyWorkoutSessionDto>
        {
            new HevyWorkoutSessionDto
            {
                Id = Guid.NewGuid(),
                StartTime = now.AddDays(-3).AddHours(-18),
                EndTime = now.AddDays(-3).AddHours(-17),
                Title = "Push Day",
                Exercises = new List<HevyExerciseDto>
                {
                    new HevyExerciseDto
                    {
                        Name = "Barbell Bench Press",
                        Sets = new List<HevySetDto>
                        {
                            new HevySetDto { Weight = 180.0, Reps = 5, SetType = "warmup" },
                            new HevySetDto { Weight = 220.0, Reps = 5, SetType = "work" },
                            new HevySetDto { Weight = 220.0, Reps = 5, SetType = "work" },
                        }
                    },
                    new HevyExerciseDto
                    {
                        Name = "Overhead Press",
                        Sets = new List<HevySetDto>
                        {
                            new HevySetDto { Weight = 95.0, Reps = 6, SetType = "work" },
                            new HevySetDto { Weight = 95.0, Reps = 6, SetType = "work" }
                        }
                    }
                }
            },
            new HevyWorkoutSessionDto
            {
                Id = Guid.NewGuid(),
                StartTime = now.AddDays(-2).AddHours(-18),
                EndTime = now.AddDays(-2).AddHours(-17),
                Title = "Pull Day",
                Exercises = new List<HevyExerciseDto>
                {
                    new HevyExerciseDto
                    {
                        Name = "Weighted Pull-ups",
                        Sets = new List<HevySetDto>
                        {
                            new HevySetDto { Weight = 25.0, Reps = 6, SetType = "work" },
                            new HevySetDto { Weight = 25.0, Reps = 6, SetType = "work" }
                        }
                    },
                    new HevyExerciseDto
                    {
                        Name = "Barbell Row",
                        Sets = new List<HevySetDto>
                        {
                            new HevySetDto { Weight = 160.0, Reps = 6, SetType = "work" },
                            new HevySetDto { Weight = 160.0, Reps = 6, SetType = "work" }
                        }
                    }
                }
            },
            new HevyWorkoutSessionDto
            {
                Id = Guid.NewGuid(),
                StartTime = now.AddDays(-1).AddHours(-18),
                EndTime = now.AddDays(-1).AddHours(-17),
                Title = "Leg Day",
                Exercises = new List<HevyExerciseDto>
                {
                    new HevyExerciseDto
                    {
                        Name = "Back Squat",
                        Sets = new List<HevySetDto>
                        {
                            new HevySetDto { Weight = 240.0, Reps = 5, SetType = "work" },
                            new HevySetDto { Weight = 240.0, Reps = 5, SetType = "work" }
                        }
                    },
                    new HevyExerciseDto
                    {
                        Name = "Romanian Deadlift",
                        Sets = new List<HevySetDto>
                        {
                            new HevySetDto { Weight = 180.0, Reps = 8, SetType = "work" }
                        }
                    }
                }
            }
        };

        var result = sessions.Take(Math.Max(0, Math.Min(limit, sessions.Count)));
        return Task.FromResult(result.AsEnumerable());
    }
}