前面我们介绍了基于Code-Defined Skill的Agent实现，今天我们来介绍基于Class-Based Skill的Agent实现。Class-Based Skill是把我们的Skill放在一个类里面实现的，这个类需要继承AgentSkill这个基类。本质是一样的，只是组织代码的方式不一样。都是基于AgentSkill这个基类来实现的。


首先我们创建一个控制台的项目，然后引入依赖的包

## 1. 创建项目 + 安装包

先建一个控制台项目，然后把依赖装上：

```bash
dotnet add package Azure.AI.OpenAI --version 2.9.0-beta.1
dotnet add package Azure.Identity --version 1.21.0
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.1.0
```

我们这里需要👉 **Microsoft.Agents.AI**，里面有个类叫 `AgentClassSkill`，这个类继承了`AgentSkill`

## 2. 定义UnitConverterSkill类

作用：
1. 定义一个名为 UnitConverterSkill 的技能类。
2. 继承自AgentClassSkill<UnitConverterSkill>，表示这是一个 class-based skill。

```csharp
internal sealed class UnitConverterSkill : AgentClassSkill<UnitConverterSkill>
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "unit-converter",
        "使用乘法因子在常见单位之间进行转换。当被要求在英里、公里、磅或千克之间换算时使用。");

    protected override string Instructions => """
        当用户请求进行单位换算时，使用此技能。

        1. 查看 conversion-table 资源，找到目标换算所需的因子。
        2. 使用 convert 脚本，并传入数值和表中的因子。
        3. 清晰地给出结果，并同时标明两种单位。
        """;
    
    protected override JsonSerializerOptions? SerializerOptions => null;

    [AgentSkillResource("conversion-table")]
    [Description("常见单位换算的乘法因子查询表。")]
    public string ConversionTable => """
        # 换算表

        公式：**result = value × factor**

        | 从          | 到          | 因子     |
        |-------------|-------------|----------|
        | 英里        | 公里        | 1.60934  |
        | 公里        | 英里        | 0.621371 |
        | 磅          | 千克        | 0.453592 |
        | 千克        | 磅          | 2.20462  |
        """;

    [AgentSkillScript("convert")]
    [Description("将数值与换算因子相乘，并以 JSON 返回结果。")]
    private static string ConvertUnits(double value, double factor)
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    }
}
```

### Frontmatter 属性

```csharp
public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "unit-converter",
        "使用乘法因子在常见单位之间进行转换。当被要求在英里、公里、磅或千克之间换算时使用。");
```

作用：

1. 定义这个 Skill 的基本元信息。
2. 第一个参数 "unit-converter" 是技能名称。
3. 第二个参数是技能描述，告诉 Agent 这个技能适用于什么场景。

### Instructions 属性

```csharp
protected override string Instructions => """
        当用户请求进行单位换算时，使用此技能。
        1. 查看 conversion-table 资源，找到目标换算所需的因子。
        2. 使用 convert 脚本，并传入数值和表中的因子。
        3. 清晰地给出结果，并同时标明两种单位。
        """;
```

作用：
1. 给 Agent 提供使用这个 Skill 的具体步骤。
2. 它告诉 Agent：
   2.1 先查看 conversion-table 资源，找到换算因子。
   2.2 再调用 convert 脚本，把数值和换算因子传进去。
   2.3 最后把换算结果清楚地展示给用户。

### SerializerOptions 属性

```csharp
protected override JsonSerializerOptions? SerializerOptions => null;
```

作用：

1. 控制脚本和资源在参数传递、返回值序列化时使用的 JSON 序列化配置。
2. 这里返回 null，表示使用默认的 JSON 序列化行为。
3. 这个属性不是必须的。


### ConversionTable 属性

```csharp
[AgentSkillResource("conversion-table")]
[Description("常见单位换算的乘法因子查询表。")]
public string ConversionTable => """
    # 换算表

    公式：**result = value × factor**

        | 从          | 到          | 因子     |
        |-------------|-------------|----------|
        | 英里        | 公里        | 1.60934  |
        | 公里        | 英里        | 0.621371 |
        | 磅          | 千克        | 0.453592 |
        | 千克        | 磅          | 2.20462  |
        """;
```
作用：

