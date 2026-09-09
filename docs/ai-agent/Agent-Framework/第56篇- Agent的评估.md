


## Agent Framework 中什么是评估，为什么会有评估？

在 Microsoft Agent Framework 中，**评估（Evaluation）是用于衡量 Agent 运行质量的一套机制**。它不仅关注程序是否能够正常执行，还会进一步判断 Agent 是否正确理解了用户意图、是否选择了合适的工具、工具参数是否准确、是否完成了任务，以及最终回答是否具有相关性、完整性和可靠性。

之所以需要评估，是因为 Agent 的执行过程通常依赖大语言模型进行动态判断，其输出不像传统程序那样完全确定。同样的输入，在不同模型、提示词、工具配置或上下文下，可能产生不同的结果。因此，仅依靠传统的单元测试，很难全面判断 Agent 是否真正“做对了事情”，需要通过 Evaluation 对 Agent 的整体行为和最终结果进行验证。

**Agent Evaluation 很像传统软件开发中“自动化测试 + 验收测试”**。例如开发一个 Web API 后，我们不仅要确认接口能否正常返回 200，还要验证返回的数据是否符合业务要求。Agent Evaluation 做的事情类似，只不过测试对象从“固定的代码逻辑”变成了“Agent 的智能行为”。

可以简单理解为：

**单元测试负责确认程序有没有按照代码设计运行，而 Agent Evaluation 负责确认 Agent 有没有按照业务目标把事情做好。**

Agent Framework 内置了一套评估框架，可用于衡量智能体的质量、安全性和正确性。你可以在开发过程中运行快速的本地检查，也可以使用 Microsoft Foundry 的云端评估器进行生产级评估，还可以在一次评估中将两者结合使用。

该评估框架遵循以下几个核心原则：

- 与提供商无关：核心评估类型和编排函数可与任何评估提供商配合使用。
- 零摩擦上手：只需极少量代码，即可从“我已经有一个智能体”快速过渡到“我已经获得评估结果”。
- 渐进式呈现：简单场景几乎不需要编写代码；高级场景则可以在相同的基础组件之上逐步扩展。

### 核心概念

评估框架建立在三种类型之上：

| 类型 | 用途 |
| --- | --- |
| `EvalItem` | 单个待评估项——封装完整对话，并通过拆分策略派生查询和响应。 |
| `Evaluator` | 为评估项评分的提供程序——可以是本地检查、Microsoft Foundry 或任何自定义实现。 |
| `EvalResults` | 一次评估运行的汇总结果——包括通过/失败数量、每项的详细信息以及可选的门户链接。 |

在 .NET 中，该评估框架构建于 Microsoft.Extensions.AI.Evaluation 之上。评估器实现 IAgentEvaluator 接口，并通过 AIAgent 和 Run 上的扩展方法提供编排功能。


## 本地评估器

LocalEvaluator 在本地运行检查，无需进行 API 调用——非常适合内部开发循环、CI 冒烟测试和快速迭代。它可以接收任意数量的检查函数，并将每个函数应用于每个评估项。

本地评估器使用了`LocalEvaluator`类 位于`Microsoft.Agents.AI`命名空间，运行在当前应用进程、在本地执行。

### 本地内置评估器列表

当前代码中 `EvalChecks` 提供以下内置检查：

| API | 作用 |
| --- | --- |
| `EvalChecks.KeywordCheck(keywords)` | 检查回答是否包含全部指定关键词；默认不区分大小写 |
| `EvalChecks.KeywordCheck(caseSensitive, keywords)` | 同上，但可以指定是否区分大小写 |
| `EvalChecks.ToolCalledCheck(toolNames)` | 检查是否调用了全部指定工具 |
| `EvalChecks.ToolCalledCheck(ToolCalledMode.Any, toolNames)` | 检查是否调用了指定工具中的任意一个 |
| `EvalChecks.ToolCalledCheck(ToolCalledMode.All, toolNames)` | 检查是否调用了全部指定工具 |
| `EvalChecks.ToolCallsPresent()` | 检查对话中是否至少存在一次工具调用 |
| `EvalChecks.ToolCallArgsMatch()` | 检查工具名称及预期参数是否匹配 |
| `EvalChecks.NonEmpty(minLength)` | 检查响应非空，并满足最小长度 |
| `EvalChecks.ContainsExpected(caseSensitive)` | 检查响应是否包含 `ExpectedOutput` |
| `EvalChecks.HasImageContent()` | 检查对话中是否包含图片内容 |


