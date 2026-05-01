在学习Skill之前我们先来普及一些基础概念。

## 什么是 MCP？它解决了哪些问题，又存在哪些局限？

MCP（Model Context Protocol）在官方文档中的定义是：

> MCP 是一套开源标准，用来让 AI 应用能够连接外部系统。使用 MCP，AI 应用（例如 Claude 或 ChatGPT）可以连接到数据源（例如本地文件、数据库）、工具（例如搜索引擎、计算器）以及工作流（例如特定的 prompt 流程），从而获取关键信息并执行任务。

同时，官方也给了一个很形象的类比：
> 可以把 MCP 理解为 AI 应用的 USB-C 接口。就像 USB-C 提供了一种标准化的设备连接方式一样，MCP 也提供了一种统一的方式，让 AI 应用可以连接到各种外部系统。

从这个角度看，MCP 解决的是一个非常关键的问题：让模型从一个封闭系统，变成一个可以和外部世界交互的开放系统。以前模型只能依赖已有知识进行推理，而有了 MCP 之后，它可以接入实时数据，可以访问业务系统，也可以触发实际操作。这一步，其实是 Agent 能够落地的前提。

但问题也恰恰出在这里。MCP 虽然把工具都接进来了，却没有告诉模型应该怎么用这些工具。当工具数量不多的时候，这个问题还不明显，但一旦工具变多，模型面对的就不再是"能力不足"，而是"信息过多"。它需要在一堆工具里做选择，却缺少明确的判断依据，很容易选错或者用错。

更关键的是，MCP 本身并不提供任何"做事的方法"。它只负责把能力暴露出来，但不会告诉模型在一个具体任务里应该先做什么、再做什么，也不会提供一套稳定的执行流程。对于多步骤的复杂任务来说，这一点影响尤其明显。模型往往只能根据当前的 prompt 临时推理一套方案，结果就是有时候表现很好，有时候又完全跑偏，很难保持稳定。

从工程的角度看，这其实是在说一件很简单的事情：MCP 让模型有机会去调用外部能力，但并不保证一定能用好。它提供的是能力本身，却没有提供使用这些能力的经验。而在真实场景里，后者往往才是决定结果好坏的关键。

也正是在这个背景下，Skill 才变得有意义。它并不是在替代 MCP，而是在 MCP 之上加了一层，让模型在面对具体任务时，不只是"有工具可用"，而是"知道该怎么用这些工具把事情做好"。

## 什么是 Agent Skills

Agent Skills 可以理解为一组模块化的能力扩展，用来让 AI Agent 在特定场景下具备更完整的"做事能力"。和普通工具不太一样，Skill 不只是一个可以调用的功能，它更像是一个打包好的能力单元，里面通常会包含：

- **使用说明（instructions）** —— 告诉模型什么时候用、怎么用
- **基本信息（metadata）** —— 比如名称、描述
- **可选资源** —— 比如 script、templates、参考资料

在运行过程中，Agent 会根据当前任务自动判断是否使用某个 Skill，而不需要手动调用。


## Skill的设计理念

Agent Skills 本质上是一种把“经验”封装起来的方式。 如果说 MCP 给智能体提供的是“手”，让它能够去操作各种工具，那么 Skills 更像是一份“操作手册”或者 SOP（标准作业流程），告诉它在具体场景下应该怎么用这些工具。 这个设计背后其实有一个很简单但挺关键的思路： “能连上工具”和“会用工具”，应该是两件事 MCP 主要解决的是前者 —— 让智能体能够接触到外部的数据和能力。 而 Skills 解决的是后者 —— 在具体场景里，应该怎么组合这些能力，把事情做对。 换句话说，两者的职责其实是分开的： MCP 做的是“连接”：把数据库、API、文件系统这些能力暴露给智能体 Skills 做的是“方法”：告诉智能体在某种任务下，应该怎么用这些能力可以用一个比较直观的类比来理解： MCP 更像是驱动程序或者 USB 接口，解决“设备能不能连上” Skills 更像是一套可复用的操作经验和流程指导，解决“连上之后该怎么用” 比如，你有一个功能完整的打印机驱动（MCP），说明电脑已经可以识别并使用打印机了。 但如果没有人告诉你怎么在 Word 里设置页边距、选择双面打印（Skills），你其实还是很难高效地完成打印任务。

