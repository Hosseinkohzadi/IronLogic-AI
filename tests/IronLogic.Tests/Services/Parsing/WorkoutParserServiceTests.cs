using FluentAssertions;
using Xunit;

namespace IronLogic.Tests.Services.Parsing;

/// <summary>
///     Unit tests for the <see cref="WorkoutParserService"/> class.
///     Tests parsing raw workout text into structured data with weight normalization.
/// </summary>
public class WorkoutParserServiceTests
{
    private readonly IronLogic.Infrastructure.Services.Parsing.WorkoutParserService _parser = new();

    [Fact]
    public void Parse_ValidText_ReturnsCorrectSessionMetadata()
    {
        // Arrange
        var rawText = """
        Evening workout 🏋️
        Thursday, Mar 26, 2026 at 12:00pm

        Incline Bench Press (Smith Machine)
        Set 1: 135 lbs x 12
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Evening workout 🏋️");
        result.Value.Date.Should().Be(new DateTime(2026, 3, 26, 12, 0, 0));
    }

    [Fact]
    public void Parse_ComplexSets_ExtractsWeightRepsAndRpe()
    {
        // Arrange
        var rawText = """
        My Workout
        Thursday, Mar 26, 2026 at 1:00pm

        Lat Pulldown (Cable)
        Set 1: 108 lbs x 12 @ 8.5 rpe
        Set 2: 120.5 lbs x 10
        Set 3: 130 lbs x 8 @ 9 rpe
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exercise = result.Value.Exercises.Should().ContainSingle().Subject;
        exercise.Name.Should().Be("Lat Pulldown (Cable)");

        // Weights should be normalized to kg (lbs * 0.45359237)
        exercise.Sets[0].Weight.Should().Be(48.99m); // 108 lbs = 48.99 kg
        exercise.Sets[0].Reps.Should().Be(12);
        exercise.Sets[0].Rpe.Should().Be(8.5m);

        exercise.Sets[1].Weight.Should().Be(54.66m); // 120.5 lbs = 54.66 kg
        exercise.Sets[1].Reps.Should().Be(10);
        exercise.Sets[1].Rpe.Should().BeNull();

        exercise.Sets[2].Weight.Should().Be(58.97m); // 130 lbs = 58.97 kg
        exercise.Sets[2].Reps.Should().Be(8);
        exercise.Sets[2].Rpe.Should().Be(9m);
    }

    [Fact]
    public void Parse_MixedUnits_ShouldNormalizeAllToKg()
    {
        // Arrange
        var rawText = """
        Mixed Unit Workout
        Monday, April 6, 2026 at 10:00am

        Bench Press
        Set 1: 220 lbs x 5
        Set 2: 100 kg x 5
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exercise = result.Value.Exercises.Should().ContainSingle().Subject;
        exercise.Name.Should().Be("Bench Press");

        // 220 lbs should be converted to kg
        exercise.Sets[0].Weight.Should().Be(99.79m); // 220 * 0.45359237 = 99.79 kg
        exercise.Sets[0].Reps.Should().Be(5);

        // 100 kg should remain as is
        exercise.Sets[1].Weight.Should().Be(100.00m);
        exercise.Sets[1].Reps.Should().Be(5);
    }

    [Fact]
    public void Parse_KilogramUnit_ShouldKeepOriginalValue()
    {
        // Arrange
        var rawText = """
        Metric Workout
        Tuesday, May 12, 2026 at 2:30pm

        Squat
        Set 1: 140 kg x 8
        Set 2: 150 kg x 6
        Set 3: 160 kg x 4
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exercise = result.Value.Exercises.Should().ContainSingle().Subject;

        exercise.Sets[0].Weight.Should().Be(140.00m);
        exercise.Sets[1].Weight.Should().Be(150.00m);
        exercise.Sets[2].Weight.Should().Be(160.00m);
    }

