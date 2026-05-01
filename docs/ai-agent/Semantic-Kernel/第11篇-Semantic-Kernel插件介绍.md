
# 第7章-Semantic-Kernel插件介绍

Semantic Kernel（SK）通过插件机制将传统函数与大语言模型（LLM）整合，实现函数调用功能（Function Calling）。本文将带你了解插件的作用、如何定义和注册插件函数，以及如何通过 AI 自动调用它们。

---

## 🔍 什么是插件（Plugin）？

插件是 Semantic Kernel 中的重要组成部分，允许你将多个相关的函数打包为一个模块，供 AI 调用。

> ✅ 本质：SK 会将函数的元数据描述注册给 LLM，模型再根据用户指令自动选择并执行对应函数。

---

## 🧱 如何创建一个插件函数？

每个插件函数必须通过特定的 C# 特性标记，SK 才能识别并让 LLM 调用。

```csharp
using Microsoft.SemanticKernel;

public class TaskManagementPlugin
{
    [KernelFunction("complete_task")]
    [Description("根据任务 ID 完成任务。")]
    [return: Description("返回更新后的任务，若找不到则为 null。")]
    public TaskModel? CompleteTask(int id)
    {
        // 任务处理逻辑
    }
}
```

### ✅ 要点说明：

| 特性 | 作用 |
|------|------|
| `[KernelFunction("函数名")]` | 声明该函数为可被调用的内核函数 |
| `[Description("说明")]` | 提供自然语言函数说明，供 LLM 理解使用 |
| `[return: Description("说明")]` | 描述返回值含义，辅助模型理解输出 |

---

## 📦 如何注册并调用插件函数？

```csharp
var kernel = new KernelBuilder().Build();
kernel.Plugins.AddFromType<TaskManagementPlugin>("TaskManagement");

var arguments = new KernelArguments { ["id"] = 1 };
var updatedTask = await kernel.InvokeAsync("TaskManagement", "complete_task", arguments);
```

通过 `Kernel` 实例手动调用插件函数，适用于业务代码中主动触发。

---

## 🤖 AI 自动调用插件函数（Function Calling）

Semantic Kernel 支持自动由 AI 调用插件函数，只需配置 `FunctionChoiceBehavior.Auto()`：

```csharp
var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};
```

添加用户消息：

```csharp
var history = new ChatHistory();
history.AddUserMessage("请列出所有的紧急任务");
```

通过 `chatCompletionService` 启动对话并自动触发函数调用：

```csharp
var result = await chatCompletionService.GetChatMessageContentAsync(
    history,
    executionSettings: settings,
    kernel: kernel);
```

### 若插件包含如下函数：

```csharp
[KernelFunction("get_critical_tasks")]
[Description("获取所有标记为 '紧急' 的任务列表")]
[return: Description("紧急任务列表")]
public List<TaskModel> GetCriticalTasks()
{
    // ...
}
```

模型将自动调用该函数并将返回结果转化为自然语言。

---

## ✅ 插件开发建议

| 建议 | 说明 |
|------|------|
| 使用清晰且具有描述性的函数名 | 避免缩写，便于 LLM 理解 |
| 函数参数尽量少，使用基本类型 | 降低 token 消耗，提高解析效率 |
| 使用 `[Description]` 补充说明 | 提高函数被正确调用的可能性 |
| 插件聚焦单一业务领域 | 保持结构清晰、便于维护 |

---

## 📌 小结

- 插件机制是 Semantic Kernel 实现 Function Calling 的核心。
- 函数通过 `[KernelFunction]` 等特性暴露给 AI 使用。
- 自动调用模式让用户输入更自然，体验更佳。
- 良好的命名与结构设计是构建智能代理的基础。

借助插件机制，你可以为 AI 添加处理业务逻辑、操作数据库、调用外部服务等能力，构建真正“可执行”的智能体！

---
