using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

internal static class SkillAgentFactory
{
    public static AIAgent Create(
        ChatClientAgentOptions options,
        IServiceProvider? services = null,
        string deepSeekDefaultModel = "deepseek-v4-pro",
        string deepSeekDefaultEndpoint = "https://api.deepseek.com")
    {
        string provider = GetOptional("AGENT_PROVIDER") ?? "Foundry";
        if (provider.Equals("Foundry", StringComparison.OrdinalIgnoreCase))
        {
            string endpoint = GetOptional("FOUNDRY_PROJECT_ENDPOINT")
                ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT 未设置。");
            string model = GetOptional("FOUNDRY_MODEL")
                ?? "gpt-5.4-mini";
            options.ChatOptions ??= new ChatOptions();
            options.ChatOptions.ModelId = model;
            return new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
                .AsAIAgent(options: options, services: services);
        }

        if (provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
        {
            string apiKey = GetOptional("DEEPSEEK_API_KEY")
                ?? throw new InvalidOperationException("DEEPSEEK_API_KEY 未设置。");
            string endpoint = GetOptional("DEEPSEEK_ENDPOINT")
                ?? deepSeekDefaultEndpoint;
            string model = GetOptional("DEEPSEEK_MODEL")
                ?? deepSeekDefaultModel;
            return new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint),
            }).GetChatClient(model).AsAIAgent(options: options, services: services);
        }

        throw new InvalidOperationException($"不支持的 AGENT_PROVIDER：{provider}。请选择 Foundry 或 DeepSeek。");
    }

    private static string? GetOptional(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
