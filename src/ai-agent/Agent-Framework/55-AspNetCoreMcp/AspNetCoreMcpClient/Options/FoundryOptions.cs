namespace AspNetCoreCallMcpServer.Options;

public sealed class FoundryOptions
{
    public string ProjectEndpoint { get; init; } = string.Empty;

    public string Model { get; init; } = "gpt-5.4-mini";
}