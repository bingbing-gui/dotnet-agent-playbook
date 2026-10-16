// Copyright (c) Microsoft. All rights reserved.

// 这个例子展示了如何使用 TextSearchProvider 为 AI 代理添加检索增强生成 (RAG) 功能。
// 该示例使用了一个内存向量存储，可以很容易地替换为实现 Microsoft.Extensions.VectorData 抽象的任何其他向量存储。
// TextSearchProvider 在每次模型调用之前通过 TextSearchStore 对向量存储运行搜索，并将结果注入模型上下文。
// TextSearchStore 是一个示例存储实现，它硬编码了存储模式，并使用向量存储来存储和检索文档。


using AIContextProvider.TextSearchProvider;
using Azure.AI.Projects;
using Azure.Identity;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";
var embeddingDeploymentName = Environment.GetEnvironmentVariable("FOUNDRY_EMBEDDING_MODEL") ?? "text-embedding-3-large";


AIProjectClient aiProjectClient = new(
    new Uri(endpoint),
    new DefaultAzureCredential());


VectorStore vectorStore = new InMemoryVectorStore(new()
{
    EmbeddingGenerator = aiProjectClient.GetProjectOpenAIClient().GetEmbeddingClient(embeddingDeploymentName).AsIEmbeddingGenerator()
});

TextSearchStore textSearchStore = new(vectorStore, "product-and-policy-info", 3072);


// 上传示例文档到存储中。
await textSearchStore.UpsertDocumentsAsync(GetSampleDocuments());

async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapterAsync(string text, CancellationToken ct)
{
    // 这里我们将搜索结果限制为单个最佳结果，以演示我们是否准确匹配每个问题的特定搜索结果，但在实际情况下，应使用更多结果。
    var searchResults = await textSearchStore.SearchAsync(text, 1, ct);
    return searchResults.Select(r => new TextSearchProvider.TextSearchResult
    {
        SourceName = r.SourceName,
        SourceLink = r.SourceLink,
        Text = r.Text ?? string.Empty,
        RawRepresentation = r
    });
}

// 配置 TextSearchProvider 的选项。
TextSearchProviderOptions textSearchOptions = new()
{
    SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
};

AIAgent agent = aiProjectClient
    .AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new() { ModelId = deploymentName, Instructions = "你是一个有帮助的 Contoso Outdoors 支持专家。使用提供的上下文回答问题，并在可用时引用源文档。" },
        
        AIContextProviders = [new TextSearchProvider(SearchAdapterAsync, textSearchOptions)],
        ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions
        {
            StorageInputRequestMessageFilter = messages => messages.Where(m => m.GetAgentRequestMessageSourceType() != AgentRequestMessageSourceType.AIContextProvider && m.GetAgentRequestMessageSourceType() != AgentRequestMessageSourceType.ChatHistory)
        }),
    });

AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine(">> 咨询关于退货政策\n");
Console.WriteLine(await agent.RunAsync("你好，我需要理解退货政策。", session));

Console.WriteLine("\n>> 咨询关于配送\n");
Console.WriteLine(await agent.RunAsync("标准配送通常需要多长时间？", session));

Console.WriteLine("\n>> 咨询关于产品维护\n");
Console.WriteLine(await agent.RunAsync("TrailRunner 帐篷面料的最佳维护方法是什么？", session));

static IEnumerable<TextSearchDocument> GetSampleDocuments()
{
    yield return new TextSearchDocument
    {
        SourceId = "return-policy-001",
        SourceName = "Contoso Outdoors 退货政策",
        SourceLink = "https://contoso.com/policies/returns",
        Text = "客户可以在商品送达后的 30 天内退回任何商品。商品应保持未使用状态并包含原始包装。商品检查完成后，退款将在 5 个工作日内退回至原付款方式。"
    };

    yield return new TextSearchDocument
    {
        SourceId = "shipping-guide-001",
        SourceName = "Contoso Outdoors 配送指南",
        SourceLink = "https://contoso.com/help/shipping",
        Text = "订单金额超过 50 美元可享受免费标准配送。在美国本土范围内，商品通常会在 3 至 5 个工作日内送达。结账时还可以选择加急配送。"
    };

    yield return new TextSearchDocument
    {
        SourceId = "tent-care-001",
        SourceName = "TrailRunner 帐篷养护说明",
        SourceLink = "https://contoso.com/manuals/trailrunner-tent",
        Text = "请使用温水和非洗涤剂型肥皂清洁帐篷面料。存放前应让帐篷完全自然晾干，并避免长时间暴露在紫外线下，以延长防水涂层的使用寿命。"
    };
}