    [Fact]
    public void Parse_PoundsWithLbAbbreviation_ShouldConvertToKg()
    {
        // Arrange
        var rawText = """
        Short Unit Test
        Wednesday, June 3, 2026 at 9:00am

        Deadlift
        Set 1: 315 lb x 3
        Set 2: 225 lb x 8
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exercise = result.Value.Exercises.Should().ContainSingle().Subject;

        // 315 lb = 142.88 kg
        exercise.Sets[0].Weight.Should().Be(142.88m);
        // 225 lb = 102.06 kg
        exercise.Sets[1].Weight.Should().Be(102.06m);
    }

    [Fact]
    public void Parse_DecimalWeightsInPounds_ShouldConvertAccurately()
    {
        // Arrange
        var rawText = """
        Precision Test
        Thursday, July 15, 2026 at 11:30am

        Dumbbell Press
        Set 1: 67.5 lbs x 10
        Set 2: 72.5 lbs x 8
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exercise = result.Value.Exercises.Should().ContainSingle().Subject;

        // 67.5 lbs = 30.62 kg
        exercise.Sets[0].Weight.Should().Be(30.62m);
        // 72.5 lbs = 32.89 kg
        exercise.Sets[1].Weight.Should().Be(32.89m);
    }

    [Fact]
    public void Parse_MultipleExercisesWithDifferentUnits_ShouldNormalizeAll()
    {
        // Arrange
        var rawText = """
        Full Body Session
        Friday, August 20, 2026 at 5:00pm

        Bench Press
        Set 1: 185 lbs x 8
        Set 2: 190 lbs x 6

        Romanian Deadlift
        Set 1: 80 kg x 10
        Set 2: 90 kg x 8

        Overhead Press
        Set 1: 115 lb x 5
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Exercises.Should().HaveCount(3);

        // Bench Press - lbs to kg
        result.Value.Exercises[0].Sets[0].Weight.Should().Be(83.91m); // 185 lbs
        result.Value.Exercises[0].Sets[1].Weight.Should().Be(86.18m); // 190 lbs

        // Romanian Deadlift - kg remains
        result.Value.Exercises[1].Sets[0].Weight.Should().Be(80.00m);
        result.Value.Exercises[1].Sets[1].Weight.Should().Be(90.00m);

        // Overhead Press - lb to kg
        result.Value.Exercises[2].Sets[0].Weight.Should().Be(52.16m); // 115 lb
    }

    [Fact]
    public void Parse_ZeroWeight_ShouldHandleCorrectly()
    {
        // Arrange
        var rawText = """
        Bodyweight Workout
        Saturday, September 5, 2026 at 8:00am

        Pull-ups
        Set 1: 0 lbs x 15
        Set 2: 0 kg x 12
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exercise = result.Value.Exercises.Should().ContainSingle().Subject;

        exercise.Sets[0].Weight.Should().Be(0.00m);
        exercise.Sets[1].Weight.Should().Be(0.00m);
    }

    [Fact]
    public void Parse_CaseInsensitiveUnits_ShouldHandleCorrectly()
    {
        // Arrange
        var rawText = """
        Case Test
        Sunday, October 10, 2026 at 3:00pm

        Leg Press
        Set 1: 400 LBS x 12
        Set 2: 180 KG x 10
        Set 3: 350 Lbs x 15
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exercise = result.Value.Exercises.Should().ContainSingle().Subject;

        exercise.Sets[0].Weight.Should().Be(181.44m); // 400 lbs
        exercise.Sets[1].Weight.Should().Be(180.00m); // 180 kg
        exercise.Sets[2].Weight.Should().Be(158.76m); // 350 lbs
    }

    [Fact]
    public void Parse_WeightWithRpe_ShouldNormalizeWeightAndPreserveRpe()
    {
        // Arrange
        var rawText = """
        RPE Training
        Monday, November 1, 2026 at 6:00pm

        Back Squat
        Set 1: 275 lbs x 5 @ 8 rpe
        Set 2: 120 kg x 5 @ 9 rpe
        """;

        // Act
        var result = _parser.Parse(rawText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exercise = result.Value.Exercises.Should().ContainSingle().Subject;

        exercise.Sets[0].Weight.Should().Be(124.74m); // 275 lbs
        exercise.Sets[0].Rpe.Should().Be(8m);

        exercise.Sets[1].Weight.Should().Be(120.00m); // 120 kg
        exercise.Sets[1].Rpe.Should().Be(9m);
    }
}