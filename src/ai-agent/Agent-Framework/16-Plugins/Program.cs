
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

ServiceCollection services = new();
services.AddSingleton<WeatherProvider>();
services.AddSingleton<CurrentTimeProvider>();
services.AddSingleton<AgentPlugin>();

IServiceProvider serviceProvider = services.BuildServiceProvider();

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: "你是一个乐于助人的助手，帮助人们查找信息。",
        name: "Assistant",
        tools: [.. serviceProvider.GetRequiredService<AgentPlugin>().AsAITools()],
        services: serviceProvider);

Console.WriteLine(await agent.RunAsync("告诉我西雅图的当前时间和天气。"));


internal sealed class AgentPlugin(WeatherProvider weatherProvider)
{
    public string GetWeather(string location)
    {
        return weatherProvider.GetWeather(location);
    }

    public DateTimeOffset GetCurrentTime(IServiceProvider sp, string location)
    {
        var currentTimeProvider = sp.GetRequiredService<CurrentTimeProvider>();

        return currentTimeProvider.GetCurrentTime(location);
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(this.GetWeather);
        yield return AIFunctionFactory.Create(this.GetCurrentTime);
    }
}

internal sealed class WeatherProvider
{
    public string GetWeather(string location)
    {
        return $"{location}天气是多云高于15°C.";
    }
}

internal sealed class CurrentTimeProvider
{
    public DateTimeOffset GetCurrentTime(string location)
    {
        return DateTimeOffset.Now;
    }
}