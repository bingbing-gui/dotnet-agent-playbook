// Copyright (c) Microsoft. All rights reserved.

#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances

// This sample shows how to create and use a simple AI agent with a conversation that can be persisted to disk.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI;
using System.Text;
using System.Text.Json;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";


//创建一个向量存储来存储聊天消息。替换为您选择的向量存储实现，如果您想将聊天历史记录保存到磁盘。
VectorStore vectorStore = new InMemoryVectorStore();

// 创建一个Agent
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(new ChatClientAgentOptions
    {
        Instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。",
        Name = "Joker",
        ChatMessageStoreFactory = ctx =>
        {
            //创建一个新的聊天消息存储器，该存储器将消息存储在向量存储中。每个线程都必须获得VectorChatMessageStore的自己的副本，因为存储器还包含线程存储下的ID。
            return new VectorChatMessageStore(vectorStore, ctx.SerializedState, ctx.JsonSerializerOptions);
        }
    });

// 启动一个新的代理线程来处理对话。
AgentThread thread = agent.GetNewThread();

// 运行代理，传入线程以存储对话历史记录在向量存储中。
Console.WriteLine(await agent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", thread));


//序列化线程状态，以便稍后使用。
JsonElement serializedThread = thread.Serialize();

Console.WriteLine("\n--- Serialized thread ---\n");
Console.WriteLine(JsonSerializer.Serialize(serializedThread, new JsonSerializerOptions { WriteIndented = true }));

//反序列化线程状态以恢复对话。
AgentThread resumedThread = agent.DeserializeThread(serializedThread);

// 继续与代理对话，传入恢复的线程以访问以前的对话历史记录。
Console.WriteLine(await agent.RunAsync("现在把这个段子加上一些表情符号，并用说书人的语气再讲一遍。", resumedThread));


// 我们能够通过线程的GetService方法访问VectorChatMessageStore，如果我们需要读取存储线程的键。
var messageStore = resumedThread.GetService<VectorChatMessageStore>()!;


Console.WriteLine($"\n线程唯一ID存储在向量数据库中: {messageStore.ThreadDbKey}");

Console.WriteLine("\n--- 完成 ---\n");

internal sealed class VectorChatMessageStore : ChatMessageStore
{
    private readonly VectorStore _vectorStore;

    public VectorChatMessageStore(VectorStore vectorStore, JsonElement serializedStoreState, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        this._vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));

        if (serializedStoreState.ValueKind is JsonValueKind.String)
        {
            // Here we can deserialize the thread id so that we can access the same messages as before the suspension.
            this.ThreadDbKey = serializedStoreState.Deserialize<string>();
        }
    }

    public string? ThreadDbKey { get; private set; }

    public override async Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        this.ThreadDbKey ??= Guid.NewGuid().ToString("N");

        var collection = this._vectorStore.GetCollection<string, ChatHistoryItem>("ChatHistory");
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        await collection.UpsertAsync(messages.Select(x => new ChatHistoryItem()
        {
            Key = this.ThreadDbKey + x.MessageId,
            Timestamp = DateTimeOffset.UtcNow,
            ThreadId = this.ThreadDbKey,
            SerializedMessage = JsonSerializer.Serialize(x),
            MessageText = x.Text
        }), cancellationToken);
    }

    public override async Task<IEnumerable<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = default)
    {
        var collection = this._vectorStore.GetCollection<string, ChatHistoryItem>("ChatHistory");
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var records = await collection
            .GetAsync(
                x => x.ThreadId == this.ThreadDbKey, 10,
                new() { OrderBy = x => x.Descending(y => y.Timestamp) },
                cancellationToken)
            .ToListAsync(cancellationToken);

        var messages = records.ConvertAll(x => JsonSerializer.Deserialize<ChatMessage>(x.SerializedMessage!)!)
;
        messages.Reverse();
        return messages;
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null) =>
        // We have to serialize the thread id, so that on deserialization we can retrieve the messages using the same thread id.
        JsonSerializer.SerializeToElement(this.ThreadDbKey);

    /// <summary>
    /// The data structure used to store chat history items in the vector store.
    /// </summary>
    private sealed class ChatHistoryItem
    {
        [VectorStoreKey]
        public string? Key { get; set; }

        [VectorStoreData]
        public string? ThreadId { get; set; }

        [VectorStoreData]
        public DateTimeOffset? Timestamp { get; set; }

        [VectorStoreData]
        public string? SerializedMessage { get; set; }

        [VectorStoreData]
        public string? MessageText { get; set; }
    }
}