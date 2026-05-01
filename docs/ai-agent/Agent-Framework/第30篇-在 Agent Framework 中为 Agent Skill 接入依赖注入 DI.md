在前面的文章中，我们介绍过 FileBased、CodeBased 和 ClassBased 等不同的 Skill 实现方式，也演示了如何通过 AgentSkillsProvider 或 AgentSkillsProviderBuilder 将多个 Skill 组合起来，让一个 Agent 同时具备多种能力。

在实际项目中，Skill 通常不只是简单的本地函数，它往往需要依赖应用中的各种服务，例如：

- 数据库访问
- HTTP API 调用
- 缓存或配置服务

如果每个 Skill 都自行创建和管理这些依赖，不仅会导致代码重复，还会增加测试和维护的成本。

因此，在 Agent Framework 中，可以将 Skill 与 .NET 的依赖注入机制结合使用。

下面通过一个单位换算的例子，来看一下如何在 Skill 中使用 DI。

---

## 1. 核心设计思想

在 Agent 架构中：

`用户 → Agent → Skill → DI → Service → 业务逻辑`

分层：

| 层级 | 职责 |
| --- | --- |
| Agent | 理解用户意图 |
| Skill | 能力入口（AI 可调用） |
| Service | 业务实现 |
| DI | 生命周期与依赖管理 |

> Skill 不承载业务逻辑，只负责能力编排。

---

## 2. 示例场景

构建一个“单位换算 Agent”，支持：

- 距离换算（英里 ⇄ 千米）
- 重量换算（磅 ⇄ 千克）

实现方式：

| Skill | 类型 | 功能 |
| --- | --- | --- |
| `distance-converter` | Inline | 距离 |
| `weight-converter` | Class | 重量 |

两者共用同一个业务服务：`ConversionService`

## 2. 创建项目并安装依赖包

首先创建一个控制台项目，然后安装相关依赖包：

```bash
dotnet add package Azure.AI.OpenAI
dotnet add package Azure.Identity
dotnet add package Microsoft.Agents.AI.OpenAI
dotnet add package Microsoft.Extensions.DependencyInjection
```

## 3. 环境准备

安装依赖：

```bash
dotnet add package Azure.AI.OpenAI --version 2.9.0-beta.1
dotnet add package Azure.Identity
dotnet add package Microsoft.Agents.AI.OpenAI
dotnet add package Microsoft.Extensions.DependencyInjection
```

> 版本说明：
> `GetResponsesClient()` 需要 `Azure.AI.OpenAI >= 2.9.0-beta.1`，否则可能出现 `404` 或 API 不兼容问题。

## 4. 配置 Azure OpenAI

示例中通过环境变量读取 Azure OpenAI 的终结点和模型部署名称：

```csharp
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? "gpt-5.4-mini";
```

这里需要提前配置环境变量：

在Windows中可以使用：

```bash
setx AZURE_OPENAI_ENDPOINT "https://你的资源名.openai.azure.com/"
setx AZURE_OPENAI_DEPLOYMENT_NAME "你的模型部署名"
```

在 Linux 或 macOS 中可以使用：

```bash
export AZURE_OPENAI_ENDPOINT="https://你的资源名.openai.azure.com/"
export AZURE_OPENAI_DEPLOYMENT_NAME="你的模型部署名"
```

---

## 5. 创建 DI 容器

在.NET 中，依赖注入通常从 ServiceCollection 开始。

示例中注册了一个 ConversionService：

```csharp
ServiceCollection services = new();
services.AddSingleton<ConversionService>();
IServiceProvider serviceProvider = services.BuildServiceProvider();
```

---

## 6. 定义业务服务 ConversionService

ConversionService 是一个普通的 C# 服务类，用于提供单位换算能力。

它主要包含三个方法：

```csharp
internal sealed class ConversionService
{
    public string GetDistanceTable() =>
        """
        # 距离转换

        公式：**结果 = 值 × 系数**

        | 从          | 到          | 系数     |
        |-------------|-------------|----------|
        | 英里        | 千米        | 1.60934  |
        | 千米        | 英里        | 0.621371 |
        """;

    public string GetWeightTable() =>
        """
        # 重量转换

        公式：**结果 = 值 × 系数**

        | 从          | 到          | 系数     |
        |-------------|-------------|----------|
        | 磅          | 千克        | 0.453592 |
        | 千克        | 磅          | 2.20462  |
        """;

    public string Convert(double value, double factor)
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    }
}
```

这里的设计思路很清晰：

- `GetDistanceTable()` 提供距离换算表。
- `GetWeightTable()` 提供重量换算表。
- `Convert()` 根据传入的值和换算系数返回计算结果。

