
本系列文章将系统介绍 [Agent Framework](#) 的基本使用方法与核心概念，适合初学者快速入门，也适用于已有经验的开发者进行体系化学习。文中涉及的示例代码均已整理至代码仓库，欢迎大家前往查看并，仓库地址 `https://github.com/bingbing-gui/dotnet-agent-playbook`⭐️ Star 支持

随着 Agent Framework 于 **2026 年 4 月 3 日正式发布 1.0（GA）版本**，本系列内容也将逐步进行升级与完善。需要特别说明的是：

---

### 📌 历史版本说明

早期文章中的示例代码基于 *Prerelease* 版本编写，部分 API 与当前正式版本存在差异。后续将逐步完成向正式版本的迁移与更新。

不同版本的变更记录可参考官方发布说明：  
`https://github.com/microsoft/agent-framework/releases`

随着后续案例的持续完善，部分基于 Prerelease 版本实现的示例将逐步被新的实践案例所替换与覆盖，以保持整体内容与正式版本的一致性。

---

### 📦 代码仓库重构

笔者对仓库进行了重构与优化，之前仓库下的大概几百篇涉及`AspNetCore`的文章的案例以迁移到`_archive`目录下，仓库重命名`dotnet-agent-playbook` 将打造一个dotnet AI Agent实战指南，涵盖从基础到进阶的多个主题模块。老的项目结构已调整，新的目录结构更清晰，便于大家查找与学习。请以本篇文章中列举的 GitHub 地址为准。

仓库地址 `https://github.com/bingbing-gui/dotnet-agent-playbook`⭐️ Star 支持

---

### 🚀 后续规划（重点）

在后续内容中，将重点探索 Agent Framework 中的 **Workflows（工作流编排）能力**。

该能力在设计理念上与 LangChain 生态中的 LangGraph 存在一定相似性，主要用于构建多 Agent 协作、任务流转以及复杂业务流程的编排与控制。

> Workflows 本质上是一种面向 Agent 的流程编排机制，用于描述任务在多个 Agent、工具与状态之间的流转关系。

---

后续将推出 **Agent Framework 进阶篇**，深入讲解多 Agent 编排、状态管理以及企业级应用实践，敬请期待。

---

> ⚠️ **重要提示**
>
> 由于仓库结构已进行重构，部分历史文章中的 Demo 示例路径已发生变化，请以本篇文章提供的仓库地址为准。
>
> 包括《Semantic Kernel 二十二篇：从零到实战 AI 智能体》系列中的部分示例代码，也已同步迁移至新仓库中。

---

> 📌 **仓库地址**  
> https://github.com/bingbing-gui/dotnet-agent-playbook

---

### 📚 相关示例导航

- [Semantic Kernel 二十二篇：从零到实战 AI 智能体](https://mp.weixin.qq.com/s/cHfZJlYnhkoozE0pBt_CeA)  
- Semantic Kernel 中 Function Calling
- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Semantic-Kernel/SK.FunctionCalling`  
- Semantic Kernel 插件（Plugins）开发指南
- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Semantic-Kernel/SK.MCP.Plugins`  
- Semantic Kernel 中 Run Prompts
- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Semantic-Kernel/SK.RunPrompts`  
- [Semantic Kernel 中文本向量（Embedding）生成]
- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Semantic-Kernel/SK.TextEmbeddingGeneration`  
- [Semantic Kernel 向量搜索（Vector Store）实践]
- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Semantic-Kernel/SK.VectorStores`

---

⭐ 欢迎大家前往查看并 Star 支持！


### 🗂️ 系列目录（第1篇～第32篇）

**第1篇｜使用Agent Framework构建你的第一个Agent应用**  
   - 📄 [文章地址](https://mp.weixin.qq.com/s/tBOMo1AXqzZEjeBwirIRFQ)  
   - 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/01-Agent-Running`

**第2篇｜Agent Thread实现同一Agent的多轮回话**  
   - 📄 [文章地址](https://mp.weixin.qq.com/s/JJgGAFn-ronUIBoVVukplg)  
   - 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/02-Multiturn-Conversation`

**第3篇｜Agent Framework调用工具**  
   - 📄 [文章地址](https://mp.weixin.qq.com/s/Tj9Bem2BtYPsJDqnOIU19g)  
   - 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/03-FunctionTools`

**第4篇｜Agent Framework的人工审批机制，确保本地函数调用安全可控**  
   - 📄 [文章地址](https://mp.weixin.qq.com/s/rjFkRi87dsOsicqAREMy-w)  
   - 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/04-FunctionTools-WithApprovals`

**第5篇｜Agent Framework结构化数据**  
   - 📄 [文章地址](https://mp.weixin.qq.com/s/zDpcEMG-cHi0DluKdebjng)  
   - 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/05-StructuredOutput`

**第6篇｜Agent-Framework实现Agent会话持久化**  
   - 📄 [文章地址](https://mp.weixin.qq.com/s/Va5318wZ0F8kX_QbF8jABA)  
   - 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/06-PersistedConversations`

**第7篇｜Agent Framework链接外部存储资源**  
   - 📄 [文章地址](https://mp.weixin.qq.com/s/82ApFOWuiP_kmVdAJ9QtrA)  
   - 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/07-StorageConversations`

**第8篇｜10行代码搞定Agent的全链路监控**  
   - 📄 [文章地址](https://mp.weixin.qq.com/s/_BReArBFUfGCikoTFIZ6Gg)  
   - 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/08-AgentObservability`

**第9篇｜使用依赖注入构建Agent**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/jaAEN8_KGxd3oJa4BMlk1A)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/09-DI`

**第10篇｜将Agent暴露为mcp工具供第三方安全调用**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/tDN2JzDDyhenEuW303ZAFg)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/10-AgentAsMcpTools`

**第11篇｜Agent Framework构建视觉Agent**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/IXaZF_ckQzYX_ONGLdYRxw)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/11-Vision-Agent`

**第12篇｜Agent Framework构建可组合的多agent系统**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/YJC64q8p4GErNrKL-hjWtA)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/12-Agent-As-Function-Tool`

**第13篇｜不阻塞、不等待：让Agent像后台服务一样持续运行**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/mawpOV42Cw7DURx3ew4nGg)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/13-Backgroud-Response-With-Tool-And-Persistence`

**第14篇｜Agent Framework中的 Middleware 设计：从 HTTP Pipeline 到 AI Agent Pipeline**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/jQJDwunLFtyUOsYgKiEAKg)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/14-Agent-Middleware`

**第15篇｜Agent Framework中 IChatReducer 进行聊天记录缩减**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/k0Vz0AvV3CkI1Prvs-KdnQ)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/15-ChatReduction`

**第16篇｜如何用 Plugins 和依赖注入为 AI Agent 装上外挂**  
	- 📄 [文章地址](...)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/16-Plugins`

**第17篇｜Agent Framework中构建声明式（Declarative）AI Agent**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/vb26P6ArE52zYukspqGmmQ)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/17-Declarative-Agent`

**第18篇｜Microsoft Agent Framework 集成 MCP：基于 STDIO 的工具接入**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/g2NP1EOGqKCcN9XIPhl51w)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/18-Agent-MCP-Server-Stdio`

**第19篇｜Agent-To-Agent协议**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/5m35kWf9vvPTyeEAbbRqWw)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/19-A2AProtocal`

**第20篇｜使用 Microsoft Foundry 实现持久化 Agents**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/mmeGOVJS5gYSKJRC_W8wmw)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/20-Persistent-Agent`

**第21篇｜使用Microsoft Agent Framework与Microsoft Foundry构建持久化 AI Agent（AIProjectClient）**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/srl9dxPh7crp-r8-BDuoIA)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/21-Persistent-Agent-AIProject`

**第22篇｜OpenAI API 调用模式对比：ChatCompletions vs Response API**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/JtKjx6Ok4HEdYcupvnj6rg)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/22-OpenAI-API-Patterns`

**第23篇｜Agent Framework 集成 GitHub Copilot SDK，实现 AI 自动操控你的电脑**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/3DW3GbICujuRtRFm4WWqtw)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/23-GitHubCopilotSDK`

