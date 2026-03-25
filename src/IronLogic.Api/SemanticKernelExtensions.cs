using Microsoft.SemanticKernel;

namespace IronLogic.Api;

public static class SemanticKernelExtensions
{
    /// <summary>
    ///     Registers Semantic Kernel, OpenAI chat completion, and IChatCompletionService.
    /// </summary>
    public static IServiceCollection AddSemanticKernel(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o-mini";
        var apiKey = configuration["OpenAI:ApiKey"] ?? "sk-fake-key-replace-me";

        services.AddScoped<Kernel>(_ =>
        {
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddOpenAIChatCompletion(modelId, apiKey);
            return kernelBuilder.Build();
        });

        services.AddScoped(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<IChatCompletionService>();
        });

        return services;
    }
}