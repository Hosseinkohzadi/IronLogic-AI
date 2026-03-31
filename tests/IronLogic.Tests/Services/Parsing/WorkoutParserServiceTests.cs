using FluentAssertions;
using Xunit;

namespace IronLogic.Tests.Services.Parsing;

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
        
        exercise.Sets[0].Weight.Should().Be(108m);
        exercise.Sets[0].Reps.Should().Be(12);
        exercise.Sets[0].Rpe.Should().Be(8.5m);

        exercise.Sets[1].Weight.Should().Be(120.5m);
        exercise.Sets[1].Reps.Should().Be(10);
        exercise.Sets[1].Rpe.Should().BeNull();
        
        exercise.Sets[2].Weight.Should().Be(130m);
        exercise.Sets[2].Reps.Should().Be(8);
        exercise.Sets[2].Rpe.Should().Be(9m);
    }
}