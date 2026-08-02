using _03_Mem0MemoryProvider;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Net.Http.Headers;



var endpoint = "";//Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var deploymentName = "";//Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";


AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());

var _mem0MemoryProvider = new Mem0MemoryProvider(
        apiKey: Environment.GetEnvironmentVariable("MEM0_API_KEY")!,
        sessionState =>
        {
            var executionContext = (sessionState?.GetSessionExecutionContext()) ?? throw new InvalidOperationException("Execution context is not initialized");

            return new Mem0ProviderState(
                storageScope: new Mem0ProviderScope
                {
                    RunId = executionContext.RunId,
                    UserId = executionContext.UserId,
                    AppId = executionContext.ApplicationId,
                    AgentId = executionContext.AgentId
                },
                searchScope: new Mem0ProviderScope
                {
                    UserId = executionContext.UserId,
                    AppId = executionContext.ApplicationId
                });
        });


AIAgent agent = aiProjectClient
    .AsAIAgent(new ChatClientAgentOptions()
    {
        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions = "You are a friendly travel assistant. Use known memories about the user when responding, and do not invent details."
        },
        AIContextProviders = [_mem0MemoryProvider]
    });
