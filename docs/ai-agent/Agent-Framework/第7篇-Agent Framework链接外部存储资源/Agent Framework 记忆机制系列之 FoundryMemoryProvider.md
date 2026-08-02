上一节介绍了 `ChatHistoryMemoryProvider`。它会保存聊天消息，并在后续对话中通过向量检索找回相关历史，为 Agent 补充对话上下文。

这一节继续介绍 Microsoft Foundry 提供的托管 Memory 服务。

与直接保存聊天记录不同，Foundry Memory 会分析对话内容，从中提取具有长期价值的信息，例如用户资料、个人偏好、旅行计划以及历史对话摘要，并将这些信息保存到云端的 Memory Store 中。

`FoundryMemoryProvider` 负责将这项托管服务接入 Agent Framework。即使应用创建了新的 `AgentSession`，只要使用相同的 Memory Scope，Agent 仍然可以检索属于同一用户的长期记忆。

## 查看 Foundry Memory

首先打开 Microsoft Foundry，并进入一个已经创建的 Project。如果当前还没有 Project，需要先创建一个。

在项目左侧导航栏中找到 **Memory**，进入后可以查看当前项目下的 Memory Store。

图片-1

在 Memory 页面中，可以查看：

- 当前项目下已经创建的 Memory Store；
- Memory Store 使用的聊天模型；
- Memory Store 使用的 Embedding 模型；
- 已经提取出的长期记忆；
- Memory 更新任务的状态。

图片-2

Foundry Memory 的处理过程发生在云端。Agent 提交对话后，Foundry 会调用聊天模型分析内容，再调用 Embedding 模型生成向量，最后将提取出的记忆保存到 Memory Store 中。

## 配置 Azure 权限

使用 Foundry Memory 前，需要为当前 Foundry Project 的托管身份配置权限。

进入 Azure Portal，找到对应的 Foundry Project，然后进入：

```text
Foundry Project
→ Access control (IAM)
→ Add role assignment
```

添加以下角色：

```text
Role：Foundry User
Member：当前 Project 的 Managed Identity
```

这里选择的是项目的 `Managed Identity`，而不是开发者自己的 User 账号。

这是因为本地程序只负责提交 Memory 更新请求，真正的记忆提取、Embedding 生成和数据写入由 Foundry 在云端异步完成。后台任务需要使用项目托管身份访问相关模型和资源。

> [!WARNING]
> 如果权限没有正确配置，Memory 更新可能失败，并出现 `401 Authentication` 等错误。
>
> 在 Foundry 的 Memory 页面中，如果系统检测到权限缺失，通常也会显示 `Resolve`，可以按照页面提示自动完成部分权限配置。

权限分配完成后，可能需要等待一段时间才会生效。如果刚刚添加角色后仍然出现认证错误，可以稍等几分钟再重新测试。

## 创建 FoundryMemoryProvider

首先创建 `FoundryMemoryProvider`：

```csharp
FoundryMemoryProvider memoryProvider = new(
    projectClient,
    memoryStoreName,
    stateInitializer: _ => new FoundryMemoryProvider.State(
        new FoundryMemoryProviderScope("sample-user")));
```

构造函数中的 `memoryStoreName` 表示要使用的 Memory Store 名称。

`FoundryMemoryProviderScope` 用于划分记忆空间。这里的 `"sample-user"` 可以理解为用户标识。

使用相同 Scope 的不同 Session 可以访问同一组记忆，而不同 Scope 之间的记忆彼此隔离。

例如：

```text
sample-user-001
sample-user-002
```

这两个 Scope 会分别维护自己的长期记忆。

在实际项目中，Scope 应使用稳定且不会轻易变化的标识，例如：

```text
用户 ID
租户 ID + 用户 ID
组织 ID + 用户 ID
```

不建议直接使用用户名、昵称或者邮箱，因为这些字段可能发生变化，也可能包含敏感信息。

随后，将 `FoundryMemoryProvider` 添加到 Agent 的 `AIContextProviders`：

