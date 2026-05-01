# 第6章-Semantic-Kernel运行提示词

---

随着大语言模型（LLM）逐渐成为智能应用的核心，**如何与模型进行高质量的“对话”设计**，成为每位开发者必须掌握的能力。Microsoft 的 Semantic Kernel（简称 SK）为我们提供了一整套**提示词系统（Prompt System）**，包括：

- ✍️ 提示词编写技巧  
- 🧩 模板化提示词管理（支持 Handlebars）  
- 🧠 聊天上下文记录（ChatHistory）

今天我们就来带大家深入理解这些关键能力，并结合代码实战，掌握构建智能对话应用的基础框架。

---

## ✍️ 一、提示词（Prompt）：让 AI 明白你的意图

在 Semantic Kernel 中，Prompt 是和大语言模型交流的方式。写得好，AI 懂你；写不好，AI 胡说。

### 常见提示词策略：


| 策略名               | 说明               | 文章                                                             |
|----------------------|--------------------|------------------------------------------------------------------|
| 🟢 Zero-shot         | 仅提供任务描述     | [零样本提示词和少良样本提示词](https://mp.weixin.qq.com/s/2UQ3OyLTs3HuRvEqlg9rRw)                               |
| 🔵 Few-shot          | 提供 1~5 个示例    | [零样本提示词和少良样本提示词](https://mp.weixin.qq.com/s/2UQ3OyLTs3HuRvEqlg9rRw)                                |
| 🟡 Priming Prompting | 设定 AI 身份       | [提示词诱导](https://mp.weixin.qq.com/s/r-kemVIW13UkVcTbPQkx2A)                        |
| 🔴 Chain-of-thought  | 引导逐步推理       | [思维链提示](https://mp.weixin.qq.com/s/CjCsC97aT2BgYe3YxuJiHg)                          |

---

## 🧩 二、提示词模板（Prompt Template）：构建可复用的智能模块

Prompt Template 就像“提示词函数”，不仅可以插入变量，还能调用其他 AI 函数、集成结构化模板，方便维护与组合。

### Template 有两种语法风格：

| 类型               | 特点                                  | 示例                                                              |
|--------------------|---------------------------------------|-------------------------------------------------------------------|
| ✅ 原生模板语法     | 使用 `{{$变量}}` 和 `{{函数}}` 语法    | `{{$city}} 天气是 {{weather.getForecast $city}}`                  |
| ✅ Handlebars 语法 | 类似前端模板引擎，结构强              | `{{request}}，{{#if image}}<image>{{image}}</image>{{/if}}`       |


### Handlebars YAML 示例：

```yaml
name: image_prompt
template_format: handlebars
template: |
    <message>
        <text>{{description}}</text>
    </message>
input_variables:
    - name: description
        is_required: true
```

```csharp
var yaml = EmbeddedResource.Read("image_prompt.yaml");
var function = kernel.CreateFunctionFromPromptYaml(yaml, new HandlebarsPromptTemplateFactory());
```

---

## 🧠 三、ChatHistory：打造有记忆的 AI 对话助手

真正“聪明”的对话系统，必须记得前面说过什么。ChatHistory 是 Semantic Kernel 中用于记录上下文的关键类。

```csharp
ChatHistory chat = [];
chat.AddSystemMessage("你是一个乐于助人的助手。");
chat.AddUserMessage("现在可以点些什么？");
chat.AddAssistantMessage("我们提供披萨、意大利面和沙拉。");

```

支持图文消息、作者信息：

```csharp
chat.Add(new()
{
        Role = AuthorRole.User,
        AuthorName = "Alice", // 用户名
        Items = [
                new TextContent { Text = "菜单上有哪些？" },
                new ImageContent { Uri = new Uri("https://example.com/menu.jpg") } // 菜单图片链接
        ]
});

```

---

## 🧩 四、三者协同：构建完整智能对话流程

✅ Prompt 负责告诉 AI “做什么”  
✅ Template 让提示词结构清晰、可复用  
✅ ChatHistory 让 AI 记住过去，理解上下文

这三者组合起来，就是打造语义感知、连续对话、可维护 AI 系统的核心架构。

---

## 🛠️ 五、语义内核提示模板

```csharp
 var skTemplateFactory = new KernelPromptTemplateFactory();
 var skPromptTemplate = skTemplateFactory.Create(new PromptTemplateConfig(
     """
     你是一名乐于助人的职业顾问。请根据用户的技能和兴趣，推荐最多 5 个合适的职位角色。
     请以如下 JSON 格式返回内容：
     "职位推荐":
     {
     "recommendedRoles": [],          // 推荐的职位
     "industries": [],                // 所属行业
     "estimatedSalaryRange": ""       // 预计薪资范围
     }
     我的技能包括：{{$skills}}。我的兴趣包括：{{$interests}}。根据这些，哪些职位适合我？
     """
 ));
 // 渲染提示模板并传入参数
 var skRenderedPrompt = await skPromptTemplate.RenderAsync(
    _kernel,
    new KernelArguments
    {
        ["skills"] = skills,
        ["interests"] = interests
    }
);
```

----

## 🛠️ 六、Handlebars提示词模板

```csharp
var roleMatch = Regex.Match(message, @"角色[:：](.*?)([;；]|$)");
var skillMatch = Regex.Match(message, @"技能[:：](.*)");
string roles = roleMatch.Success ? roleMatch.Groups[1].Value.Trim() : "";
string skill = skillMatch.Success ? skillMatch.Groups[1].Value.Trim() : "";
var hbTemplateFactory = new HandlebarsPromptTemplateFactory();
var hbPromptTemplate = hbTemplateFactory.Create(new PromptTemplateConfig()
{
    TemplateFormat = "handlebars",
    Name = "MissingSkillsPrompt",
    Template = """
    <message role="system">
        指令：你是一名职业顾问。请分析用户当前技能与目标职位要求之间的技能差距。
    </message>         
    <message role="user">目标职位：{{targetRole}}</message>
    <message role="user">当前技能：{{currentSkills}}</message>
    <message role="assistant">
     “技能差距分析”：
     {
         "缺失技能": [],
         "建议学习的课程": [],
         "推荐的认证": []
     }
     </message>
 """
}
);

var hbRenderedPrompt = await hbPromptTemplate.RenderAsync(
_kernel,
new KernelArguments
{
    ["targetRole"] = roles,
    ["currentSkills"] = skill
});
```

----
### ✅ 总结一句话

使用 Semantic Kernel 的 Prompt + Template + ChatHistory 组合，你可以轻松构建真正智能、会“聊天”的 AI 应用。

不论是开发 AI 助手、客服机器人，还是个性化推荐系统，这一套设计都是值得借鉴的黄金架构。
