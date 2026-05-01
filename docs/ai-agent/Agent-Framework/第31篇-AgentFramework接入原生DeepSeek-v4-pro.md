
我们前面的文章主要基于 Azure OpenAI Service 以及 Azure AI Foundry 提供的 Provider 来构建 Agent。

不过，在真实项目中，我们不一定只使用 Azure OpenAI。有时候也希望直接接入模型厂商自己的 API，例如 DeepSeek。

Agent Framework 提供了 `OpenAIClient` 和 `AnthropicClient`，用于分别接入 OpenAI 和 Anthropic 的模型服务。而 DeepSeek API 刚好同时提供 OpenAI Chat Completions 兼容格式和 Anthropic Messages 兼容格式，因此我们可以借助这两条路径，把 `deepseek-v4-pro` 接入到 Agent Framework 中。


为了演示 AgentFramework 接入原生 DeepSeek-v4-pro，笔者专门购买了 10 元的 DeepSeek-v4-pro 体验版来测试，所以大家多多支持，三连击（点赞、收藏、关注）哦。

图片1

图片2


DeepSeek 官方文档也说明，OpenAI base URL 为 `https://api.deepseek.com`，Anthropic base URL 为 `https://api.deepseek.com/anthropic`，并且 V4 支持 deepseek-v4-pro 和 deepseek-v4-flash 模型。


## 配置参数

### DeepSeek API Configuration

| Parameter             | Value |
|----------------------|-------|
| **base_url (OpenAI)**    | `https://api.deepseek.com` |
| **base_url (Anthropic)** | `https://api.deepseek.com/anthropic` |
| **api_key**              | Apply for an API key |
| **model***               | - `deepseek-v4-flash` <br> - `deepseek-v4-pro` <br> - `deepseek-chat` *(deprecated on 2026/07/24)* <br> - `deepseek-reasoner` *(deprecated on 2026/07/24)* |

那么以为这我们可以直接使用`OpenAIClient` 和 `AnthropicClient` 来接入 DeepSeek API 了。我们通过示例来演示一下



## 基于 OpenAI 协议接入 DeepSeek V4 Pro


OpenAIClient 的接入非常简单，我们只需要在创建 OpenAIClient 实例时，指定 Endpoint 为 DeepSeek 的 API 地址即可。然后我们就可以像平常一样使用 ChatClient 来调用 deepseek-v4-pro 模型了。

```csharp
var client = new OpenAIClient(
    new ApiKeyCredential("sk-xxxxxx"),
    new OpenAIClientOptions
    {
        Endpoint = new Uri("https://api.deepseek.com")
    });

var chatClient = client.GetChatClient("deepseek-v4-pro");

var result = await chatClient.AsAIAgent().RunAsync("你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。");

Console.WriteLine(result);
```

## 基于 Anthropic 协议接入 DeepSeek V4 Pro

AnthropicClient 的接入同样非常简单，我们只需要在创建 AnthropicClient 实例时，指定 BaseUrl 为 DeepSeek 的 Anthropic API 地址即可。然后我们就可以像平常一样使用 ChatClient 来调用 deepseek-v4-pro 模型了。

```csharp
var client2 = new AnthropicClient(new ClientOptions
{
    ApiKey = "sk-xxxxxx",
    BaseUrl = "https://api.deepseek.com/anthropic"
});

var agent = client2.AsIChatClient("deepseek-v4-pro");

result = await agent.AsAIAgent().RunAsync("你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。");

Console.WriteLine(result);
```



## 输出效果

### 基于 OpenAI 协议接入 DeepSeek V4 Pro

[图1]

### 基于 Anthropic 协议接入 DeepSeek V4 Pro

[图1]

## 总结

在前面的内容中，我们尚未对 `OpenAIClient` 和 `AnthropicClient` 的使用进行示例说明。本节将以 DeepSeek 为例，演示其在 OpenAI 协议和 Anthropic 协议下的两种接入方式。DeepSeek 这一波可以说是“一箭双雕”：既兼容 OpenAI 协议，又支持 Anthropic 接口。