### 示例

```csharp
AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(
        model: deploymentName,
        instructions: "你是一个数学导师。简洁回答，只给出数字结果。",
        name: "MathTutor");
// 内置的检查器可以组合使用，以便在评估时应用多个条件。
LocalEvaluator localEvaluator = new LocalEvaluator(EvalChecks.ContainsExpected(), EvalChecks.NonEmpty());   // 响应必须包含预期答案
                                                                                                            // 响应不能为空
// 查询和预期输出。
string[] queries = ["2+2 是多少?", "144 的平方根是多少?"];
string[] expectedOutputs = ["4", "12"];

// 运行代理并使用预期输出进行评估。
AgentEvaluationResults results = await agent.EvaluateAsync(
    queries,
    localEvaluator,
    expectedOutput: expectedOutputs);
// 打印结果。
Console.WriteLine($"评估: {results.ProviderName}");
Console.WriteLine($"  通过: {results.Passed}/{results.Total}");
Console.WriteLine($"  全部通过: {results.AllPassed}");
Console.WriteLine();
```

### 运行效果


## Microsoft Foundry 评估器

FoundryEvals 连接到 Microsoft Foundry 的评估服务，以进行基于云的 LLM 评判式评估。可以在 Foundry 门户中通过仪表板和对比视图查看结果。

有关项目设置、跟踪评估、评分标准评估器以及可运行的特定服务示例，请参阅 Microsoft Foundry 评估。

Foundry评估器使用了`FoundryEvals`类 位于`Microsoft.Agents.AI.Foundry`命名空间中，在`Microsoft Foundry`服务端执行

### Foundry 内置评估器列表

`FoundryEvals` 当前公开了以下评估器常量：

### Agent 行为评估

| 常量 | 实际名称 | 用途 |
| --- | --- | --- |
| `FoundryEvals.IntentResolution` | `intent_resolution` | 是否正确理解并解决用户意图 |
| `FoundryEvals.TaskAdherence` | `task_adherence` | 是否遵循任务指令 |
| `FoundryEvals.TaskCompletion` | `task_completion` | 是否完成用户要求的任务 |
| `FoundryEvals.TaskNavigationEfficiency` | `task_navigation_efficiency` | 完成任务过程是否高效 |

### 工具使用评估

| 常量 | 实际名称 | 用途 |
| --- | --- | --- |
| `FoundryEvals.ToolCallAccuracy` | `tool_call_accuracy` | 工具调用是否准确 |
| `FoundryEvals.ToolSelection` | `tool_selection` | 是否选择了正确的工具 |
| `FoundryEvals.ToolInputAccuracy` | `tool_input_accuracy` | 传给工具的输入参数是否准确 |
| `FoundryEvals.ToolOutputUtilization` | `tool_output_utilization` | 是否正确利用工具输出 |
| `FoundryEvals.ToolCallSuccess` | `tool_call_success` | 工具调用是否成功 |

### 回答质量评估

| 常量 | 实际名称 | 用途 |
| --- | --- | --- |
| `FoundryEvals.Coherence` | `coherence` | 回答是否连贯 |
| `FoundryEvals.Fluency` | `fluency` | 语言是否流畅 |
| `FoundryEvals.Relevance` | `relevance` | 回答是否与问题相关 |
| `FoundryEvals.Groundedness` | `groundedness` | 回答是否基于所提供的上下文 |
| `FoundryEvals.ResponseCompleteness` | `response_completeness` | 回答是否完整 |
| `FoundryEvals.Similarity` | `similarity` | 回答与预期输出是否相似，需要 ground truth/expected output |

