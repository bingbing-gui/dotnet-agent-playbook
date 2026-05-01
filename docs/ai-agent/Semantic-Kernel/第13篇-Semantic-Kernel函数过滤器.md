# 🧰 第9章-Semantic-Kernel函数过滤器

Semantic Kernel 提供了强大的过滤器机制（Filters），帮助开发者实现函数调用过程的可观察性、安全性与可控性，构建符合企业标准的 Responsible AI 解决方案。

---

## 🧱 三种过滤器类型

| 类型                         | 作用描述 |
|----------------------------|---------|
| **Function Invocation Filter** | 拦截并处理每一次函数执行（不管是 Prompt 驱动还是 C# 调用） |
| **Prompt Render Filter**      | 在提示词渲染前进行处理，如敏感信息脱敏、缓存处理等 |
| **Auto Function Invocation Filter** | 拦截自动函数调用过程，可实现中途终止、流程控制等 |

---

## 🔍 1. Function Invocation Filter（函数调用过滤器）

该过滤器会在每次函数执行时触发，支持：

- 访问函数元数据和参数
- 执行前后日志记录、权限校验
- 替换结果或切换模型重试

### ✅ 示例：记录函数调用日志

```csharp
public sealed class LoggingFilter(ILogger logger) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        logger.LogInformation("Invoking: {PluginName}.{FunctionName}", context.Function.PluginName, context.Function.Name);

        await next(context); // 注意：必须调用 next 才能继续执行！

        logger.LogInformation("Executed: {PluginName}.{FunctionName}", context.Function.PluginName, context.Function.Name);
    }
}
```

---

## 🧾 2. Prompt Render Filter（提示词渲染过滤器）

该过滤器在 Prompt 渲染前触发，适用于：

- 敏感信息处理（如 PII 脱敏）
- 动态修改提示词
- 启用语义缓存

### ✅ 示例：替换为“安全提示词”

```csharp
public class SafePromptFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next(context);
        context.RenderedPrompt = "Safe and sanitized prompt.";
    }
}
```

---

## ⚙️ 3. Auto Function Invocation Filter（自动函数调用过滤器）

只在自动函数调用流程中触发，适用于：

- 分阶段流程控制
- 条件终止自动调用链

### ✅ 示例：达到期望结果即终止调用链

```csharp
public sealed class EarlyTerminationFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        await next(context);

        var result = context.Result.GetValue<string>();
        if (result == "desired result")
        {
            context.Terminate = true;
        }
    }
}
```

---

## 🧩 如何注册过滤器？

### ✅ 方法一：通过 DI 容器注入

```csharp
builder.Services.AddSingleton<IFunctionInvocationFilter, LoggingFilter>();
```

### ✅ 方法二：直接添加到 Kernel

```csharp
kernel.FunctionInvocationFilters.Add(new LoggingFilter(logger));
```

> ⚠️ 重要：**务必调用 `next(context)` 以继续执行操作链，否则函数将被中断！**

---

## 🎯 小结

| 过滤器类型 | 用途 |
|------------|------|
| FunctionInvocationFilter | 日志、安全、替代执行等通用函数拦截 |
| PromptRenderFilter | Prompt 格式化前的内容管控 |
| AutoFunctionInvocationFilter | 控制自动调用流程、中途终止 |

通过合理集成这些过滤器，开发者可提升系统安全性、透明性和智能性，打造符合生产级要求的 AI 应用。
