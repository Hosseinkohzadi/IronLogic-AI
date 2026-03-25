using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using IronLogic.Domain.Entities;
using IronLogic.Tests.Infrastructure;

namespace IronLogic.Tests.IntegrationTests;

/// <summary>
///     Integration tests for POST /api/v1/progress/weight (DailyWeight endpoint).
///     Uses WebApplicationFactory with an EF Core InMemory database.
///     Test cases are derived from the OpenAPI specification (openapi.yaml).
/// </summary>
public class DailyWeightIntegrationTests(WebApplicationFactory factory)
    : IClassFixture<WebApplicationFactory>, IDisposable
{
    private const string Endpoint = "/api/v1/progress/weight";

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
    //  201 Created — Happy Path (OpenAPI: responses.201)
    // =====================================================================

    [Fact]
    public async Task PostWeight_ValidPayload_Returns201Created()
    {
        // Arrange — matches the OpenAPI example exactly
        var payload = new { date = "2026-03-24", weight = 85.5f, note = "Morning fasted" };

        // Act
        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostWeight_ValidPayload_ReturnsPersistedEntity()
    {
        // Arrange
        var payload = new { date = "2026-03-24", weight = 92.0f, note = "Post-refeed" };

        // Act
        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<DailyWeight>(JsonOptions,
            TestContext.Current.CancellationToken);

        // Assert
        body.Should().NotBeNull();
        body.Id.Should().NotBeEmpty();
        body.Weight.Should().Be(92.0f);
        body.Date.Should().Be(new DateTime(2026, 3, 24));
        body.Note.Should().Be("Post-refeed");
    }

    [Fact]
    public async Task PostWeight_WithoutOptionalNote_Returns201Created()
    {
        // Arrange — OpenAPI spec: "note" is NOT in the required array
        var payload = new { date = "2026-03-24", weight = 85.5f };

        // Act
        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostWeight_WithoutOptionalNote_ReturnsNullNote()
    {
        // Arrange
        var payload = new { date = "2026-03-24", weight = 85.5f };

        // Act
        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<DailyWeight>(JsonOptions,
            TestContext.Current.CancellationToken);

        // Assert
        body.Should().NotBeNull();
        body.Note.Should().BeNull();
    }

    // =====================================================================
    //  201 — Weight Boundary Tests (OpenAPI: minimum: 40, maximum: 200)
    // =====================================================================

    [Fact]
    public async Task PostWeight_AtMinimumBoundary40_Returns201Created()
    {
        var payload = new { date = "2026-03-24", weight = 40.0f };

        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostWeight_AtMaximumBoundary200_Returns201Created()
    {
        var payload = new { date = "2026-03-24", weight = 200.0f };

        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // =====================================================================
    //  400 Bad Request — Weight Out of Range (OpenAPI constraints)
    // =====================================================================

    [Theory]
    [InlineData(39.0f)]
    [InlineData(0f)]
    [InlineData(-10f)]
    public async Task PostWeight_BelowMinimum_Returns400BadRequest(float weight)
    {
        var payload = new { date = "2026-03-24", weight };

        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(201.0f)]
    [InlineData(300f)]
    public async Task PostWeight_AboveMaximum_Returns400BadRequest(float weight)
    {
        var payload = new { date = "2026-03-24", weight };

        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  400 Bad Request — Missing Required Fields (OpenAPI: required: [date, weight])
    // =====================================================================

    [Fact]
    public async Task PostWeight_MissingWeight_Returns400BadRequest()
    {
        // Only "date" provided — "weight" is required per spec
        var payload = new { date = "2026-03-24" };

        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostWeight_EmptyBody_Returns400BadRequest()
    {
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(Endpoint, content, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  400 Bad Request — Invalid Date Formats
    // =====================================================================

    [Fact]
    public async Task PostWeight_InvalidDateFormat_Returns400BadRequest()
    {
        var json = """{ "date": "not-a-date", "weight": 85.5 }""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(Endpoint, content, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostWeight_FutureDate_Returns400BadRequest()
    {
        // Business rule: cannot log future dates
        var futureDate = DateTime.UtcNow.Date.AddDays(5).ToString("yyyy-MM-dd");
        var payload = new { date = futureDate, weight = 85.5f };

        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  400 Bad Request — Note Exceeds MaxLength (OpenAPI: maxLength: 200)
    // =====================================================================

    [Fact]
    public async Task PostWeight_NoteExceeds200Characters_Returns400BadRequest()
    {
        var payload = new { date = "2026-03-24", weight = 85.5f, note = new string('A', 201) };

        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostWeight_NoteExactly200Characters_Returns201Created()
    {
        var payload = new { date = "2026-03-24", weight = 85.5f, note = new string('A', 200) };

        var response =
            await _client.PostAsJsonAsync(Endpoint, payload, JsonOptions, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // =====================================================================
    //  400 Bad Request — Invalid Content Type & Malformed JSON
    // =====================================================================

    [Fact]
    public async Task PostWeight_MalformedJson_Returns400BadRequest()
    {
        var content = new StringContent("{ broken json }", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(Endpoint, content, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================================
    //  Multiple Sequential Posts — Data Persistence
    // =====================================================================

    [Fact]
    public async Task PostWeight_TwoConsecutivePosts_BothReturn201()
    {
        var payload1 = new { date = "2026-03-20", weight = 84.0f, note = "Day 1" };
        var payload2 = new { date = "2026-03-21", weight = 84.3f, note = "Day 2" };

        var response1 = async () =>
            await _client.PostAsJsonAsync(Endpoint, payload1, JsonOptions, TestContext.Current.CancellationToken);
        var response2 = async () =>
            await _client.PostAsJsonAsync(Endpoint, payload2, JsonOptions, TestContext.Current.CancellationToken);

        (await response1()).StatusCode.Should().Be(HttpStatusCode.Created);
        (await response2()).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostWeight_TwoConsecutivePosts_EachGetUniqueId()
    {
        var payload1 = new { date = "2026-03-22", weight = 85.0f };
        var payload2 = new { date = "2026-03-23", weight = 85.5f };

        var response1 =
            await _client.PostAsJsonAsync(Endpoint, payload1, JsonOptions, TestContext.Current.CancellationToken);
        var response2 =
            await _client.PostAsJsonAsync(Endpoint, payload2, JsonOptions, TestContext.Current.CancellationToken);

        var body1 = await response1.Content.ReadFromJsonAsync<DailyWeight>(JsonOptions,
            TestContext.Current.CancellationToken);
        var body2 = await response2.Content.ReadFromJsonAsync<DailyWeight>(JsonOptions,
            TestContext.Current.CancellationToken);

        body1!.Id.Should().NotBe(body2!.Id);
    }
}