## 什么是「结构化输出」？

传统的聊天式 AI，返回的内容往往是一整段自然语言文本，这种形式适合人阅读，但对程序并不友好。

而结构化输出，就是让模型按照我们事先定义好的数据结构返回结果，比如：

```json
{
    "name": "桂兵兵",
    "age": 39,
    "occupation": "软件工程师"
}
```

在 .NET 代码里，它就可以直接对应到一个类，例如：

```csharp
public class PersonInfo
{
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string? Occupation { get; set; }
}
```

这样做的好处是：
- 可编程：拿到结果后，可以直接 personInfo.Age、personInfo.Occupation 来用。
- 可验证：字段缺失、类型不对都能第一时间发现。
- 可组合：很容易把结果传给别的服务、写数据库、生成报表等。

这节示例展示的，就是如何用 Microsoft Agents 框架，让 ChatClientAgent 直接产出这样的结构化结果。

## 整体代码结构解析

主要做了三件事：
- 配置 Azure OpenAI 客户端，并创建一个 ChatClientAgent
- 用泛型的方式，直接拿到 PersonInfo 类型的输出
- 用 JSON Schema（ResponseFormat）方式，在流式响应里拿到结构化数据

我们一步步拆开看。

### 配置 Azure OpenAI ChatClient

```csharp
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

// Create chat client to be used by chat client agents.
ChatClient chatClient = new AzureOpenAIClient(
        new Uri(endpoint),
        new AzureCliCredential())
        .GetChatClient(deploymentName);
```

这里做了几件事：
- 从环境变量里取出：
    - AZURE_OPENAI_ENDPOINT：Azure OpenAI 的 Endpoint
    - AZURE_OPENAI_DEPLOYMENT_NAME：模型部署名（没设置的话默认用 gpt-4o-mini）
- 使用 AzureOpenAIClient + AzureCliCredential 创建一个 ChatClient

这一步是所有后续 Agent 能工作的基础：先有 ChatClient，后有 ChatClientAgent。

### 创建一个基础的 ChatClientAgent

```csharp
// Create the ChatClientAgent with the specified name and instructions.
ChatClientAgent agent = chatClient.CreateAIAgent(new ChatClientAgentOptions(name: "HelpfulAssistant", instructions: "你是一个乐于助人的助手."));
```

这里我们创建了一个名为 "HelpfulAssistant" 的 Agent，并用一句简单的系统提示词说明它的角色：你是一个乐于助人的助手。 这是一个通用型助手，后面我们会在它之上加上结构化输出的能力。

### 定义结构化输出类型 PersonInfo

```csharp
        [Description("个人信息，包括他们的姓名、年龄和职业。")]
        public class PersonInfo
        {
                [JsonPropertyName("name")]
                public string? Name { get; set; }

                [JsonPropertyName("age")]
                public int? Age { get; set; }

                [JsonPropertyName("occupation")]
                public string? Occupation { get; set; }
        }
```

这里值得注意的点：
- 使用了 [Description] 特性，为这个类型加了一段自然语言说明：个人信息，包括他们的姓名、年龄和职业。框架可以利用这些说明，帮模型更好地理解要输出的结构。
- 属性上用了 [JsonPropertyName("xxx")]，对应 JSON 字段名。这样无论 C# 属性命名怎么变，JSON 字段名都是稳定的。

### 用 RunAsync<PersonInfo> 直接拿结构化输出

```csharp
AgentRunResponse<PersonInfo> response = await agent.RunAsync<PersonInfo>(
        "请提供关于桂兵兵的信息，他是一名 39 岁的软件工程师。"
);
```

这里的关键是：`RunAsync<PersonInfo>` 这个泛型参数。

我们通过类型参数 PersonInfo 告诉 Agent：你需要产出的是这个结构。框架会自动帮我们：
- 指导模型按照这个结构来组织回答；
- 把模型的输出解析并反序列化为 PersonInfo 对象。

拿到结果后，就可以直接以强类型的方式来使用：

```csharp
Console.WriteLine("助理输出:");
Console.WriteLine($"姓名: {response.Result.Name}");
Console.WriteLine($"年龄: {response.Result.Age}");
Console.WriteLine($"职业: {response.Result.Occupation}");
```

从这一步开始，你已经不再需要靠字符串处理去提取信息了。

## 用 JSON Schema + 流式输出拿结构化数据

上面我们是一次性拿到完整结果。而在很多业务场景里，我们希望支持流式输出（Streaming）：
- 让用户更快看到回复的第一部分；
- 边流式接收边处理，提升响应体验。

这就用到了示例里的第二部分代码。

### 指定 ResponseFormat 为 JSON Schema