---

## 7. 使用 AgentInlineSkill 实现距离换算

第一个 Skill 使用 AgentInlineSkill 实现。

它适合比较简单、轻量的场景，直接在代码中定义 Skill 的资源和脚本。

```csharp
var distanceSkill = new AgentInlineSkill(
    name: "distance-converter",
    description: "在距离单位之间转换。当要求将英里转换为千米或将千米转换为英里时使用。",
    instructions: """
        当用户要求在距离单位（英里和千米）之间转换时，请使用此技能。

        1. 查看 distance-table 资源，找到所请求转换的系数。
        2. 使用 convert 脚本，传入值和表中的系数。
        """)
    .AddResource("distance-table", (IServiceProvider serviceProvider) =>
    {
        var service = serviceProvider.GetRequiredService<ConversionService>();
        return service.GetDistanceTable();
    })
    .AddScript("convert", (double value, double factor, IServiceProvider serviceProvider) =>
    {
        var service = serviceProvider.GetRequiredService<ConversionService>();
        return service.Convert(value, factor);
    });
```

这里有一个关键点：资源和脚本方法中都声明了 `IServiceProvider` 参数。

例如：

```csharp
.AddResource("distance-table", (IServiceProvider serviceProvider) =>
{
    var service = serviceProvider.GetRequiredService<ConversionService>();
    return service.GetDistanceTable();
})
```

Agent Framework 会自动注入 `IServiceProvider`。

然后我们就可以通过：`var service = serviceProvider.GetRequiredService<ConversionService>();`

距离换算的执行过程大致如下：

1. 用户提出距离换算问题。
2. Agent 判断需要使用 `distance-converter`。
3. Skill 读取 `distance-table` 资源。
4. Agent 根据换算表选择正确的系数。
5. 调用 `convert` 脚本完成计算。
6. 返回最终结果。

---

## 8. 使用 AgentClassSkill 实现重量换算

第二个 Skill 使用基于类的方式实现，也就是 AgentClassSkill。
这种方式更适合结构稍复杂的 Skill，因为可以把资源、脚本、说明信息都封装在一个类中。

```csharp
internal sealed class WeightConverterSkill : AgentClassSkill<WeightConverterSkill>
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "weight-converter",
        "在重量单位之间转换。当要求将磅转换为千克或将千克转换为磅时使用。");

    protected override string Instructions => """
        当用户要求在重量单位（磅和千克）之间转换时，请使用此技能。

        1. 查看 weight-table 资源，找到所请求转换的系数。
        2. 使用 convert 脚本，传入值和表中的系数。
        3. 使用两个单位清晰呈现结果。
        """;

    [AgentSkillResource("weight-table")]
    [Description("重量转换乘法系数的查找表。")] 
    private static string GetWeightTable(IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<ConversionService>();
        return service.GetWeightTable();
    }

    [AgentSkillScript("convert")]
    [Description("将值乘以转换系数，并以 JSON 形式返回结果。")] 
    private static string Convert(double value, double factor, IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<ConversionService>();
        return service.Convert(value, factor);
    }
}
```

基于ClassBased的 Skill里面属性和方法的作用，我们在前面的文章中已经介绍过了，这里不再赘述。
 

在类形式的 Skill 中，同样可以通过 `IServiceProvider` 使用 DI。

例如资源方法：

```csharp
[AgentSkillResource("weight-table")]
[Description("重量转换乘法系数的查找表。")] 
private static string GetWeightTable(IServiceProvider serviceProvider)
{
    var service = serviceProvider.GetRequiredService<ConversionService>();
    return service.GetWeightTable();
}
```

脚本方法：

```csharp
[AgentSkillScript("convert")]
[Description("将值乘以转换系数，并以 JSON 形式返回结果。")] 
private static string Convert(double value, double factor, IServiceProvider serviceProvider)
{
    var service = serviceProvider.GetRequiredService<ConversionService>();
    return service.Convert(value, factor);
}
```

只要方法参数中声明了`IServiceProvider`, Agent Framework 就会在执行时自动注入它。这样，ClassBased Skill 也可以和普通 ASP.NET Core 或 Worker Service 一样，通过 DI 使用应用中的业务服务。

---

## 9. 注册多个 Skill

定义好距离 Skill 和重量 Skill 后，需要把它们注册到统一的技能提供者中。

```csharp
var weightSkill = new WeightConverterSkill();

var skillsProvider = new AgentSkillsProvider(distanceSkill, weightSkill);
```

它们分别负责不同的领域：

| Skill | 实现方式 | 能力 |
|---|---|---|
| `distance-converter` | AgentInlineSkill | 英里和千米转换 |
| `weight-converter` | AgentClassSkill | 磅和千克转换 |

