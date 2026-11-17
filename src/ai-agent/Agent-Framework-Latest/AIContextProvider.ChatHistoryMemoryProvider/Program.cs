using Azure.AI.Projects;
using Azure.Identity;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using System.Text;

var endpoint = "https://maf.services.ai.azure.com"; //Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var deploymentName = "gpt-5.2";//Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";
var embeddingDeploymentName = "text-embedding-3-large"; //Environment.GetEnvironmentVariable("FOUNDRY_EMBEDDING_MODEL") ?? "text-embedding-3-large";



Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Trace)
        .AddSimpleConsole(options =>
        {
            options.SingleLine = true;
        });
});

AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());
// 创建一个向量存储来存储聊天消息。
// 为了演示目的，我们使用内存中的向量存储。
// 将其替换为您选择的可以长期保存聊天历史记录的向量存储实现。

VectorStore vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions()
{
    EmbeddingGenerator = aiProjectClient
        .GetProjectOpenAIClient()
        .GetEmbeddingClient(embeddingDeploymentName)
        .AsIEmbeddingGenerator()
});
var memoryOptions = new ChatHistoryMemoryProviderOptions
{
    MaxResults = 5,

    // 仅用于本地调试。
    // 开启后，日志中会显示用户 ID、搜索文本和检索结果。
    EnableSensitiveTelemetryData = true,

    ContextPrompt =
        "下面是从该用户过去的对话中检索到的信息。" +
        "回答当前问题时，请优先使用其中明确表达的用户偏好："
};

// 创建代理并添加 ChatHistoryMemoryProvider 以将聊天消息存储在向量存储中。
AIAgent agent = aiProjectClient
    .AsAIAgent(new ChatClientAgentOptions
    {

        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions = "你是一个擅长讲笑话的助手。"
        },
        Name = "Joker",

        AIContextProviders = [new ChatHistoryMemoryProvider(
            vectorStore,
            collectionName: "chathistory",
            vectorDimensions: 3072,
            // 回调以配置 ChatHistoryMemoryProvider 的初始状态。
            // ChatHistoryMemoryProvider 将其状态存储在 AgentSession 中，并且每当ChatHistoryMemoryProvider
            // 无法在会话中找到现有状态时，将调用此回调，
            // 通常是在与新会话首次使用时。
            session => new ChatHistoryMemoryProvider.State(
                // 配置聊天消息将被存储的范围值。
                // 在这种情况下，我们使用固定的用户ID和每个新会话的唯一会话ID。
                storageScope: new() { UserId = "UID1", SessionId = Guid.NewGuid().ToString() },
                // 配置将用于搜索相关先前消息的范围。
                // 在这种情况下，我们正在搜索该用户在所有会话中的任何消息。
                searchScope: new() { UserId = "UID1" }),
               options: memoryOptions,
               loggerFactory: loggerFactory
            )]
    });

// 为代理对话启动一个新会话。
AgentSession session = await agent.CreateSessionAsync();

// 使用将对话历史记录存储在向量存储中的会话运行代理。
Console.WriteLine(await agent.RunAsync("我喜欢程序员的笑话，给我讲一个。", session));

Console.WriteLine(string.Concat(Enumerable.Repeat("-", 80)));

// 启动第二个会话。由于我们将搜索范围配置为跨该用户的所有会话，
// 代理应该记得用户喜欢海盗笑话。
AgentSession? session2 = await agent.CreateSessionAsync();

// 使用第二个会话运行代理。
Console.WriteLine(await agent.RunAsync("给我讲一个我喜欢的笑话。", session2));

Console.ReadLine();