```csharp
ChatClientAgent agent = projectClient.AsAIAgent(
    new ChatClientAgentOptions
    {
        Name = "TravelAssistantWithFoundryMemory",
        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions =
                "你是一个友好的旅行助手。" +
                "在回答时使用已知的用户记忆，不要编造细节。"
        },
        AIContextProviders = [memoryProvider]
    });
```

配置完成后，Agent Framework 会在两个阶段调用该 Provider：

```text
调用模型之前
→ 从 Foundry Memory 中检索与当前问题相关的记忆

Agent 返回结果之后
→ 将本轮用户消息和 Agent 响应提交给 Foundry Memory
```

因此，Memory 的读取和更新都由 `FoundryMemoryProvider` 自动完成。

## 确保 Memory Store 已创建

使用 Foundry Memory 前，需要确保指定名称的 Memory Store 已经存在。

```csharp
AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine("\n>> 设置 Foundry Memory Store\n");

await memoryProvider.EnsureMemoryStoreCreatedAsync(
    deploymentName,
    embeddingModelName,
    "面向旅行助手的 Memory Store");
```

创建 Memory Store 时需要指定两个模型。

`deploymentName` 是聊天模型部署名称，用于分析对话内容，从中提取、归纳和更新长期记忆。

`embeddingModelName` 是 Embedding 模型部署名称，用于将记忆转换为向量，以便后续根据语义检索相关内容。

例如：

```csharp
var deploymentName = "gpt-4o";
var embeddingModelName = "text-embedding-3-large";
```

这里填写的是 Foundry 中实际创建的 **Deployment Name**，不一定等于模型目录中显示的 Model Name。

`EnsureMemoryStoreCreatedAsync` 会先检查指定名称的 Memory Store 是否存在。如果不存在，就使用传入的聊天模型和 Embedding 模型创建。

如果同名 Memory Store 已经存在，该方法不会重新创建，也不会自动修改原有的模型配置。

例如，下面这个 Store 已经使用 `gpt-4o` 创建：

```text
memory-store-0001
```

之后即使把代码中的模型改成其他部署，再次调用：

```csharp
await memoryProvider.EnsureMemoryStoreCreatedAsync(...);
```

也不代表已有 Store 会切换到新的模型。

如果需要测试不同模型配置，建议使用新的 Store 名称：

```csharp
var memoryStoreName =
    $"memory-store-{DateTime.UtcNow:yyyyMMddHHmmss}";
```

## 写入用户信息

接下来，通过正常的 Agent 对话提交用户信息：

```csharp
Console.WriteLine(await agent.RunAsync(
    "你好，我的名字是桂兵兵，我计划去北海道旅游。",
    session));

Console.WriteLine(await agent.RunAsync(
    "我和朋友一起去，希望找一些风景优美的景点。",
    session));
```

每次调用 `RunAsync()` 后，`FoundryMemoryProvider` 都会自动将本轮用户消息和 Agent 响应提交给 Foundry Memory。

不需要手动调用其他方法才能触发保存。

连续调用多次 `RunAsync()` 后，可以只调用一次：

```csharp
await memoryProvider.WhenUpdatesCompletedAsync();
```

例如：

```csharp
await agent.RunAsync("我的名字是桂兵兵。", session);
await agent.RunAsync("我计划去北海道旅游。", session);
await agent.RunAsync("我比较喜欢自然风景。", session);

await memoryProvider.WhenUpdatesCompletedAsync();
```

前面的三次 `RunAsync()` 都会提交 Memory 更新，而 `WhenUpdatesCompletedAsync()` 负责等待这些异步更新处理完成。

需要注意，Foundry Memory 不一定把三轮对话原样保存为三条记录。它会分析内容，并提取成更适合长期保存的信息，例如：

```text
用户的名字是桂兵兵。
用户计划和朋友去北海道旅行。
用户偏好自然风景类景点。
```

相关信息也可能被合并成一条更加完整的记忆。

