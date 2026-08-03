
上一节介绍了 `FileMemoryProvider`。它以文件的形式存储记忆。

这一节中我们介绍`ValkeyChatHistoryProvider`，它可以把 Agent 的聊天记录保存到 Valkey 中。

## 什么是 Valkey？

`Valkey` 是一款高性能数据结构服务器，主要用于处理键值（key/value）工作负载。它支持多种原生数据结构，并提供可扩展的插件系统，以便添加新的数据结构和访问模式。

Github地址：`https://github.com/valkey-io/valkey`

## 安装准备 Valkey

为了方面，我们安装docker的Valkey 容器：

```bash
docker run -d \
  --name valkey \
  -p 6379:6379 \
  valkey/valkey:latest
```

## 数据库链接

Valkey默认监听在 6379 端口，连接字符串如下：

```text
localhost:6379
```

在程序中需要安装如下包 `Microsoft.Agents.AI.Valkey`, 这个包引用了`Valkey.Glide`包，里面包含了链接 Valkey数据库的接口：

```csharp
var connection = await ConnectionMultiplexer.ConnectAsync(valkeyConnection);
```

## 创建 ValkeyChatHistoryProvider

接下来创建 ValkeyChatHistoryProvider：

```csharp
var valkeyConnection = Environment.GetEnvironmentVariable("VALKEY_CONNECTION") ?? "localhost:6379";
var connection = await ConnectionMultiplexer.ConnectAsync(valkeyConnection);
var historyProvider = new ValkeyChatHistoryProvider(
    connection,
    _ => new ValkeyChatHistoryProvider.State(
        $"sample-{Guid.NewGuid():N}"),
    new ValkeyChatHistoryProviderOptions
    {
        KeyPrefix = "sample_chat",
        MaxMessages = 20
    });
```

这里有两个比较重要的配置。

KeyPrefix 用于设置 Valkey Key 的前缀：

```text
sample_chat
```
最终生成的 Key 类似：

```text
sample_chat:sample-332f6a198ca84300adfce6ea8f2cce53
```

MaxMessages 表示最多保留多少条聊天消息：
```csharp
MaxMessages = 20
```

当前示例使用随机 Guid 作为会话标识，因此每次创建新的 AgentSession，都会生成新的聊天历史 Key。

在实际业务中，通常可以把用户 ID 和业务会话 ID 组合起来：

```csharp
new ValkeyChatHistoryProvider.State(
    $"user-{userId}:conversation-{conversationId}")
```
这样既可以区分不同用户，也可以区分同一用户的多个聊天窗口。

## 创建 Agent

本示例通过 Microsoft Foundry 获取 Responses Client：

```csharp
AIAgent historyAgent = new AIProjectClient(
        new Uri(endpoint),
        new DefaultAzureCredential())
    .GetProjectOpenAIClient()
    .GetProjectResponsesClient()
    .AsIChatClientWithStoredOutputDisabled(deploymentName)
    .AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions =
                "你是一个乐于助人的助手，会记住我们的对话。"
        },
        ChatHistoryProvider = historyProvider
    });
```

这里需要注意：

```csharp
AsIChatClientWithStoredOutputDisabled(deploymentName)
```

关闭了服务端 Stored Output。
这是因为当前示例已经使用 ValkeyChatHistoryProvider 管理聊天历史。如果同时启用 Foundry 服务端会话历史，会出现两套历史管理机制冲突的问题。

## 运行多轮对话

创建一个 Session：

```csharp
AgentSession session1 =
    await historyAgent.CreateSessionAsync();
```

然后连续进行三轮对话：

```csharp
Console.WriteLine(await historyAgent.RunAsync(
    "你好，我叫桂兵兵，我是一名软件工程师。",
    session1));

Console.WriteLine(await historyAgent.RunAsync(
    "我正在使用Valkey进行缓存的项目。",
    session1));

Console.WriteLine(await historyAgent.RunAsync(
    "你记得我什么吗？",
    session1));
```

每次调用 RunAsync() 后，用户消息和 Agent 回复都会被写入 Valkey。

三轮对话通常会产生六条消息：

- 3 条用户消息
- 3 条 Agent 回复

最后可以通过下面的方法查看当前 Session 已保存的消息数量：

```csharp
var messageCount =
    await historyProvider.GetMessageCountAsync(session1);

Console.WriteLine(
    $"\n已在 Valkey 中存储 {messageCount} 条消息。\n");
```

## 查看 Valkey 中的数据

常用的 Valkey 命令行工具，可以通过 docker 进入 Valkey 容器后使用。

进入 Valkey 命令行：
```bash
docker exec -it valkey valkey-cli
```

查看当前数据库中的 Key 数量：
```bash
DBSIZE
```

查看当前数据库中的所有 Key：
```bash
KEYS *
```


为了方面查看数据，我写了一个脚本`show-valkey-history.ps1`来查看测试数据：

```bash
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()

$keys = docker exec valkey valkey-cli --raw --scan --pattern "sample_chat:*"

foreach ($key in $keys) {
    Write-Host "`n=== $key ==="

    docker exec valkey valkey-cli --raw LRANGE $key 0 -1 |
    ForEach-Object {
        $message = $_ | ConvertFrom-Json

        [PSCustomObject]@{
            Role = $message.role
            Text = ($message.contents | ForEach-Object { $_.text }) -join "`n"
        }
    } |
    Format-Table -Wrap
}
```

如下图所示：





## 总结

`ValkeyChatHistoryProvider` 的核心作用，是把原本只存在于当前进程中的聊天记录保存到 Valkey 中。

在每次调用 `RunAsync()` 后，用户消息和 Agent 回复都会追加到 Valkey List。下一次继续使用同一个会话标识时，Provider 会重新读取这些历史消息，并将它们补充到模型上下文中。

如果应用需要保存多轮聊天上下文，并希望在应用重启后继续恢复会话，`ValkeyChatHistoryProvider` 是一种比较直接的实现方式。
