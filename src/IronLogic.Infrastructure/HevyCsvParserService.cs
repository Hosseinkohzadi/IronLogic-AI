using System.Globalization;
using CsvHelper;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;

namespace IronLogic.Infrastructure;

public class HevyCsvParserService : IHevyParserService
{
    public IReadOnlyList<ExerciseRecord> Parse(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found at: {filePath}");

        using var reader = new StreamReader(filePath, System.Text.Encoding.UTF8);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<ExerciseRecordMap>();

        var records = csv.GetRecords<ExerciseRecord>().ToList();

        return records;
    }
}