using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
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

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath, System.Text.Encoding.UTF8);
        using var csv = new CsvReader(reader, config);

        csv.Context.RegisterClassMap<ExerciseRecordMap>();

        var records = csv.GetRecords<ExerciseRecord>().ToList();

        return records;
    }
}