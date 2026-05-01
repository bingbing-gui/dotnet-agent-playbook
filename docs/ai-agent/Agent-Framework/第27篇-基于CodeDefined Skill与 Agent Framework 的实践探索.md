# 基于 Code-Defined Skill 的 Agent 实现

上一节我们讲了 File-Based Skill，这一节介绍另外一种方式，直接用代码来写 Skill。

## 简单说一下区别

- **File-Based**：用 SKILL.md + 脚本 + 资源文件
- **Code-Defined**：全部写在代码里

## 1. 创建项目 + 安装包

先建一个控制台项目，然后把依赖装上：

```bash
dotnet add package Azure.AI.OpenAI --version 2.9.0-beta.1
dotnet add package Azure.Identity --version 1.21.0
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.1.0
```

这里最关键的是：

👉 **Microsoft.Agents.AI**

里面有个类叫 `AgentInlineSkill`，就是我们这节要用的核心。

## 2. 定义一个 Skill

我们还是用"单位转换"这个例子，简单直观一点。

先看整体代码：

```csharp
var unitConverterSkill = new AgentInlineSkill(
    name: "unit-converter",
    description: "用于常见单位之间的转换",
    instructions: """
        当用户需要做单位转换时：

        1. 查 conversion-table 找系数
        2. 查 conversion-policy 看规则
        3. 调用 convert 脚本计算
        """)
```

这一段其实就是在告诉 Agent：

👉 **"什么时候用我，以及怎么用我"**

### ① 静态资源（直接写死在代码里）

```csharp
.AddResource(
    "conversion-table",
    """
    | 从   | 到   | 系数     |
    |------|------|----------|
    | 英里 | 公里 | 1.60934  |
    | 公里 | 英里 | 0.621371 |
    | 磅   | 千克 | 0.453592 |
    | 千克 | 磅   | 2.20462  |
    """)
```

就是一张表，没什么特别的。

### ② 动态资源（运行时生成）

```csharp
.AddResource("conversion-policy", () =>
{
    const int Precision = 4;
    return $"""
        小数位：{Precision}
        时间：{DateTime.UtcNow:O}
        """;
})
```

这个更像"运行时配置"，可以根据情况动态变。

### ③ 脚本（直接写在代码里）

```csharp
.AddScript("convert", (double value, double factor) =>
{
    double result = Math.Round(value * factor, 4);
    return JsonSerializer.Serialize(new { value, factor, result });
});
```

就是一个函数。

👉 **Agent 需要的时候会自动调这个。**

## 3. 注册 Skill

```csharp
var skillsProvider = new AgentSkillsProvider(unitConverterSkill);
```
可以理解成：把我们的skill放在技能包里面。

## 4. 创建 Agent

```csharp
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetResponsesClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "UnitConverterAgent",
        ChatOptions = new()
        {
            Instructions = "你是一个可以做单位转换的助手",
        },
        AIContextProviders = [skillsProvider],
    },
    model: deploymentName);
```

关键点就一个：

👉 **`AIContextProviders = [skillsProvider]`**

把 Skill 挂进去就行了。

## 5. 调用测试

```csharp
var sb = new StringBuilder();

await foreach (var res in agent.RunStreamingAsync(
    "请用脚本计算：26.2 英里是多少公里？75 千克是多少磅？"))
{
    sb.Append(res.Text);
}

Console.WriteLine(sb.ToString());
```

输出大概是这样：

```
26.2 英里 ≈ 42.1647 公里
75 千克 ≈ 165.3465 磅
```

## 总结

> **Code-Defined** 更像"直接写死在代码里"，
> **File-Based** 更像"外挂插件"。