这意味着同一个 Agent 可以根据用户问题自动选择合适的 Skill。

如果用户问：

> 26.2 英里是多少千米？

Agent 会倾向于使用 `distance-converter`。

如果用户问：

> 75 千克是多少磅？

Agent 会倾向于使用 `weight-converter`。

如果用户同时问两个问题，Agent 也可以分别调用不同的 Skill 来完成任务。

---

## 10. 创建 Agent 并注入AgentSkillsProvider和IServiceProvider

接下来创建 Agent：

```csharp
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetResponsesClient()
    .AsAIAgent(
        options: new ChatClientAgentOptions
        {
            Name = "UnitConverterAgent",
            ChatOptions = new()
            {
                Instructions = "你是一个可以转换单位的实用助手。",
            },
            AIContextProviders = [skillsProvider],
        },
        model: deploymentName,
        services: serviceProvider);
```

这里有两个地方非常重要。

第一个是：

```csharp
AIContextProviders = [skillsProvider]
```

这表示把前面定义的 Skill 提供给 Agent。

第二个是：`services: serviceProvider`

这表示把 DI 容器传递给 Agent。
正因为传入了 `serviceProvider`，所以 Skill 在执行资源和脚本时，才能自动获得 `IServiceProvider`，然后解析出 `ConversionService`。
如果没有传入这个服务提供者，Skill 中依赖 DI 的代码就无法正常解析服务。

---

## 11. 运行示例

最后，我们向 Agent 提出一个同时包含距离和重量换算的问题：

```csharp
AgentResponse response = await agent.RunAsync(
    "一场马拉松（26.2 英里）是多少千米？75 千克是多少磅？");

Console.WriteLine($"智能体：{response.Text}");
```



根据示例中的转换系数，计算结果如下：



## 12. 为什么要在 Skill 中使用 DI？

把 DI 引入 Agent Skill，最大的好处是让 Skill 不再直接依赖具体实现。
比如在当前示例中，Skill 并不负责保存换算表，也不直接维护业务规则，而是通过`ConversionService`来完成具体业务。

这样做有几个好处。

### 1. 业务逻辑可以复用

ConversionService 不只可以被 Agent Skill 使用，也可以被普通 API、后台任务、命令行工具或测试代码使用。
Skill 只是业务能力的一种入口。

### 2. 更容易测试

如果未来要测试 Skill，可以替换掉真实的服务实现，注入 Mock 服务或测试服务。
例如可以把`ConversionService` 抽象成接口 `IConversionService` 然后在测试中注入假的实现。

### 3. 更容易扩展

如果以后换算规则不再写死在代码中，而是来自数据库，只需要修改服务层即可。
Skill 的调用方式可以保持不变。

### 4. 更符合 .NET 应用架构

在 ASP.NET Core、Worker Service 和现代 .NET 应用中，DI 是非常核心的基础设施。
Agent Skill 支持 DI，意味着它可以自然融入现有 .NET 应用架构。

---

## 13. Agent Skill 和传统服务的关系

可以把 Agent Skill 理解成一层“AI 可调用的能力入口”。
传统代码中，我们可能这样调用服务：

```csharp
var result = conversionService.Convert(value, factor);
```

```
而在 Agent Framework 中，用户通过自然语言提出问题：
> 26.2 英里是多少千米？

```
Agent 会根据问题自动选择 Skill，然后 Skill 再调用服务：

```csharp
var service = serviceProvider.GetRequiredService<ConversionService>();
return service.Convert(value, factor);
```

所以它们的关系可以理解为：

```
用户自然语言
    ↓
Agent
    ↓
Skill
    ↓
DI 服务
    ↓
业务逻辑
```
Skill 并不是替代业务服务，而是把业务服务包装成 Agent 可以理解和调用的能力。

## 总结

本文演示了如何在 Agent Framework 中结合依赖注入使用 Agent Skill。
示例中包含两种 Skill：
1. AgentInlineSkill：用于距离换算。
2. AgentClassSkill：用于重量换算。

这两个 Skill 都没有直接维护复杂业务逻辑，而是通过 IServiceProvider 从 DI 容器中解析同一个 ConversionService。

通过这种方式，Agent Skill 可以像普通 .NET 组件一样使用应用中的服务。
这让 Skill 的设计更加清晰，也让 Agent 更容易接入真实业务系统。
随着 Skill 接入的服务越来越多，Agent 的能力也会不断增强。从简单的单位换算，到数据库查询、HTTP API 调用、企业系统集成，最终都可以通过同样的方式扩展出来。
