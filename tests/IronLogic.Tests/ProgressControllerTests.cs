using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using IronLogic.Api.Controllers;
using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace IronLogic.Tests;

public class ProgressControllerTests : IDisposable
{
    private readonly IDailyWeightService _dailyWeightService = Substitute.For<IDailyWeightService>();
    private readonly IMuscleMeasurementService _muscleMeasurementService = Substitute.For<IMuscleMeasurementService>();
    private readonly ProgressController _sut;

    public ProgressControllerTests()
    {
        _sut = new ProgressController(_dailyWeightService, _muscleMeasurementService);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    // ── Helper: runs DataAnnotation validation on any model ──────────────
    private static List<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    // =====================================================================
    //  LogWeight – Controller Action Tests
    // =====================================================================

    [Fact]
    public async Task LogWeight_ValidRequest_Returns201Created()
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = 85.5f,
            Note = "Morning fasted"
        };

        var expectedEntry = new DailyWeight
        {
            Date = request.Date,
            Weight = request.Weight,
            Note = request.Note
        };

        _dailyWeightService
            .LogWeightAsync(request)
            .Returns(expectedEntry);

        var result = await _sut.LogWeight(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task LogWeight_ValidRequest_ReturnsLoggedEntryInBody()
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = 92.0f
        };

        var expectedEntry = new DailyWeight
        {
            Date = request.Date,
            Weight = request.Weight
        };

        _dailyWeightService
            .LogWeightAsync(request)
            .Returns(expectedEntry);

        var result = await _sut.LogWeight(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var body = createdResult.Value.Should().BeOfType<DailyWeight>().Subject;
        body.Weight.Should().Be(92.0f);
        body.Date.Should().Be(new DateTime(2026, 3, 24));
    }