**第24篇｜Agent Framework 接入 Ollama（本地模型实践记录）**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/H0LoWUKNnlYA7b-FMezqVA)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/24-Agent-On-Ollama`

**第25篇｜从 MCP 到 Skill：基于 FileBased Skill 与 Agent Framework 的实践探索**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/u5pwD_Qy_DoHtdDGQi55GQ)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/25-FileBased-Agent-Skills`

**第26篇｜基于 CodeDefined Skill 与 Agent Framework 的实践探索**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/R8ndIcS5QkpeDieyqUWg2Q)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/26-CodeDefined-Agent-Skills`

**第27篇｜基于 ClassBased Skill 与 Agent Framework 的实践探索**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/RMyjuBohYruW2j24iTw0Cg)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/27-ClassBased-Agent-Skills`

**第28篇｜基于 FileBased、CodeBased 和 ClassBased 组合 Skills 与 Agent Framework 的实践探索**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/ZJVMgolAI1BrXJqUEdju0g)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/28-Mixed-Agent-Skills`

**第29篇｜在 Agent Framework 中为 Agent Skill 接入依赖注入 DI**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/YFNpAZWb-Ojnea2tTlD-Kw)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/29-AgentSkill-Integration-DI`

**第30篇｜Agent Framework接入原生DeepSeek-v4-pro**  
	- 📄 [文章地址](https://mp.weixin.qq.com/s/P6kpgat5OOCLS-bc8PMaQw)  
	- 💻 GitHub源代码地址：`https://github.com/bingbing-gui/dotnet-agent-playbook/tree/master/src/ai-agent/Agent-Framework/30-Agent-Providers-DeepSeek`