1. 这是一个 Skill Resource，也就是提供给 Agent 查阅的静态资源。
2. [AgentSkillResource("conversion-table")] 表示这个属性会被注册成名为 conversion-table 的资源。
3. [Description(...)] 是这个资源的说明，帮助 Agent 理解它的用途。
4. 属性返回的是一张 Markdown 格式的单位换算表。

### ConvertUnits 方法

```csharp
[AgentSkillScript("convert")]
[Description("将数值与换算因子相乘，并以 JSON 返回结果。")]
private static string ConvertUnits(double value, double factor)
{
    double result = Math.Round(value * factor, 4);
    return JsonSerializer.Serialize(new { value, factor, result });
}
```

作用：

1. 这是一个 Skill Script，也就是 Agent 可以调用的可执行函数。
2. [AgentSkillScript("convert")] 表示这个方法会被注册为名为 convert 的脚本。
3. [Description(...)] 说明这个脚本的作用：把数值乘以换算因子，并以 JSON 返回结果。
4. 方法接收两个参数：
   - value：要转换的原始数值。
   - factor：换算因子。
5. 方法内部执行：
   - value * factor
   - 然后用 Math.Round(..., 4) 保留 4 位小数。
   - 最后用 JsonSerializer.Serialize(...) 返回 JSON 字符串。




## 3. 注册SKILL

```csharp
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
// --- 配置 ---
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT 未设置。");
string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5.4-mini";

// --- 基于类的技能 ---
var unitConverter = new UnitConverterSkill();
// --- 技能提供程序 ---
#pragma warning disable MAAI001 // 此类型仅用于评估，未来更新中可能更改或移除。抑制此诊断以继续。
var skillsProvider = new AgentSkillsProvider(unitConverter);
#pragma warning restore MAAI001 // 此类型仅用于评估，未来更新中可能更改或移除。抑制此诊断以继续。

// --- Agent 设置 ---
#pragma warning disable OPENAI001 // 此类型仅用于评估，未来更新中可能更改或移除。抑制此诊断以继续。
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetResponsesClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "UnitConverterAgent",
        ChatOptions = new()
        {
            Instructions = "你是一个可以进行单位换算的助手。",
        },
        AIContextProviders = [skillsProvider],
    },
    model: deploymentName);
#pragma warning restore OPENAI001 // 此类型仅用于评估，未来更新中可能更改或移除。抑制此诊断以继续。

// --- 示例：单位换算 ---
Console.WriteLine("使用基于类的技能进行单位换算");
Console.WriteLine(new string('-', 60));

AgentResponse response = await agent.RunAsync(
    "马拉松（26.2 英里）是多少公里？75 千克又是多少磅？");

Console.WriteLine($"助手：{response.Text}");

```

## 4. 运行效果


图片


## 总结

这个示例通过 UnitConverterSkill 演示了如何用 C# 类 定义一个 Agent Skill。
用户并不需要手动指定调用哪个 Skill，而是通过自然语言提出单位换算需求，Agent 会根据 Skill 的 Frontmatter、Instructions、Resource 和 Script 描述自动判断是否使用该 Skill。

UnitConverterSkill 中：

- **Frontmatter**：描述 Skill 的名称和适用场景。
- **Instructions**：告诉 Agent 什么时候使用以及如何使用该 Skill。
- **ConversionTable**：作为 Resource，提供单位换算因子表。
- **ConvertUnits**：作为 Script，负责执行 `value × factor` 的实际计算。
- **AgentSkillsProvider**：将 Skill 注册给 Agent，使大模型能够在对话中发现并调用它。

整体流程如下：

1. 用户提出单位换算问题。
2. Agent 判断是否匹配 `UnitConverterSkill`。
3. Agent 读取换算表。
4. Agent 调用 `convert` 脚本计算结果。
5. Agent 返回自然语言答案。