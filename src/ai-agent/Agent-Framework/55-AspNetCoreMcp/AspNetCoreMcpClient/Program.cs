using AspNetCoreCallMcpServer.Options;
using AspNetCoreCallMcpServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddOptions<McpServerOptions>()
    .Bind(builder.Configuration.GetSection(McpServerOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var endpoint)
            && endpoint.Scheme is "http" or "https",
        "McpServer:Endpoint must be an absolute HTTP or HTTPS URL.")
    .ValidateOnStart();

builder.Services.AddSingleton(new FoundryOptions
{
    ProjectEndpoint = builder.Configuration["FOUNDRY_PROJECT_ENDPOINT"] ?? string.Empty,
    Model = builder.Configuration["FOUNDRY_MODEL"] ?? "gpt-5.4-mini"
});

builder.Services.AddScoped<McpChatService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/mcp/weather")
        || context.Request.Path.StartsWithSegments("/api/mcp/tools"))
    {
        await Results.Problem(
            title: "当前仅支持聊天咨询模式",
            detail: "请使用 /api/mcp/chat 接口。",
            statusCode: StatusCodes.Status404NotFound)
            .ExecuteAsync(context);
        return;
    }

    await next();
});

var mcpApi = app.MapGroup("/api/mcp");

mcpApi.MapGet("/configuration", (
    Microsoft.Extensions.Options.IOptions<McpServerOptions> options,
    FoundryOptions foundryOptions) =>
    Results.Ok(new
    {
        endpoint = options.Value.Endpoint,
        model = foundryOptions.Model,
        foundryConfigured = Uri.TryCreate(foundryOptions.ProjectEndpoint, UriKind.Absolute, out _)
    }));

mcpApi.MapPost("/chat", async (
    McpChatRequest request,
    McpChatService chatService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Message)] = ["请输入自然语言问题。"]
        });
    }

    try
    {
        return Results.Ok(await chatService.AskAsync(request.Message, cancellationToken));
    }
    catch (Exception exception)
    {
        return Results.Problem(
            title: "自然语言 MCP 调用失败",
            detail: exception.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }
});

app.Run();