```csharp
// Create the ChatClientAgent with the specified name, instructions, and expected structured output the agent should produce.
ChatClientAgent agentWithPersonInfo = chatClient.CreateAIAgent(new ChatClientAgentOptions(name: "HelpfulAssistant", instructions: "你是一个乐于助人的助手。")
{
        ChatOptions = new()
        {
                ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema<PersonInfo>()
        }
});
```

这段代码与前面的区别在于：
- 多设置了 ChatOptions
- ChatOptions.ResponseFormat 被显式配置为：`ChatResponseFormat.ForJsonSchema<PersonInfo>()`，意思就是告诉模型，请严格按照 PersonInfo 对应的 JSON Schema 来输出结果。

和前面的泛型方法相比，这种方式更显式地把「结构化 JSON」的要求传递给模型，是更专业的接口级用法。

### 以流式方式获取响应

```csharp
// Invoke the agent with some unstructured input while streaming, to extract the structured information from.
var updates = agentWithPersonInfo.RunStreamingAsync("请提供关于桂兵兵的信息，他是一名 39 岁的软件工程师。");
```

这里调用的是 `RunStreamingAsync`，返回的是一个「更新流」（updates）：
- 你可以像监听事件一样，一点点收到模型生成的内容；
- 示例中选择先把所有流接收完，再整体反序列化。

### 汇总流式结果，并反序列化为 PersonInfo

```csharp
PersonInfo personInfo = (await updates.ToAgentRunResponseAsync()).Deserialize<PersonInfo>(JsonSerializerOptions.Web);

Console.WriteLine("助理输出:");
Console.WriteLine($"姓名: {personInfo.Name}");
Console.WriteLine($"年龄: {personInfo.Age}");
Console.WriteLine($"职业: {personInfo.Occupation}");
```

流程是这样的：
- `updates.ToAgentRunResponseAsync()`：把流式更新整合成一个完整的 AgentRunResponse 对象；
- `.Deserialize<PersonInfo>(JsonSerializerOptions.Web)`：使用 System.Text.Json 的 Web 风格配置，把 JSON 解析为 PersonInfo；
- 最后同样以强类型方式来访问属性并输出。

通过这一步，你就完成了：在流式响应场景下的结构化输出解析。

## 两种方式对比：该用哪一种？

- 泛型 `RunAsync<PersonInfo>`
    - 写法简单，直接：`await agent.RunAsync<PersonInfo>(prompt)`
    - 适合非流式场景，拿到完整结果即可
    - 更像是一种「高级封装」

- 显式 `ResponseFormat.ForJsonSchema<PersonInfo>()` + `RunStreamingAsync`
    - 明确指定返回格式为 JSON Schema
    - 支持流式输出，适合对实时性要求较高的交互式应用
    - 适合更复杂的生产环境，比如前端逐字显示内容、中间过程要做日志记录、监控、截断等

简单建议：
- 控制台小工具 / 后台任务：用 `RunAsync<PersonInfo>` 就够了；
- 对用户体验和可观测性要求高的应用：考虑使用 `ResponseFormat` + `RunStreamingAsync` 的组合。

## 实际业务中可以怎么玩？

有了「结构化输出」这个能力，你可以很方便地构建各种「AI 数据接口」，比如：
- 用户信息提取：输入一段自然语言的自我介绍，输出一个标准化的 UserProfile 对象
- 订单信息抽取：从用户的聊天记录中提取 ProductId、数量、地址等，生成 OrderInfo
- 文本分析：让模型输出 SentimentResult、KeyPhrasesResult 等结构化分析结果
- 决策辅助：输出一个 RecommendationResult，里面包含推荐理由、分数、风险等级等

在这些场景中，借助 Microsoft Agents 框架 + Azure OpenAI + .NET：
- 上游只需给一个较为自然的 Prompt
- 中间由 Agent 帮你「把人话变成结构化数据」
- 下游可以直接当作普通的 C# 对象来用

## 小结

这一节的示例展示了如何在 .NET 中，使用 Microsoft Agents 框架配置 ChatClientAgent，让大模型输出结构化的数据：
- 利用 `RunAsync<PersonInfo>`，直接拿到强类型的结构化结果；
- 利用 `ResponseFormat.ForJsonSchema<PersonInfo>()` + `RunStreamingAsync`，在流式场景下也能可靠地解析结构化输出；
- 通过定义 PersonInfo 等类型，把「模型输出」变成「可编程、可验证、可组合」的数据资源。

你可以在此基础上：
- 替换成你自己的业务类型（比如 OrderInfo, UserProfile, ReportSummary）
- 接上数据库、消息队列、REST API
- 打造属于你自己的「AI 驱动业务流程」