## Agent Skill 文件系统的架构

Agent Skills 最核心的架构是渐进式披露（Progressive Disclosure）机制。这种基于文件系统的架构支持"渐进式披露"：模型根据需要分阶段加载信息，而不是在一开始就消耗上下文。

### 三种 Skill 内容，对应三层加载
Skills 可以包含三种类型的内容，并在不同时间加载：

**第 1 层：元数据（始终加载）**

内容类型（Content type）：Instructions。Skill 的 YAML 前置信息（frontmatter）提供用于发现的相关信息：
```yaml
---
name: unit-converter
description: 用于执行单位转换，通过 value 和 factor 计算结果
---
```
Agent 在启动时会加载这些元数据，并将其纳入系统提示。这种轻量级方式意味着可以安装大量 Skills 而不会显著增加上下文开销；Agent 只需要知道每个 Skill 的存在以及在什么情况下应该使用它。

**第 2 层：指令（在触发时加载）**

内容类型（Content type）：Instructions。SKILL.md 的主体包含过程性知识，例如工作流程、最佳实践以及使用指导：

```
## 使用方法

当用户请求单位转换时：

1. 首先查看 `references/conversion-table.md`，找到对应的换算系数  
2. 运行 `scripts/convert.py` 脚本，并传入参数 `--value <数值> --factor <系数>`  
   （例如：`--value 26.2 --factor 1.60934`）  
3. 将转换结果清晰地展示出来，并同时标明原单位和目标单位

当你的请求与某个 Skill 的描述匹配时，模型会在需要时从文件系统加载 SKILL.md 的内容（具体实现方式依赖运行环境）。只有在此时，这部分内容才会进入上下文窗口。

```

**第 3 层：资源和代码（按需加载）**

内容类型（Content type）：Instructions、Code 和 Resources。Skills 可以打包额外的材料：

```
skills/
└── unit-converter/
    ├── SKILL.md
    ├── references/
    │   └── conversion-table.md
    └── scripts/
        └── convert.py
```

- **Instructions（指令）**：额外的 Markdown 文件（如 FORMS.md、REFERENCE.md），用于提供更专业的指导和流程  
- **Code（代码）**：可执行脚本（如 fill_form.py、validate.py），Claude 通过 bash 运行这些脚本；脚本可以在不消耗上下文的情况下执行确定性操作  
- **Resources（资源）**：参考资料（references），例如数据库 schema、API 文档、模板或示例  


### 加载机制总结

| 层级 | 加载时机 | Token 开销 | 内容 |
|------|----------|------------|------|
| Level 1：Metadata | 始终加载（启动时） | 每个 Skill 约 100 tokens | YAML 中的 name 和 description |
| Level 2：Instructions | Skill 被触发时 | 小于 5k tokens | SKILL.md 主体（指令与指导） |
| Level 3+：Resources | 按需加载 | 基本不受限制 | 通过 bash 执行或读取的文件 |


说到底，MCP 解决的是“能不能做”，但没有解决“该怎么做”。

Skill 正是在这个缺口上出现的。它不去替代 MCP，而是补上一层“经验和方法”。通过分层加载的设计，Skills 让模型在不增加太多上下文负担的情况下，获得必要的指导，从而在面对具体任务时，不只是有工具可用，而是真正知道该怎么把事情做好。


## Agent Framework 里使用Skill

Agent Framework 给我们提供了几种使用Agent的方式。今天我们主要介绍基于文件系统使用Skill的方式。

首先我们需要引用如下包：

``` bash
dotnet add package Azure.AI.OpenAI --version 2.9.0-beta.1
dotnet add package Azure.Identity --version 1.21.0
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.1.0
```

