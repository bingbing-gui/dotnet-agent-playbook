

近年来，AI代理（Agent）的能力越来越强，通过调用外部函数驱动真实世界的操作成为主流。但AI调用函数如何做到安全、受控，防止“失控”或误操作？  
微软[Agent Framework]提供了极具参考价值的方案——**结合函数工具(Function Tool)与人工审批(Human-in-the-loop Approval)**。

---

## 1. 背景与价值

- **AI代理**已能自动理解、推理、决策，并通过函数接口驱动外部服务或系统。
- 但自动化背后存在风险，比如：泄漏敏感信息、误操作生产系统、未授权支付等。
- “人机协同”是现实场景最佳解法。**高风险操作，由AI发起、人工审批，保障灵活性与安全边界**。

---

## 2. 实现方式解析

下面的 C# 代码示例，展示了如何使用微软 Agent Framework 创建一个需要人工审批才能调用外部函数的 AI 代理。

```csharp
### 2.1 环境变量与模型部署

我们一如既往的使用之前我们配置好的环境变量

```csharp
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
```
> **解读**  
> 用于读取 Azure OpenAI 的API endpoint和部署的模型名，确保运行时环境配置正确。

---

### 2.2 定义函数工具

```csharp
[Description("获取指定国家的最新新闻标题。")]
static string GetNews([Description("国家名称。")] string country)
    => $"来自 {country} 的头条新闻：AI 正在革新软件开发领域。";
```
> **解读**  
> 固定逻辑的“获取新闻”函数，通过特性 `Description` 给AI以调用说明——AI可以基于描述自动决定何时调用。

---

### 2.3 构建带审批的 Agent

```csharp
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: "你是一个乐于助人的助手",
        tools: [new ApprovalRequiredAIFunction(AIFunctionFactory.Create(GetNews))]
    );
```
> **解读**  
> - 创建 AzureOpenAI 客户端，用本地 CLI 认证。
> - 将 `GetNews` 注册为工具，但**强制包裹为需要人工审批的工具**。即AI代理每请求调用此函数一次，必须人类审批一次。

---

### 2.4 对话及审批流程

```csharp
AgentThread thread = agent.GetNewThread();
var response = await agent.RunAsync("美国的最新新闻是什么？", thread);
var userInputRequests = response.UserInputRequests.ToList();
```
- 开始新对话，让代理处理“美国的最新新闻是什么？”问题。代理分析后会发现需要调用函数，但因设置了“审批”，需要请求人工同意。

```csharp
while (userInputRequests.Count > 0)
{
    var userInputResponses = userInputRequests
        .OfType<FunctionApprovalRequestContent>()
        .Select(functionApprovalRequest =>
        {
            Console.WriteLine($"代理想调用以下函数，请回复 Y 以批准：Name {functionApprovalRequest.FunctionCall.Name}");
            return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)]);
        })
        .ToList();

    response = await agent.RunAsync(userInputResponses, thread);
    userInputRequests = response.UserInputRequests.ToList();
}
```
> **解读**  
> - 发现有函数审批请求时，程序会打印出请求内容，并要求用户Y/N输入。  
> - 用户输入Y（同意）或N（拒绝），AI据此决定能否调用函数，再给出正式回复。
> - 以循环结构保证多轮函数审批顺利完成。

---

### 2.5 最后输出AI代理回复内容

```csharp
Console.WriteLine($"\nAgent: {response}");
```
> **解读**  
> 输出经审批流程后的AI完整回复。

---

### 2.6 扩展：流式响应与持久化

代码中预留了“流式调用”的 Demo（即对话过程中可以一边计算一边同步展示结果），同时建议结合“历史会话持久化”，以适应前后端解耦与远程用户场景。

---

## 3. 场景与优势

- **企业内控**：比如，AI可以初步处理报销、审批等流程，但遇到高额度或敏感操作，必须人工再审确认。
- **外部系统访问**：调用日志、变更配置、跨系统调度时，敏感接口自动卡口，确保AI决策有人兜底。
- **AI安全合规**：面向金融、医疗等监管行业，满足自动化与合规“双高”诉求。

---

## 4. 总结

微软Agent Framework通过**ApprovalRequiredAIFunction包装外部函数，结合AI代理与人类审批，实现了强AI控制力的业务模式**。这种方案极具普适性，尤其适合“AI终审交付业务结果”但又“必须人工安全兜底”的企业和行业应用场景。

基于上述开发模式，AI代理可大幅释放“自动化”红利，又可保障企业核心资产和操作安全。

