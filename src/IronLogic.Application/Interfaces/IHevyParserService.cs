namespace IronLogic.Application.Interfaces;

public interface IHevyParserService
{
    IReadOnlyList<ExerciseRecord> Parse(string filePath);
}