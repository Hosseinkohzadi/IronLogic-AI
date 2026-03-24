using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using IronLogic.Domain.Entities;
using IronLogic.Tests.Infrastructure;

namespace IronLogic.Tests;

/// <summary>
/// Integration tests for POST /api/v1/progress/measurements (MuscleMeasurement endpoint).
/// Uses WebApplicationFactory with an EF Core InMemory database.
/// Test cases are derived from the OpenAPI specification (openapi.yaml).
/// Special focus on Waist and Chest validation ranges and their persisted ratios,
/// which are critical metrics for Classic Physique bodybuilding.
/// </summary>
public class MuscleMeasurementIntegrationTests(IronLogicWebApplicationFactory factory)
    : IClassFixture<IronLogicWebApplicationFactory>, IDisposable
{
    private const string Endpoint = "/api/v1/progress/measurements";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _client = factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    // =====================================================================
    //  201 Created - Happy Path (OpenAPI: responses.201)
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_ValidFullPayload_Returns201Created()
    {
        // Arrange - all required + all optional fields
        var payload = new
        {
            date = "2026-03-24",
            neck = 40.0f,
            chest = 115.0f,
            waist = 78.0f,
            bicepsLeft = 42.0f,
            bicepsRight = 42.5f,
            thighLeft = 65.0f,
            thighRight = 65.5f
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostMeasurements_ValidFullPayload_ReturnsPersistedEntity()
    {
        // Arrange
        var payload = new
        {
            date = "2026-03-24",
            neck = 40.0f,
            chest = 115.0f,
            waist = 78.0f,
            bicepsLeft = 42.0f,
            bicepsRight = 42.5f,
            thighLeft = 65.0f,
            thighRight = 65.5f
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        // Assert
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Date.Should().Be(new DateTime(2026, 3, 24));
        body.Neck.Should().Be(40.0f);
        body.Chest.Should().Be(115.0f);
        body.Waist.Should().Be(78.0f);
        body.BicepsLeft.Should().Be(42.0f);
        body.BicepsRight.Should().Be(42.5f);
        body.ThighLeft.Should().Be(65.0f);
        body.ThighRight.Should().Be(65.5f);
    }

    [Fact]
    public async Task PostMeasurements_OnlyRequiredFields_Returns201Created()
    {
        // Arrange - OpenAPI required: [date, neck, chest, waist]
        var payload = new
        {
            date = "2026-03-24",
            neck = 38.0f,
            chest = 110.0f,
            waist = 82.0f
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostMeasurements_OnlyRequiredFields_OptionalFieldsReturnNull()
    {
        // Arrange
        var payload = new
        {
            date = "2026-03-24",
            neck = 38.0f,
            chest = 110.0f,
            waist = 82.0f
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        // Assert
        body.Should().NotBeNull();
        body!.BicepsLeft.Should().BeNull();
        body.BicepsRight.Should().BeNull();
        body.ThighLeft.Should().BeNull();
        body.ThighRight.Should().BeNull();
    }

    // =====================================================================
    //  201 - Waist Boundary Tests (OpenAPI: minimum: 40, maximum: 150)
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_WaistAtMinimum40_Returns201Created()
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 100.0f, waist = 40.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostMeasurements_WaistAtMaximum150_Returns201Created()
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 100.0f, waist = 150.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostMeasurements_WaistAtMinimum_PersistsCorrectValue()
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 100.0f, waist = 40.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        body.Should().NotBeNull();
        body!.Waist.Should().Be(40.0f);
    }

    [Fact]
    public async Task PostMeasurements_WaistAtMaximum_PersistsCorrectValue()
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 100.0f, waist = 150.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        body.Should().NotBeNull();
        body!.Waist.Should().Be(150.0f);
    }

    // =====================================================================
    //  400 - Waist Out of Range (OpenAPI constraints)
    // =====================================================================

    [Theory]
    [InlineData(39.0f)]
    [InlineData(0f)]
    [InlineData(-5f)]
    public async Task PostMeasurements_WaistBelowMinimum_Returns400BadRequest(float waist)
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 100.0f, waist };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(151.0f)]
    [InlineData(200.0f)]
    public async Task PostMeasurements_WaistAboveMaximum_Returns400BadRequest(float waist)
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 100.0f, waist };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  201 - Chest Boundary Tests (OpenAPI: minimum: 50, maximum: 180)
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_ChestAtMinimum50_Returns201Created()
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 50.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostMeasurements_ChestAtMaximum180_Returns201Created()
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 180.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostMeasurements_ChestAtMinimum_PersistsCorrectValue()
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 50.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        body.Should().NotBeNull();
        body!.Chest.Should().Be(50.0f);
    }

    [Fact]
    public async Task PostMeasurements_ChestAtMaximum_PersistsCorrectValue()
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest = 180.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        body.Should().NotBeNull();
        body!.Chest.Should().Be(180.0f);
    }

    // =====================================================================
    //  400 - Chest Out of Range (OpenAPI constraints)
    // =====================================================================

    [Theory]
    [InlineData(49.0f)]
    [InlineData(0f)]
    [InlineData(-10f)]
    public async Task PostMeasurements_ChestBelowMinimum_Returns400BadRequest(float chest)
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(181.0f)]
    [InlineData(250.0f)]
    public async Task PostMeasurements_ChestAboveMaximum_Returns400BadRequest(float chest)
    {
        var payload = new { date = "2026-03-24", neck = 35.0f, chest, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  Chest-to-Waist Ratio - Database Persistence Verification
    //  (Classic Physique golden ratio target: chest/waist ~1.4)
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_ClassicPhysiqueRatio_PersistsBothValuesForRatioCalc()
    {
        // Arrange - classic bodybuilding proportions: 46-inch chest, 29-inch waist (~1.59 ratio)
        var payload = new
        {
            date = "2026-03-24",
            neck = 43.0f,
            chest = 117.0f,  // ~46 inches
            waist = 73.5f    // ~29 inches
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        // Assert - values are persisted accurately for downstream ratio calculation
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();
        body!.Chest.Should().Be(117.0f);
        body.Waist.Should().Be(73.5f);

        var chestToWaistRatio = body.Chest / body.Waist;
        chestToWaistRatio.Should().BeApproximately(1.59f, 0.01f);
    }

    [Fact]
    public async Task PostMeasurements_WideDifferenceChestWaist_PersistsAccurately()
    {
        // Arrange - extreme V-taper: large chest, tight waist
        var payload = new
        {
            date = "2026-03-24",
            neck = 42.0f,
            chest = 140.0f,
            waist = 75.0f
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();
        body!.Chest.Should().Be(140.0f);
        body.Waist.Should().Be(75.0f);

        var ratio = body.Chest / body.Waist;
        ratio.Should().BeGreaterThan(1.0f, "Chest must always exceed waist for a V-taper physique");
    }

    [Fact]
    public async Task PostMeasurements_NarrowDifferenceChestWaist_PersistsAccurately()
    {
        // Arrange - off-season bulk: chest and waist closer together
        var payload = new
        {
            date = "2026-03-24",
            neck = 40.0f,
            chest = 110.0f,
            waist = 100.0f
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();
        body!.Chest.Should().Be(110.0f);
        body.Waist.Should().Be(100.0f);

        var ratio = body.Chest / body.Waist;
        ratio.Should().BeApproximately(1.1f, 0.01f);
    }

    // =====================================================================
    //  201 - Neck Boundary Tests (OpenAPI: minimum: 20, maximum: 60)
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_NeckAtMinimum20_Returns201Created()
    {
        var payload = new { date = "2026-03-24", neck = 20.0f, chest = 100.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostMeasurements_NeckAtMaximum60_Returns201Created()
    {
        var payload = new { date = "2026-03-24", neck = 60.0f, chest = 100.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // =====================================================================
    //  400 - Neck Out of Range (OpenAPI constraints)
    // =====================================================================

    [Theory]
    [InlineData(19.0f)]
    [InlineData(0f)]
    public async Task PostMeasurements_NeckBelowMinimum_Returns400BadRequest(float neck)
    {
        var payload = new { date = "2026-03-24", neck, chest = 100.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(61.0f)]
    [InlineData(100.0f)]
    public async Task PostMeasurements_NeckAboveMaximum_Returns400BadRequest(float neck)
    {
        var payload = new { date = "2026-03-24", neck, chest = 100.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  400 - Missing Required Fields (OpenAPI: required: [date, neck, chest, waist])
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_MissingNeck_Returns400BadRequest()
    {
        var payload = new { date = "2026-03-24", chest = 110.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMeasurements_MissingChest_Returns400BadRequest()
    {
        var payload = new { date = "2026-03-24", neck = 38.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMeasurements_MissingWaist_Returns400BadRequest()
    {
        var payload = new { date = "2026-03-24", neck = 38.0f, chest = 110.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMeasurements_EmptyBody_Returns400BadRequest()
    {
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(Endpoint, content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  400 - Invalid Date (Business Rule + Format)
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_InvalidDateFormat_Returns400BadRequest()
    {
        var json = """{ "date": "not-a-date", "neck": 38.0, "chest": 110.0, "waist": 80.0 }""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(Endpoint, content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMeasurements_FutureDate_Returns400BadRequest()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(5).ToString("yyyy-MM-dd");
        var payload = new { date = futureDate, neck = 38.0f, chest = 110.0f, waist = 80.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  400 - Malformed JSON
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_MalformedJson_Returns400BadRequest()
    {
        var content = new StringContent("{ broken json }", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(Endpoint, content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  Multiple Sequential Posts - Data Persistence & Unique IDs
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_TwoConsecutivePosts_BothReturn201()
    {
        var payload1 = new { date = "2026-03-20", neck = 38.0f, chest = 110.0f, waist = 80.0f };
        var payload2 = new { date = "2026-03-21", neck = 38.5f, chest = 111.0f, waist = 79.5f };

        var response1 = await _client.PostAsJsonAsync(Endpoint, payload1, JsonOptions);
        var response2 = await _client.PostAsJsonAsync(Endpoint, payload2, JsonOptions);

        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostMeasurements_TwoConsecutivePosts_EachGetUniqueId()
    {
        var payload1 = new { date = "2026-03-22", neck = 38.0f, chest = 110.0f, waist = 80.0f };
        var payload2 = new { date = "2026-03-23", neck = 38.0f, chest = 112.0f, waist = 79.0f };

        var response1 = await _client.PostAsJsonAsync(Endpoint, payload1, JsonOptions);
        var response2 = await _client.PostAsJsonAsync(Endpoint, payload2, JsonOptions);

        var body1 = await response1.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);
        var body2 = await response2.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        body1!.Id.Should().NotBe(body2!.Id);
    }

    [Fact]
    public async Task PostMeasurements_TwoPostsDifferentRatios_PersistEachCorrectly()
    {
        // Arrange - contest shape vs off-season
        var contestPayload = new { date = "2026-03-20", neck = 42.0f, chest = 120.0f, waist = 73.0f };
        var offseasonPayload = new { date = "2026-03-21", neck = 44.0f, chest = 125.0f, waist = 95.0f };

        // Act
        var contestResponse = await _client.PostAsJsonAsync(Endpoint, contestPayload, JsonOptions);
        var offseasonResponse = await _client.PostAsJsonAsync(Endpoint, offseasonPayload, JsonOptions);

        var contestBody = await contestResponse.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);
        var offseasonBody = await offseasonResponse.Content.ReadFromJsonAsync<MuscleMeasurement>(JsonOptions);

        // Assert - each entry persists its own distinct chest/waist values
        contestBody!.Chest.Should().Be(120.0f);
        contestBody.Waist.Should().Be(73.0f);
        offseasonBody!.Chest.Should().Be(125.0f);
        offseasonBody.Waist.Should().Be(95.0f);

        // Contest shape should have a superior ratio
        var contestRatio = contestBody.Chest / contestBody.Waist;
        var offseasonRatio = offseasonBody.Chest / offseasonBody.Waist;
        contestRatio.Should().BeGreaterThan(offseasonRatio);
    }

    // =====================================================================
    //  400 - Multiple Validation Failures at Once
    // =====================================================================

    [Fact]
    public async Task PostMeasurements_AllRequiredFieldsOutOfRange_Returns400BadRequest()
    {
        // neck below 20, chest below 50, waist above 150
        var payload = new { date = "2026-03-24", neck = 10.0f, chest = 30.0f, waist = 200.0f };

        var response = await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