## 等待 Foundry Memory 异步更新

Foundry Memory 的更新过程是异步的。

Agent 完成一轮对话后，消息会先提交到服务端，随后经历以下处理：

```text
提交对话
→ 排队等待
→ 提取长期记忆
→ 合并或更新已有记忆
→ 生成 Embedding
→ 写入 Memory Store
```

因此，`RunAsync()` 返回时，不代表长期记忆已经可以立即检索。

测试代码可以使用：

```csharp
await memoryProvider.WhenUpdatesCompletedAsync();
```

等待当前 Provider 提交的更新完成。

这个方法会轮询 Memory Update 状态。日志中可能看到：

```text
Queued
InProgress
Completed
```

其中：

- `Queued` 表示更新已经提交，但后台任务尚未开始执行；
- `InProgress` 表示服务正在提取和处理记忆；
- `Completed` 表示本次更新已经完成；
- `Failed` 表示处理失败。

与固定等待几秒相比，等待任务状态更加可靠。

```csharp
// 不推荐仅依赖固定等待时间
await Task.Delay(TimeSpan.FromSeconds(10));
```

固定等待时间无法保证服务已经处理完成。等待时间过短时，新记忆可能还没有生成；等待时间过长又会增加不必要的延迟。

不过，在生产环境中也不应该无限等待。建议增加超时和异常处理：

```csharp
using CancellationTokenSource cancellationTokenSource =
    new(TimeSpan.FromMinutes(3));

try
{
    await memoryProvider.WhenUpdatesCompletedAsync(
        pollingInterval: TimeSpan.FromSeconds(5),
        cancellationToken: cancellationTokenSource.Token);

    Console.WriteLine("Memory 更新完成。");
}
catch (OperationCanceledException)
{
    Console.WriteLine("等待 Memory 更新超时。");
}
catch (Exception ex)
{
    Console.WriteLine($"Memory 更新失败：{ex.Message}");
}
```

Foundry Memory 具有最终一致性。用户刚刚提供的信息，可能需要等待一段时间后，才能作为长期记忆被后续请求检索到。

如果更新长时间停留在 `Queued`，应记录日志中的 Memory Update ID。这个 ID 可以帮助排查服务端队列、模型部署、配额和权限问题。

## 开启日志

为了观察 Memory 更新状态，可以给 `FoundryMemoryProvider` 配置日志：

```csharp
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
});
```

创建 Provider 时传入 `loggerFactory`：

```csharp
FoundryMemoryProvider memoryProvider = new(
    projectClient,
    memoryStoreName,
    stateInitializer: _ => new FoundryMemoryProvider.State(
        new FoundryMemoryProviderScope("sample-user")),
    options: new FoundryMemoryProviderOptions
    {
        EnableSensitiveTelemetryData = true
    },
    loggerFactory: loggerFactory);
```

运行后可以看到类似日志：

```text
15:13:18 FoundryMemoryProvider: Update status: Queued
15:13:26 FoundryMemoryProvider: Update status: InProgress
15:13:35 FoundryMemoryProvider: Update status: Completed
```

`EnableSensitiveTelemetryData` 适合本地调试，它可能会让日志中包含 Scope、用户内容或检索结果等信息。生产环境使用时需要谨慎，避免将敏感数据写入日志。

## 验证长期记忆

等待更新完成后，可以直接询问 Agent：

```csharp
Console.WriteLine(await agent.RunAsync(
    "你已经知道我即将进行的旅行的哪些信息？",
    session));
```

这次请求执行前，`FoundryMemoryProvider` 会根据问题检索相关记忆，并将结果补充到当前模型上下文中。

Agent 可能回答：

```text
你叫桂兵兵，计划和朋友一起去北海道旅游，
并且希望寻找一些风景优美的景点。
```

这说明前面的用户信息已经被 Foundry Memory 提取并保存。

## 序列化和恢复 Session

Agent Framework 还可以序列化当前 Session：