首先我们定义我们的skill目录结构：

📁 Skills 目录结构

```text
skills/
└── unit-converter/
    ├── SKILL.md
    ├── references/
    │   └── conversion-table.md
    └── scripts/
        └── convert.py
```

然后在代码里创建 Agent 的时候，指定一个 AgentSkillsProvider，创建时第一个参数是 Skill 存放的目录，第二个参数是一个委托，用来告诉 Agent Framework怎么运行 Skill 里的scripts脚本：

```csharp
AgentFileSkillScriptRunner myRunner = async (skill, script, args, ct) =>
{
    try
    {
        Console.WriteLine($"运行脚本: {script.Name}");
        // 你在这个方法里面：
        // ✔ 调 Python
        // ✔ 调 HTTP API
        // ✔ 调数据库
        // ✔ 调你自己的 C# 方法
        var psi = new ProcessStartInfo("python")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true, 
            UseShellExecute = false,
        };
        psi.ArgumentList.Add(Path.Combine(skill.Path, script.FullPath));
        if (args != null)
        {
            foreach (var (key, value) in args)
            {
                if (value is not null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    psi.ArgumentList.Add(key);
                    psi.ArgumentList.Add(value.ToString()!);
                }
            }
        }
        using var process = Process.Start(psi)!;
        string output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        Console.WriteLine("STDOUT: " + output);
        //Console.WriteLine("STDERR: " + error);
        await process.WaitForExitAsync();

        return output.Trim();
    }
    catch (Exception ex)
    {
        throw;
    }
};
var skillsProvider = new AgentSkillsProvider(Path.Combine(AppContext.BaseDirectory, "skills"), myRunner);
```

接下来创建 Agent 的时候，把这个 skillsProvider 传进去：
```csharp
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint),
    new AzureCliCredential())
    .GetResponsesClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "UnitConverterAgent",
        ChatOptions = new()
        {
            Instructions = "你是一个可以调用工具进行单位转换的助手。",
        },

        AIContextProviders = [skillsProvider],
    },
    model: deploymentName);

#pragma warning restore OPENAI001

Console.WriteLine("正在使用基于文件的技能进行单位转换");

Console.WriteLine(new string('-', 60));

var stringBuilder = new StringBuilder();
await foreach (var response in agent.RunStreamingAsync("请严格用脚本计算。马拉松（26.2 英里）等于多少公里？另外，75 千克等于多少磅？"))
{
    stringBuilder.Append(response.Text);
}
Console.WriteLine(stringBuilder.ToString());
Console.ReadLine();
```
运行后，Agent 会根据 prompt 里的需求，自动判断需要用到 unit-converter 这个 Skill，然后根据 SKILL.md 里的指导，调用我们在 AgentFileSkillScriptRunner 里定义的方式去运行 convert.py 这个脚本，最终把结果返回给用户。

运行效果


## 🧾 总结

MCP（Model Context Protocol）本质上解决的是“连接能力”的问题，它通过标准化接口，让 AI 应用能够接入外部数据、工具和系统，使模型从一个封闭的推理引擎，升级为可以与现实世界交互的开放系统。这是 Agent 能够真正落地的基础。

但 MCP 的能力主要停留在“把工具接进来”，并不包含“如何使用工具”。当工具数量增加时，模型需要在众多选项中做出决策，却缺乏稳定的判断依据和执行策略，容易出现选择错误或执行不稳定的问题。

因此，从工程角度来看，MCP 提供的是“能力”，而不是“方法”。而 Skill 的价值正是在此基础上进行补充，它通过结构化的方式为模型提供明确的执行路径和使用规范，使模型不仅拥有工具，还具备正确使用工具的能力。

简而言之：

- **MCP 解决的是连接问题（能不能用）**
- **Skill 解决的是使用问题（怎么用好）**

两者结合，才能真正支撑一个稳定、可控且可落地的 Agent 系统。
