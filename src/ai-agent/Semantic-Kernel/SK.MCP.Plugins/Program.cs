using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using SK.MCP.Plugins.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();



builder.Services.AddSingleton((serviceProvider) =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    kernelBuilder.Services
            .AddAzureOpenAIChatCompletion(
            deploymentName: "gpt-5-mini",
            apiKey: "",
            endpoint: "https://rg-mcp.cognitiveservices.azure.com/");

    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var github_mcp_key = "";

    if (!string.IsNullOrWhiteSpace(github_mcp_key))
    {
        var mcpClient = McpClientFactory.CreateAsync(new SseClientTransport(new()
        {
            Name = "GitHub",
            Endpoint = new Uri("https://api.githubcopilot.com/mcp/"),
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {github_mcp_key}"
            }
        })).GetAwaiter().GetResult();

        var tools = mcpClient.ListToolsAsync().GetAwaiter().GetResult();
        kernelBuilder.Plugins.AddFromFunctions("GithubMCP", tools.Select(skf => skf.AsKernelFunction()));
    }

    return kernelBuilder.Build();
});

builder.Services.AddSingleton<IMcpToolService, McpToolService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