```csharp
JsonElement serializedSession =
    await agent.SerializeSessionAsync(session);

AgentSession restoredSession =
    await agent.DeserializeSessionAsync(serializedSession);
```

恢复后，可以继续使用该 Session：

```csharp
Console.WriteLine(await agent.RunAsync(
    "你能回顾一下你记得的个人信息吗？",
    restoredSession));
```

这里需要区分两种状态。

`AgentSession` 保存的是当前 Agent 会话状态，序列化后可以在应用重启或其他进程中恢复。

`FoundryMemoryProviderScope` 决定长期记忆在云端属于哪个用户或业务范围。

即使不恢复原来的 Session，只要新 Session 使用相同 Scope，也可以访问同一组 Foundry Memory。

## 在新 Session 中读取记忆

下面重新创建一个全新的 Session：

```csharp
AgentSession newSession =
    await agent.CreateSessionAsync();

Console.WriteLine(await agent.RunAsync(
    "总结一下你已经知道的关于我的信息。",
    newSession));
```

虽然 `newSession` 与之前的 Session 不同，但 Provider 仍然使用：

```csharp
new FoundryMemoryProviderScope("sample-user")
```

因此，新 Session 可以检索此前属于 `sample-user` 的长期记忆。

可以简单理解为：

```text
AgentSession
→ 管理一次具体会话的状态

FoundryMemoryProviderScope
→ 决定长期记忆属于哪个用户
```

只要 Scope 相同，多个 Session 就可以共享同一组长期记忆。

## 清理记忆与隔离测试数据

为了确保示例每次运行都从干净的数据开始，可以删除当前 Scope 下已经保存的记忆：

```csharp
await memoryProvider.EnsureStoredMemoriesDeletedAsync(session);
```

这里传入 `session`，是因为 `FoundryMemoryProvider` 会从 Session 中读取对应的 Provider 状态，并确定当前使用的 Memory Scope。

它删除的是该 Scope 在云端 Memory Store 中保存的长期记忆，而不只是当前 Session 中的聊天记录。

`EnsureStoredMemoriesDeletedAsync` 适合演示和自动化测试，可以避免以前的数据影响本次运行结果。

生产环境不能在普通对话流程中随意调用，否则可能误删用户已经积累的长期记忆。

实际系统中，记忆删除通常应作为独立的数据管理能力，用于以下场景：

- 用户主动要求清除记忆；
- 自动化测试结束后清理测试数据；
- 用户注销账号；
- 用户退出租户或组织；
- 按数据保留策略删除过期内容；
- 满足隐私保护和合规要求。

## 在 Foundry 中查看结果

程序运行完成后，重新打开 Microsoft Foundry，并进入当前 Project 的 Memory 页面。

可以看到代码中创建的 Memory Store：

```text
memory-store-0001
```

点击进入详情页，可以查看该 Store 使用的模型配置。

在这个示例中，聊天模型为：

```text
gpt-4o
```

Embedding 模型为：

```text
text-embedding-3-large
```

聊天模型负责分析对话并提取长期记忆，Embedding 模型负责将记忆转换成向量，以支持后续的语义检索。

图片-3

继续进入 Memory Store 的记忆列表，还可以查看 Foundry 从对话中提取出的信息。

例如，两轮对话可能被整理为：

```text
用户的名字是桂兵兵。
用户计划和朋友前往北海道旅行。
用户偏好风景优美的自然景点。
```

图片-4

这里保存的不是完整聊天记录，而是 Foundry 从对话中归纳出的长期信息。

这也是 `FoundryMemoryProvider` 与 `ChatHistoryMemoryProvider` 最明显的区别。

`ChatHistoryMemoryProvider` 更偏向保存并检索原始历史消息。

`FoundryMemoryProvider` 则由 Foundry 托管服务负责提取、合并和维护长期记忆。

通过 Memory Store、Memory Scope 和异步更新机制，长期记忆不再依赖某一个 `AgentSession`。即使应用创建新的 Session，Agent 仍然可以继续了解同一用户的资料、偏好和历史信息。

