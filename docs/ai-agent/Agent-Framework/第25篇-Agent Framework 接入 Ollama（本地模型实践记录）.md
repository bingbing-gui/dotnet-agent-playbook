我们之前的案例大多是调用 Azure 上的 OpenAI 模型。  
如果项目不太适合上云（例如涉及敏感数据，或对成本、可控性有要求），那么使用 Ollama 在本地运行模型会是一个更实用的方案。  
这一节介绍 Agent Framework 集成 Ollama 的基本方法。

## 1. 什么是 Ollama？

Ollama 可以理解为一个在本地运行大模型的工具。安装完成后，本机会启动一个服务，你可以像调用接口一样使用模型，而不依赖云服务。

---

首先需要在本地安装 Ollama。  

官网地址：  
https://ollama.com/

【图】

安装过程没有特别之处，Windows 下基本是一路“下一步”。

安装完成后界面也比较简单，但有一点需要注意：**使用前需要先下载模型**。

【图】

模型体积通常不小，常见从几 GB 到十几 GB。这里使用的是：`qwen3:1.7b`。

下载完成后，就可以在本地直接使用。

【视频演示】

---

## 2. 集成到 Agent Framework

接下来把 Ollama 接入 Agent Framework。

安装好之后，Ollama 会在本机起一个服务，默认监听 `11434` 端口。  

你可以把它理解成一个本地的 API 服务，后面不管是浏览器访问，还是在代码里调用，都是通过这个地址和模型交互。

`http://localhost:11434` 其实就是 Ollama 在本地启动的服务地址。

在浏览器打开该地址，如果看到：

【图】

说明服务已正常启动。随后在 Agent Framework 中配置该地址，即可直接调用本地模型。

需要引用如下两个包：

```txt
Microsoft.Agents.AI
OllamaSharp
```

```bash
dotnet add package Microsoft.Agents.AI
dotnet add package OllamaSharp
```

```csharp
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = "http://localhost:11434/";
var modelName = "qwen3:1.7b";

AIAgent agent = new OllamaApiClient(new Uri(endpoint), modelName)
    .AsAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");

await foreach (var chunk in agent.RunStreamingAsync("给我讲一个发生在茶馆里的段子。"))
{
    Console.Write(chunk);
}
```

代码运行如下：

【视频】

## 总结

整体下来，其实流程并不复杂。

Ollama 主要解决的是“本地跑模型”的问题，而 Agent Framework 负责把模型能力组织成 Agent 去使用，两者结合起来之后，就可以在本地完成一套简单的 AI 调用链路。

在一些对数据敏感、或者不方便上云的场景下，这种方式会更灵活，也更可控。

后面如果有需要，还可以在这个基础上继续往上做，比如封装成自己的 API，或者做一层简单的调度。
