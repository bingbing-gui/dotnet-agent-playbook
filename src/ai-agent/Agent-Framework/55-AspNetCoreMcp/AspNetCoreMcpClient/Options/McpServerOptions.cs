namespace AspNetCoreCallMcpServer.Options;

public sealed class McpServerOptions
{
    public const string SectionName = "McpServer";

    public string Endpoint { get; init; } = string.Empty;

    public string? TokenEndpoint { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }
}