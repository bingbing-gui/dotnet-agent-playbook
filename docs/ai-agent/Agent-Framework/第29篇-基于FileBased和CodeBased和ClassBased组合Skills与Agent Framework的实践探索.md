我们在前面的文章中介绍了FileBased、CodeBased和ClassBased三种不同的技能实现方式，并且在Agent Framework中也提供了相应的支持。
在Agent Framework中，我们可以通过组合不同类型的技能来让一个 Agent 同时具备多种能力。这一节将介绍如何在Agent Framework中使用FileBased、CodeBased和ClassBased技能，例如：

- 电商系统中的单位换算（重量 / 体积）
- 国际化应用中的温度转换
- 数据分析中的多单位统一处理


## 1. 创建项目 + 安装包

先建一个控制台项目，然后把依赖装上：

```bash
dotnet add package Azure.AI.OpenAI --version 2.9.0-beta.1
dotnet add package Azure.Identity --version 1.21.0
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.1.0
```

需要注意的是，如果使用 `GetResponsesClient()`，必须安装 `Azure.AI.OpenAI 2.9.0-beta.1` 及以上版本，否则可能会出现接口不可用或运行时报错（如 404）的情况。

## 2. 组合Skills

前面我们已经分别实现了三种 Skill：

- File-Based（基于文件）
- Code-Based（基于代码）
- Class-Based（基于类）

但这些方式本质上都是在使用“单个 Skill”，对应的类是 `AgentSkillsProvider`。

在实际场景中，我们往往需要让一个 Agent 同时具备多种能力，这时候就需要将多个 Skill 组合起来。这里使用 `AgentSkillsProviderBuilder` 来组合不同来源的 Skill，是实现多能力 Agent 的关键。

```csharp
var skillsProvider = new AgentSkillsProviderBuilder()
    .UseFileSkill(Path.Combine(AppContext.BaseDirectory, "skills"))    // 基于文件unit-converter
    .UseSkill(volumeConverterSkill)                                    // 代码定volume-converter
    .UseSkill(temperatureConverter)                                    // 基于类temperature-converter
    .UseFileScriptRunner(myRunner)
    .Build();
```

我们把三种技能组合在一起，形成一个统一的技能提供者 `skillsProvider`，并注入到 Agent 的 `AIContextProviders` 中。

```csharp
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetResponsesClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "MultiConverterAgent",
        ChatOptions = new()
        {
            Instructions = "你是一个乐于助人的助手，可以进行单位、体积和温度转换。",
        },
        AIContextProviders = [skillsProvider],
    },
    model: deploymentName);
```

这里的 `AIContextProviders` 会把所有 Skill 暴露给模型。

模型在推理过程中会根据用户的问题，自动判断是否需要调用某个 Skill，并选择合适的工具来完成任务（类似 Function Calling 的机制）。

## 3. 运行效果

```csharp
await foreach (var response in agent.RunStreamingAsync(
    "我需要三个换算：" +
    "1）马拉松（26.2 英里）是多少公里？" +
    "2）5 加仑是多少升？" +
    "3）98.6°F 是多少摄氏度？"))
{
    Console.WriteLine(response.Text);
}
```
输出示例：

tupian


## 🔗 Skill 的扩展能力

其实 Skill 不只是本地写的代码，你也可以把它理解成一层“对外能力的入口”，可以接各种东西：

- 🌍 **HTTP API**：比如调天气、地图、支付这些接口  
- 🗄️ **数据库查询**：直接查你系统里的业务数据  
- 🤖 **其他 AI 模型**：多个 Agent 一起配合干活  
- 🔌 **外部协议**：以后也可以接像 MCP 这种标准能力  

💡 说白了：你能接多少东西，Agent 就能干多少事。


## 总结

`AgentSkillsProviderBuilder` 提供了一种统一的组合方式，可以将通过不同路径实现的 Skill（File / Code / Class）整合在一起。

通过这种方式，Agent 不再只是单一能力的工具，而是可以根据用户问题，自动选择并调用不同 Skill 来完成任务。

随着 Skill 的不断扩展（HTTP、数据库、AI 等），Agent 的能力也会随之增强，随着 Skill 的不断扩展（HTTP、数据库、AI 等），Agent 的能力也会持续增强，能够处理更加复杂的任务。