### 安全评估

| 常量 | 实际名称 | 用途 |
| --- | --- | --- |
| `FoundryEvals.Violence` | `violence` | 检测暴力内容 |
| `FoundryEvals.Sexual` | `sexual` | 检测性内容 |
| `FoundryEvals.SelfHarm` | `self_harm` | 检测自残内容 |
| `FoundryEvals.HateUnfairness` | `hate_unfairness` | 检测仇恨或不公平内容 |


**Foundry 默认评估器**

如果创建 FoundryEvals 时不传评估器：

```csharp
FoundryEvals evaluator = new(projectClient, deploymentName);
```

框架默认启用：

- Relevance
- Coherence
- TaskAdherence

另外，如果输入包含工具定义，但没有显式配置任何工具类评估器，框架会自动追加：

- ToolCallAccuracy


### 示例

```csharp

AIProjectClient projectClient = new(new Uri(endpoint), new DefaultAzureCredential());

AIAgent agent = projectClient.AsAIAgent(
    model: deploymentName,
    instructions: "你是一个乐于助人的助手。提供清晰、准确的答案。",
    name: "Joker");

// 配置 Foundry 质量评估器 — 通过 Foundry Evals API 在服务器端运行评估。
FoundryEvals evaluator = new(projectClient, deploymentName, FoundryEvals.Relevance, FoundryEvals.Coherence);

// 运行代理以测试查询并在一次调用中进行评估。
string[] queries = ["什么是光合作用?", "疫苗是如何工作的?"];
AgentEvaluationResults results = await agent.EvaluateAsync(queries, evaluator);

// 输出评估结果。
Console.WriteLine($"通过: {results.Passed}/{results.Total}");
if (results.ReportUrl is not null)
{
    Console.WriteLine($"报告: {results.ReportUrl}");
}

Console.WriteLine();
```




### 运行效果



## 自定义函数评估器

我们还可以创建自定义的评估器，自定义规则由 FunctionEvaluator.Create 定义/包装，由 LocalEvaluator 调度并在本地执行。
这类自定义评估不会调用 Foundry Evals API；

### 示例

```csharp
AIProjectClient projectClient = new(new Uri(endpoint), new DefaultAzureCredential());

AIAgent agent = projectClient.AsAIAgent(
    model: deploymentName,
    instructions: "你是一个客户支持代理。帮助用户解决他们的问题 "
                + "礼貌地提供清晰、可操作的步骤。",
    name: "SupportAgent");

EvalCheck noRefusal = FunctionEvaluator.Create("no_refusal", (string response) =>
    !response.Contains("我不能帮忙", StringComparison.OrdinalIgnoreCase)
    && !response.Contains("我无法帮忙", StringComparison.OrdinalIgnoreCase)
    && !response.Contains("超出我的能力范围", StringComparison.OrdinalIgnoreCase));

EvalCheck hasActionableSteps = FunctionEvaluator.Create("has_actionable_steps", (string response) =>
    response.Contains("1.", StringComparison.Ordinal)
    || response.Contains("- ", StringComparison.Ordinal)
    || response.Contains("• ", StringComparison.Ordinal));

EvalCheck reasonableLength = FunctionEvaluator.Create("reasonable_length", (string response) =>
    response.Length >= 50 && response.Length <= 2000);

LocalEvaluator evaluator = new LocalEvaluator(noRefusal, hasActionableSteps, reasonableLength);

string[] queries =
[
    "我的订单在两周后还没有到。 我该怎么办？",
    "我被同一件商品收取了两次费用。 你能帮忙吗？",
    "我如何退回损坏的产品？",
];

AgentEvaluationResults results = await agent.EvaluateAsync(queries, evaluator);

Console.WriteLine($"通过: {results.Passed}/{results.Total}");
Console.WriteLine();
```

### 运行效果




## 总结

这节我们介绍了什么是评估以及为什么要评估，以及在Agent Framework中常见的三种评估方式，包括本地评估、Foundry评估以及自定义评估。