    [Fact]
    public async Task LogWeight_ValidRequest_CallsServiceExactlyOnce()
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = 85.5f
        };

        _dailyWeightService
            .LogWeightAsync(request)
            .Returns(new DailyWeight());

        await _sut.LogWeight(request);

        await _dailyWeightService.Received(1).LogWeightAsync(request);
    }

    // =====================================================================
    //  DailyWeightRequest – Validation Tests (OpenAPI spec constraints)
    // =====================================================================

    [Fact]
    public void DailyWeightRequest_ValidData_PassesValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = 85.5f,
            Note = "Post-refeed"
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void DailyWeightRequest_WeightAtMinimumBoundary_PassesValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = 40f
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void DailyWeightRequest_WeightAtMaximumBoundary_PassesValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = 200f
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(39.9f)]
    [InlineData(0f)]
    [InlineData(-10f)]
    public void DailyWeightRequest_WeightBelowMinimum_FailsValidation(float weight)
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = weight
        };

        var errors = ValidateModel(request);

        errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Weight must be between 40 and 200 kg");
    }

    [Theory]
    [InlineData(200.1f)]
    [InlineData(300f)]
    public void DailyWeightRequest_WeightAboveMaximum_FailsValidation(float weight)
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = weight
        };

        var errors = ValidateModel(request);

        errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Weight must be between 40 and 200 kg");
    }

    [Fact]
    public void DailyWeightRequest_NoteExceeds200Characters_FailsValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = 85.5f,
            Note = new string('A', 201)
        };

        var errors = ValidateModel(request);

        errors.Should().ContainSingle();
    }

    [Fact]
    public void DailyWeightRequest_NoteExactly200Characters_PassesValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = 85.5f,
            Note = new string('A', 200)
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void DailyWeightRequest_NullNote_PassesValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = new DateTime(2026, 3, 24),
            Weight = 85.5f,
            Note = null
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    // =====================================================================
    //  DailyWeightRequest – Future Date Validation (Business Rule)
    // =====================================================================

    [Fact]
    public void DailyWeightRequest_FutureDate_FailsValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = DateTime.UtcNow.Date.AddDays(1),
            Weight = 85.5f
        };

        var errors = ValidateModel(request);

        errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Date cannot be in the future");
    }

    [Fact]
    public void DailyWeightRequest_FarFutureDate_FailsValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = DateTime.UtcNow.Date.AddYears(1),
            Weight = 85.5f
        };

        var errors = ValidateModel(request);

        errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Date cannot be in the future");
    }

    [Fact]
    public void DailyWeightRequest_TodayDate_PassesValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = DateTime.UtcNow.Date,
            Weight = 85.5f
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void DailyWeightRequest_PastDate_PassesValidation()
    {
        var request = new DailyWeightRequest
        {
            Date = DateTime.UtcNow.Date.AddDays(-7),
            Weight = 85.5f
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    // =====================================================================
    //  MuscleMeasurementRequest – Waist Range Validation (OpenAPI spec)
    // =====================================================================

    [Fact]
    public void MuscleMeasurementRequest_ValidData_PassesValidation()
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 40f,
            Chest = 110f,
            Waist = 82f
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void MuscleMeasurementRequest_WaistAtMinimumBoundary_PassesValidation()
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 38f,
            Chest = 105f,
            Waist = 40f
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void MuscleMeasurementRequest_WaistAtMaximumBoundary_PassesValidation()
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 38f,
            Chest = 105f,
            Waist = 150f
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(39.9f)]
    [InlineData(0f)]
    [InlineData(-5f)]
    public void MuscleMeasurementRequest_WaistBelowMinimum_FailsValidation(float waist)
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 38f,
            Chest = 105f,
            Waist = waist
        };

        var errors = ValidateModel(request);

        errors.Should().Contain(e => e.ErrorMessage!.Contains("Waist must be between 40 and 150 cm"));
    }

    [Theory]
    [InlineData(150.1f)]
    [InlineData(200f)]
    public void MuscleMeasurementRequest_WaistAboveMaximum_FailsValidation(float waist)
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 38f,
            Chest = 105f,
            Waist = waist
        };

        var errors = ValidateModel(request);

        errors.Should().Contain(e => e.ErrorMessage!.Contains("Waist must be between 40 and 150 cm"));
    }

    [Theory]
    [InlineData(19.9f)]
    [InlineData(60.1f)]
    public void MuscleMeasurementRequest_NeckOutOfRange_FailsValidation(float neck)
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = neck,
            Chest = 105f,
            Waist = 80f
        };

        var errors = ValidateModel(request);

        errors.Should().Contain(e => e.ErrorMessage!.Contains("Neck must be between 20 and 60 cm"));
    }

    [Theory]
    [InlineData(49.9f)]
    [InlineData(180.1f)]
    public void MuscleMeasurementRequest_ChestOutOfRange_FailsValidation(float chest)
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 38f,
            Chest = chest,
            Waist = 80f
        };

        var errors = ValidateModel(request);

        errors.Should().Contain(e => e.ErrorMessage!.Contains("Chest must be between 50 and 180 cm"));
    }

    [Fact]
    public void MuscleMeasurementRequest_OptionalFieldsNull_PassesValidation()
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 38f,
            Chest = 105f,
            Waist = 80f,
            BicepsLeft = null,
            BicepsRight = null,
            ThighLeft = null,
            ThighRight = null
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }

    // =====================================================================
    //  LogMeasurements – Controller Action Tests
    // =====================================================================

    [Fact]
    public async Task LogMeasurements_ValidRequest_Returns201Created()
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 40f,
            Chest = 110f,
            Waist = 82f,
            BicepsLeft = 38f,
            BicepsRight = 39f,
            ThighLeft = 60f,
            ThighRight = 61f
        };

        var expectedEntry = new MuscleMeasurement
        {
            Date = request.Date,
            Neck = request.Neck,
            Chest = request.Chest,
            Waist = request.Waist,
            BicepsLeft = request.BicepsLeft,
            BicepsRight = request.BicepsRight,
            ThighLeft = request.ThighLeft,
            ThighRight = request.ThighRight
        };

        _muscleMeasurementService
            .LogMeasurementAsync(request)
            .Returns(expectedEntry);

        var result = await _sut.LogMeasurements(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task LogMeasurements_ValidRequest_ReturnsLoggedEntryInBody()
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 40f,
            Chest = 110f,
            Waist = 82f
        };

        var expectedEntry = new MuscleMeasurement
        {
            Date = request.Date,
            Neck = request.Neck,
            Chest = request.Chest,
            Waist = request.Waist
        };

        _muscleMeasurementService
            .LogMeasurementAsync(request)
            .Returns(expectedEntry);

        var result = await _sut.LogMeasurements(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var body = createdResult.Value.Should().BeOfType<MuscleMeasurement>().Subject;
        body.Neck.Should().Be(40f);
        body.Chest.Should().Be(110f);
        body.Waist.Should().Be(82f);
    }

    [Fact]
    public async Task LogMeasurements_ValidRequest_CallsServiceExactlyOnce()
    {
        var request = new MuscleMeasurementRequest
        {
            Date = new DateTime(2026, 3, 24),
            Neck = 40f,
            Chest = 110f,
            Waist = 82f
        };

        _muscleMeasurementService
            .LogMeasurementAsync(request)
            .Returns(new MuscleMeasurement());

        await _sut.LogMeasurements(request);

        await _muscleMeasurementService.Received(1).LogMeasurementAsync(request);
    }

    // =====================================================================
    //  MuscleMeasurementRequest – Future Date Validation (Business Rule)
    // =====================================================================

    [Fact]
    public void MuscleMeasurementRequest_FutureDate_FailsValidation()
    {
        var request = new MuscleMeasurementRequest
        {
            Date = DateTime.UtcNow.Date.AddDays(1),
            Neck = 38f,
            Chest = 105f,
            Waist = 80f
        };

        var errors = ValidateModel(request);

        errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Date cannot be in the future");
    }

    [Fact]
    public void MuscleMeasurementRequest_TodayDate_PassesValidation()
    {
        var request = new MuscleMeasurementRequest
        {
            Date = DateTime.UtcNow.Date,
            Neck = 38f,
            Chest = 105f,
            Waist = 80f
        };

        var errors = ValidateModel(request);

        errors.Should().BeEmpty();
    }
}