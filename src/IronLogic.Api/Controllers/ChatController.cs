using System.Text;
using IronLogic.Application.Services;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace IronLogic.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
[Tags("AI")]
public class ChatController(
    Kernel kernel,
    IChatCompletionService chatCompletionService,
    BodybuildingCoachPlugin bodybuildingPlugin)
    : ControllerBase
{
    /// <summary>
    ///     POST /api/v1/ai/ask
    ///     Accepts a user prompt and optional measurement/weight data. If the prompt requests progress analysis,
    ///     the controller uses the BodybuildingCoachPlugin as a tool (direct call) to produce data-driven outputs,
    ///     then forwards a composed prompt to the configured chat completion service for a natural response.
    /// </summary>
    [HttpPost("ask")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Ask([FromBody] AskRequest? request)
    {
        if (request is null)
            return Problem(
                "Request body is required.",
                title: "Bad Request",
                statusCode: StatusCodes.Status400BadRequest
            );

        if (string.IsNullOrWhiteSpace(request.Prompt))
            return Problem(
                "Prompt cannot be empty.",
                title: "Bad Request",
                statusCode: StatusCodes.Status400BadRequest
            );

        try
        {
            // Collect deterministic tool outputs from the BodybuildingCoachPlugin when applicable.
            var toolOutputs = new List<string>();

            if (request.Measurement is not null &&
                request.Measurement.Chest.HasValue &&
                request.Measurement.Waist.HasValue)
            {
                var mm = new MuscleMeasurement
                {
                    Date = request.Measurement.Date ?? DateTime.UtcNow,
                    Neck = request.Measurement.Neck ?? 0f,
                    Chest = request.Measurement.Chest!.Value,
                    Waist = request.Measurement.Waist!.Value,
                    BicepsLeft = request.Measurement.BicepsLeft,
                    BicepsRight = request.Measurement.BicepsRight,
                    ThighLeft = request.Measurement.ThighLeft,
                    ThighRight = request.Measurement.ThighRight
                };

                // Use the plugin as a tool to compute V-taper classification.
                var vtaper = bodybuildingPlugin.AnalyzeVTaper(mm);
                toolOutputs.Add(
                    $"V-Taper: {vtaper} (chest: {mm.Chest} cm, waist: {mm.Waist} cm, ratio: {(mm.Waist > 0 ? (mm.Chest / mm.Waist).ToString("0.00") : "n/a")})");
            }

            if (request.RecentWeights is not null && request.RecentWeights.Count >= 2 && request.RepsVolume.HasValue)
            {
                var weights = request.RecentWeights
                    .Where(w => w is { Weight: not null, Date: not null })
                    .OrderBy(w => w.Date!.Value)
                    .Select(w => new DailyWeight { Date = w.Date!.Value, Weight = w.Weight!.Value, Note = w.Note })
                    .ToList();

                if (weights.Count >= 2)
                {
                    var advice = bodybuildingPlugin.SmartCutAdvice(weights, request.RepsVolume!.Value);
                    toolOutputs.Add($"SmartCutAdvice: {advice}");
                }
            }

            // Compose a prompt for the chat completion service that includes deterministic tool outputs.
            var composed = new StringBuilder();
            composed.AppendLine("User prompt:");
            composed.AppendLine(request.Prompt.Trim());
            composed.AppendLine();

            if (toolOutputs.Any())
            {
                composed.AppendLine("Tool outputs (data-driven):");
                foreach (var t in toolOutputs) composed.AppendLine($"- {t}");
                composed.AppendLine();
            }

            composed.AppendLine("Please respond concisely and refer to the data above when making recommendations.");

            // Call out to the injected chat completion service to produce the final natural-language response.
            // NOTE: this controller expects IChatCompletionService to expose an async method that accepts a string
            // and returns a string. Replace the method call below if your concrete interface uses a different name/signature.
            var aiResponse = await chatCompletionService.GetChatMessageContentsAsync(composed.ToString());

            // Return the AI answer and the tool outputs for transparency.
            return Ok(new
            {
                answer = aiResponse,
                tools = toolOutputs
            });
        }
        catch (ArgumentException ex)
        {
            // Business validation / plugin input errors -> 400 with ProblemDetails
            return Problem(
                ex.Message,
                title: "Invalid input",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            // Unexpected errors -> 500 with ProblemDetails (do not expose stack traces)
            return Problem(
                "An unexpected error occurred while processing the request.",
                title: "Internal Server Error",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    ///     Request payload for the AI chat endpoint.
    /// </summary>
    public sealed class AskRequest
    {
        /// <summary>
        ///     The user's free-text question for the assistant.
        /// </summary>
        public string? Prompt { get; set; }

        /// <summary>
        ///     Optional measurement the assistant can use to provide a data-driven V-taper analysis.
        /// </summary>
        public MuscleMeasurementPayload? Measurement { get; set; }

        /// <summary>
        ///     Optional recent weights (chronological) used for trend analysis.
        /// </summary>
        public List<DailyWeightPayload>? RecentWeights { get; set; }

        /// <summary>
        ///     Optional aggregated reps-volume for the same observation window used by SmartCutAdvice.
        ///     Convention: caller supplies a non-negative numeric representation (e.g., total reps*weight).
        /// </summary>
        public double? RepsVolume { get; set; }
    }

    /// <summary>
    ///     Minimal DTO for muscle measurement accepted by the AI endpoint.
    ///     Mirrors the domain entity fields used by the plugin.
    /// </summary>
    public sealed class MuscleMeasurementPayload
    {
        public DateTime? Date { get; set; }
        public float? Neck { get; set; }
        public float? Chest { get; set; }
        public float? Waist { get; set; }
        public float? BicepsLeft { get; set; }
        public float? BicepsRight { get; set; }
        public float? ThighLeft { get; set; }
        public float? ThighRight { get; set; }
    }

    /// <summary>
    ///     Minimal DTO for daily weight accepted by the AI endpoint.
    /// </summary>
    public sealed class DailyWeightPayload
    {
        public DateTime? Date { get; set; }
        public float? Weight { get; set; }
        public string? Note { get; set; }
    }
}