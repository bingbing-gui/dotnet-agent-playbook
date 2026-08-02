
上一节介绍了 `FoundryMemoryProvider`。它由 Microsoft Foundry 托管，能够从对话中自动提取长期记忆，并将其保存到云端的 Memory Store 中。

这一节介绍另一种记忆实现：`FileMemoryProvider`。

`FileMemoryProvider` 是一个 `AIContextProvider`。它会向 Agent 提供一组用于管理记忆文件的工具，由 Agent 自己判断哪些信息值得保存、什么时候读取，以及是否需要修改或删除已有记忆。

记忆会以独立文件的形式保存在 AgentFileStore 中，Agent 可以根据需要创建、读取或更新这些文件。后续创建的新 Session 仍然可以继续读取这些信息。


## FileMemoryProvider 提供的工具

将 `FileMemoryProvider` 添加到 Agent 后，Agent 可以使用以下记忆工具：

| 工具 | 说明 |
| --- | --- |
| `file_memory_write` | 创建或写入一个记忆文件，并指定文件名、内容和可选说明 |
| `file_memory_read` | 根据文件名读取记忆文件的内容 |
| `file_memory_delete` | 根据文件名删除记忆文件 |
| `file_memory_ls` | 列出当前工作目录中的所有记忆文件及其说明 |
| `file_memory_grep` | 使用正则表达式搜索记忆文件内容 |
| `file_memory_replace` | 替换记忆文件中出现的指定字符串 |
| `file_memory_replace_lines` | 替换记忆文件中的整行内容 |

这些工具并不是由应用代码直接调用，而是由Agent根据当前对话自主决定是否调用，Agent 可以调用 file_memory_write，将这项偏好写入一个记忆文件。

当用户在后续会话中询问旅行建议时，Agent 可以调用 file_memory_read，读取此前保存的旅行偏好。

### memories.md 索引文件

FileMemoryProvider 还会维护一个名为 memories.md 的索引文件。FileMemoryProvider 会将该索引注入当前对话，使 Agent 不需要先调用 file_memory_ls，就能知道当前有哪些记忆文件可用。

### AgentFileStore

AgentFileStore 是 FileMemoryProvider 使用的文件存储抽象。

FileMemoryProvider 负责提供记忆工具和管理逻辑，真正的数据保存位置则由 AgentFileStore 的具体实现决定。

我们使用：`FileSystemAgentFileStore`

它会将记忆保存到本地磁盘。

代码如下：

```csharp
var memoryRoot = Path.Combine(
    AppContext.BaseDirectory,
    "agent-memory");

var fileStore = new FileSystemAgentFileStore(memoryRoot);
```

这里将应用程序基目录下的 agent-memory 文件夹设置为记忆存储根目录。


除了 `FileSystemAgentFileStore`，还可以使用：`InMemoryAgentFileStore`将记忆保存在内存中。

不过，内存存储只适合测试或临时演示，应用程序退出后数据就会丢失。

在实际项目中，也可以自定义 AgentFileStore，将记忆保存到 Azure Blob Storage、共享文件系统或其他持久化存储中。

因此，FileMemoryProvider 中的“File”表示记忆以文件形式组织，但底层存储并不一定只能使用本地磁盘。

### FileMemoryState

FileMemoryState 是 FileMemoryProvider 保存到 AgentSession 中的状态。

其中最重要的属性是：`WorkingFolder`，它用于指定当前 Session 应该从哪个目录读取和写入记忆文件。
这个目录是相对于 AgentFileStore 根目录的子目录。

在当前示例中：

```csharp
const string UserId = "UID1";
var workingFolder = $"users/{UserId}";
```

因此：

- Memory Root：`agent-memory`
- WorkingFolder：`users/UID1`

最终的记忆文件保存位置为：
`agent-memory/users/UID1`

源码中也输出了实际目录：
```csharp
Console.WriteLine(
    $"记忆文件被写入到: " +
    $"{Path.Combine(memoryRoot, workingFolder)}");
```

## 创建 FileMemoryProvider

完整的 Provider 创建代码如下：

```csharp
using var fileMemoryProvider = new FileMemoryProvider(
    fileStore,
    session => new FileMemoryState { WorkingFolder = $"users/{userId}" });
```

## 将 FileMemoryProvider 添加到 Agent

创建 Provider 后，将其添加到 Agent 的 AIContextProviders：

```csharp
AIAgent agent = new AIProjectClient(
        new Uri(endpoint),
        new DefaultAzureCredential())
    .AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions = "你是一个乐于助人的旅行助手。记住用户告诉你的关于他们自己的信息，以便以后提供更好的建议。"
        },
        Name = "TravelAssistant",
        AIContextProviders = [fileMemoryProvider],
    });
```

## 第一次对话写入记忆

首先创建第一次会话，然后向 Agent 提供用户资料和旅行计划：

```csharp
AgentSession firstSession = await agent.CreateSessionAsync();
Console.WriteLine("=== 第一次对话 ===");
Console.WriteLine(await agent.RunAsync(
    "你好，我的名字是桂兵兵，我计划去北海道旅游，我和朋友去旅游，帮我找风景优美的景点。",
    firstSession));
Console.WriteLine();
```

在这一轮对话结束之后，Agent 可以调用 file_memory_write，将这些信息写入一个或多个记忆文件。

## 查看磁盘上的记忆文件

第一次对话结束后，示例通过下面的代码列出磁盘上生成的文件：

```csharp
Console.WriteLine("=== 磁盘文件===");
foreach (var file in Directory.EnumerateFiles(Path.Combine(memoryRoot, workingFolder)))
{
    Console.WriteLine(Path.GetFileName(file));
}

Console.WriteLine();
```

## 新 Session 读取以前的记忆

接下来创建一个全新的 Session：

```csharp

AgentSession secondSession = await agent.CreateSessionAsync();
Console.WriteLine("=== 第二次对话 (新会话) ===");
Console.WriteLine(await agent.RunAsync("你能回顾一下你记得的个人信息吗？", secondSession));
```

## 运行效果


## 工作原理

1. 创建一个 FileSystemAgentFileStore，其根目录为本地 agent-memory 文件夹。
2. 在该存储之上创建一个 FileMemoryProvider，并使用状态初始化器将当前用户的记忆放入其各自的工作文件夹。
3. 通过 ChatClientAgentOptions.AIContextProviders 将该提供程序附加到代理，这会为Agent提供 file_memory_* 工具以及使用它们的说明。
4. 在第一段对话中，用户分享了一些偏好，代理调用 file_memory_write 将其作为文件存储到工作文件夹中。然后，此示例会列出在磁盘上创建的文件。
5. 在第二段对话中，会创建一个全新的会话，不包含第一段对话的聊天历史。提供程序会将记忆索引注入对话中，代理调用 file_memory_read 来回忆已存储的偏好，并在提出建议时使用这些偏好。

