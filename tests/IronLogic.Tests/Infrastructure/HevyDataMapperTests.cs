using IronLogic.Application.Mappers;
using IronLogic.Domain.Entities;

namespace IronLogic.Tests.Infrastructure;

public class HevyDataMapperTests : IDisposable
{
    private readonly HevyDataMapper _sut = new();
    private readonly List<ExerciseRecord> _workoutData = LoadWorkoutData();

    public void Dispose()
    {
        // Nothing to dispose currently; reserved for future resource cleanup.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Loads sample records matching workout_data.csv content.
    /// </summary>
    private static List<ExerciseRecord> LoadWorkoutData()
    {
        var pushDate = new DateTime(2024, 6, 15);
        var pullDate = new DateTime(2024, 6, 13);

        return
        [
            new ExerciseRecord
            {
                Date = pushDate, WorkoutName = "Push Day", ExerciseName = "Bench Press", SetOrder = 1, Weight = 80,
                Reps = 10, RPE = 7
            },
            new ExerciseRecord
            {
                Date = pushDate, WorkoutName = "Push Day", ExerciseName = "Bench Press", SetOrder = 2, Weight = 85,
                Reps = 8, RPE = 8
            },
            new ExerciseRecord
            {
                Date = pushDate, WorkoutName = "Push Day", ExerciseName = "Bench Press", SetOrder = 3, Weight = 90,
                Reps = 6, RPE = 9
            },
            new ExerciseRecord
            {
                Date = pushDate, WorkoutName = "Push Day", ExerciseName = "Overhead Press", SetOrder = 1, Weight = 40,
                Reps = 12, RPE = 6
            },
            new ExerciseRecord
            {
                Date = pushDate, WorkoutName = "Push Day", ExerciseName = "Overhead Press", SetOrder = 2, Weight = 45,
                Reps = 10, RPE = 7
            },
            new ExerciseRecord
            {
                Date = pullDate, WorkoutName = "Pull Day", ExerciseName = "Barbell Row", SetOrder = 1, Weight = 70,
                Reps = 10, RPE = 7
            },
            new ExerciseRecord
            {
                Date = pullDate, WorkoutName = "Pull Day", ExerciseName = "Barbell Row", SetOrder = 2, Weight = 75,
                Reps = 8, RPE = 8
            },
            new ExerciseRecord
            {
                Date = pullDate, WorkoutName = "Pull Day", ExerciseName = "Deadlift", SetOrder = 1, Weight = 120,
                Reps = 5, RPE = 8
            },
            new ExerciseRecord
            {
                Date = pullDate, WorkoutName = "Pull Day", ExerciseName = "Deadlift", SetOrder = 2, Weight = 130,
                Reps = 4, RPE = 9
            },
            new ExerciseRecord
            {
                Date = pullDate, WorkoutName = "Pull Day", ExerciseName = "Deadlift", SetOrder = 3, Weight = 140,
                Reps = 3, RPE = 10
            }
        ];
    }

    [Fact]
    public void MapToSessions_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.MapToSessions(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MapToSessions_EmptyInput_ReturnsEmptyList()
    {
        var result = _sut.MapToSessions([]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void MapToSessions_WorkoutData_ReturnsTwoSessions()
    {
        var result = _sut.MapToSessions(_workoutData);

        result.Should().HaveCount(2);
    }

    [Fact]
    public void MapToSessions_WorkoutData_SessionsAreOrderedByDateDescending()
    {
        var result = _sut.MapToSessions(_workoutData);

        result.Should().BeInDescendingOrder(s => s.Date);
    }

    [Fact]
    public void MapToSessions_WorkoutData_FirstSessionIsPushDay()
    {
        var result = _sut.MapToSessions(_workoutData);

        var pushDay = result.First();
        pushDay.Name.Should().Be("Push Day");
        pushDay.Date.Should().Be(new DateTime(2024, 6, 15));
    }

    [Fact]
    public void MapToSessions_WorkoutData_PushDayHasTwoExercises()
    {
        var result = _sut.MapToSessions(_workoutData);

        var pushDay = result.First(s => s.Name == "Push Day");
        pushDay.Exercises.Should().HaveCount(2);
        pushDay.Exercises.Select(e => e.Name)
            .Should().Contain(["Bench Press", "Overhead Press"]);
    }

    [Fact]
    public void MapToSessions_WorkoutData_PullDayHasTwoExercises()
    {
        var result = _sut.MapToSessions(_workoutData);

        var pullDay = result.First(s => s.Name == "Pull Day");
        pullDay.Exercises.Should().HaveCount(2);
        pullDay.Exercises.Select(e => e.Name)
            .Should().Contain(["Barbell Row", "Deadlift"]);
    }

    [Fact]
    public void MapToSessions_WorkoutData_BenchPressHasThreeSets()
    {
        var result = _sut.MapToSessions(_workoutData);

        var benchPress = result
            .First(s => s.Name == "Push Day")
            .Exercises.First(e => e.Name == "Bench Press");

        benchPress.Sets.Should().HaveCount(3);
    }

    [Fact]
    public void MapToSessions_WorkoutData_SetsAreOrderedBySetOrder()
    {
        var result = _sut.MapToSessions(_workoutData);

        var deadliftSets = result
            .First(s => s.Name == "Pull Day")
            .Exercises.First(e => e.Name == "Deadlift")
            .Sets;

        deadliftSets.Should().BeInAscendingOrder(s => s.SetOrder);
    }

    [Fact]
    public void MapToSessions_WorkoutData_SetPropertiesAreMappedCorrectly()
    {
        var result = _sut.MapToSessions(_workoutData);

        var firstBenchSet = result
            .First(s => s.Name == "Push Day")
            .Exercises.First(e => e.Name == "Bench Press")
            .Sets.First();

        firstBenchSet.SetOrder.Should().Be(1);
        firstBenchSet.Weight.Should().Be(80);
        firstBenchSet.Reps.Should().Be(10);
        firstBenchSet.RPE.Should().Be(7);
    }

    [Fact]
    public void MapToSessions_SingleRecord_ReturnsSingleSessionWithSingleExerciseAndSingleSet()
    {
        var singleRecord = new ExerciseRecord
        {
            Date = new DateTime(2024, 7, 1),
            WorkoutName = "Leg Day",
            ExerciseName = "Squat",
            SetOrder = 1,
            Weight = 100,
            Reps = 5,
            RPE = 8
        };

        var result = _sut.MapToSessions([singleRecord]);

        result.Should().ContainSingle()
            .Which.Exercises.Should().ContainSingle()
            .Which.Sets.Should().ContainSingle();
    }

    [Fact]
    public void MapToSessions_WorkoutData_DeadliftLastSetHasMaxWeight()
    {
        var result = _sut.MapToSessions(_workoutData);

        var deadliftSets = result
            .First(s => s.Name == "Pull Day")
            .Exercises.First(e => e.Name == "Deadlift")
            .Sets;

        deadliftSets.Last().Weight.Should().Be(140);
        deadliftSets.Last().RPE.Should().Be(10);
    }

    [Fact]
    public void MapToSessions_SetsWithUnorderedInput_AreSortedBySetOrder()
    {
        var date = new DateTime(2024, 8, 1);
        var unorderedRecords = new List<ExerciseRecord>
        {
            new()
            {
                Date = date, WorkoutName = "Test", ExerciseName = "Curl", SetOrder = 3, Weight = 15, Reps = 8, RPE = 8
            },
            new()
            {
                Date = date, WorkoutName = "Test", ExerciseName = "Curl", SetOrder = 1, Weight = 10, Reps = 12, RPE = 6
            },
            new()
            {
                Date = date, WorkoutName = "Test", ExerciseName = "Curl", SetOrder = 2, Weight = 12, Reps = 10, RPE = 7
            }
        };

        var result = _sut.MapToSessions(unorderedRecords);

        var sets = result.Single().Exercises.Single().Sets;
        sets.Should().BeInAscendingOrder(s => s.SetOrder);
        sets.Select(s => s.Weight).Should().ContainInOrder(10, 12, 15);
    }
}