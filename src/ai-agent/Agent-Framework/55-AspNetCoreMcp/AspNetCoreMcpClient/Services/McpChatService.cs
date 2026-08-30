using AspNetCoreCallMcpServer.Options;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using System.Net.Http.Json;

namespace AspNetCoreCallMcpServer.Services;

public sealed class McpChatService(
    FoundryOptions foundryOptions,
    IOptions<McpServerOptions> mcpServerOptions,
    ILoggerFactory loggerFactory)
{
    public async Task<McpChatResult> AskAsync(string message, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (!Uri.TryCreate(foundryOptions.ProjectEndpoint, UriKind.Absolute, out var projectEndpoint))
        {
            throw new InvalidOperationException(
                "未配置有效的 FOUNDRY_PROJECT_ENDPOINT。请设置环境变量后重启应用。");
        }

        await using var mcpClient = await CreateMcpClientAsync(cancellationToken);
        var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);
        List<AITool> tools = [.. mcpTools.Cast<AITool>()];

        AIProjectClient projectClient = new(projectEndpoint, new DefaultAzureCredential());
        AIAgent agent = projectClient.AsAIAgent(
            model: foundryOptions.Model,
            instructions: "你是一个乐于助人的助手。根据用户的自然语言请求，选择并调用可用的 MCP 工具；使用工具结果准确回答，并明确说明工具无法完成的请求。",
            name: "WebMcpAgent",
            tools: tools);

        AgentResponse response = await agent.RunAsync(message, cancellationToken: cancellationToken);

        return new McpChatResult(response.ToString(), mcpTools.Select(tool => tool.Name).ToArray());
    }

    private async Task<McpClient> CreateMcpClientAsync(CancellationToken cancellationToken)
    {
        var additionalHeaders = await GetAuthenticationHeadersAsync(cancellationToken);
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(mcpServerOptions.Value.Endpoint),
                TransportMode = HttpTransportMode.AutoDetect,
                Name = "ASP.NET Core MCP web client",
                AdditionalHeaders = additionalHeaders
            },
            loggerFactory);

        return await McpClient.CreateAsync(
            transport,
            loggerFactory: loggerFactory,
            cancellationToken: cancellationToken);
    }

    private async Task<Dictionary<string, string>?> GetAuthenticationHeadersAsync(CancellationToken cancellationToken)
    {
        var settings = mcpServerOptions.Value;
        if (string.IsNullOrWhiteSpace(settings.TokenEndpoint))
        {
            return null;
        }

        using var httpClient = new HttpClient();
        using var response = await httpClient.PostAsJsonAsync(
            settings.TokenEndpoint,
            new { settings.Username, settings.Password },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("MCP Server 的令牌接口未返回访问令牌。");
        if (string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("MCP Server 的令牌接口返回了空的访问令牌。");
        }

        var tokenType = string.IsNullOrWhiteSpace(token.TokenType) ? "Bearer" : token.TokenType;
        return new Dictionary<string, string>
        {
            ["Authorization"] = $"{tokenType} {token.AccessToken}"
        };
    }
}

public sealed record McpChatRequest(string Message);

public sealed record McpChatResult(string Answer, IReadOnlyList<string> AvailableTools);

file sealed record TokenResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string AccessToken,
    [property: System.Text.Json.Serialization.JsonPropertyName("token_type")] string? TokenType);