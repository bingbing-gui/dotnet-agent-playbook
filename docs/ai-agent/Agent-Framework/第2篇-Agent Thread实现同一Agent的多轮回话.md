微软开源的agent-framework 以简洁方式帮助构建具备多轮对话能力的智能 Agent。我们一如既往的沿用上一节中我们的基础配置。如果你没有看上一节，请转到上一节

---

## 一、简化多轮对话

1. **创建带上下文记忆的 Agent**  
   利用微软 agent-framework，结合 Azure OpenAI 服务，创建了一个能够记忆对话上下文的智能 Agent。每次回复都基于上一次的内容，真正实现“多轮对话”，让 Agent 能够理解和跟进用户的上下文需求。

2. **自定义 Agent 个性与功能**  
   代码中通过 instructions（角色设定）给 Agent 加入了个性：“你擅长讲笑话”，让 AI 每次回答都能契合这个人设，体验丝滑的定制化智能服务。

3. **多轮对话与上下文 Thread**  
   利用 `AgentThread` 对象维护对话上下文，将每轮交互加入 thread，实现连续多轮交谈，比如先让 AI 讲一个海盗笑话，再要求加入表情并模仿鹦鹉风格，AI 都能准确响应。

4. **流式输出体验**  
   示例还展示了多轮对话的流式输出方式（Streaming），更适合输出长文本或逐步构建回复，让用户实时看到生成过程，带来更好的交互体验。


---

## 二、代码示例

创建一个 Console 应用项目，并添加以下 NuGet 包：

```bash
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Agents.AI.OpenAI
dotnet add package Microsoft.Extensions.AI.OpenAI
dotnet add package Azure.Identity
```

```csharp
// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with a multi-turn conversation.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");


AgentThread thread = agent.GetNewThread();
Console.WriteLine(await agent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", thread));
Console.WriteLine(await agent.RunAsync("现在把这个段子加上一些表情符号，并用说书人的语气再讲一遍。", thread));

// Invoke the agent with a multi-turn conversation and streaming, where the context is preserved in the thread object.
thread = agent.GetNewThread();

Console.WriteLine(await agent.RunAsync("再讲一个关于江湖侠客的小笑话，要幽默一点。", thread));
Console.WriteLine(await agent.RunAsync("给这个江湖侠客的小笑话加些表情符号，再添加点夸张的江湖腔。", thread));

//await foreach (var update in agent("再讲一个关于江湖侠客的小笑话，要幽默一点。", thread))
//{
//    Console.WriteLine(update);
//}
//await foreach (var update in agent.RunStreamingAsync("给这个江湖侠客的小笑话加些表情符号，再添加点夸张的江湖腔。", thread))
//{
//    Console.WriteLine(update);
//}
```

```csharp
话说啊，这江湖上最大的事儿，归根结底就是两件儿——喝茶和扯皮。要说这茶馆儿，嘿，那可是江湖的大本营，高手云集，故事多得能绕八个城墙。

有一天啊，青城派的老张头、武当派的小王，还有一个闲云野鹤的卖糖葫芦的老葛头，仨人凑一起喝茶。老张头拍着桌子就吹牛：“我们青城派啊，那可是讲究内功，这内功练好了，能隔空点火，炉子都不用带，自备火锅随时吃！”

小王一听不服气：“屁啊！我武当派练的是太极，能借力打力！你别说火锅，你那锅里的肉片落下去，我就能用一根筷子给它弹回来，刀工都不用练，直接薄成比翅！”

卖糖葫芦的老葛头在边上听了半天，嗞溜一口茶，悠悠地来一句：“别吹了，你们这功夫学得再好，还不是得请我贴糖葫芦搁锅里热一热？”

俩人被怼得哑口无言，但是为了面子，觉得不能输！老张头愤怒地把茶碗一放：“伙计，来壶茶！”

伙计过来壶，手一抖，茶水全洒了。小王瞪眼就骂：“怎么回事？！这武功不行啊！”

伙计搓搓手，叹气道：“不好意思啊，一听你们聊功夫吓的，怕我这洒水派入不了流啊。”

全场顿时哄堂大笑，这就是江湖嘛，吹牛为主，动手为辅，嗓门最大才是真高手！
好嘞！客官您稍坐，听我慢慢道来～ 📜🍵

咱说这江湖啊，那不外乎两件事儿：**喝茶，扯皮**！这茶馆喽，嘿，那可是不打眼的地方，却是江湖大侠的聚集地，高手云集，热闹得跟过年似的！🎭

有一日，青城派的老张头、武当派的小王，再加上一位闲云野鹤的——呃，干啥的？卖糖葫芦的老葛头，仨人凑一桌，边喝茶边吹牛🍵🐂。

老张头一拍桌子，差点把壶震翻了，张嘴就来：“我们青城派的内功，那叫一个绝！💪 隔空点火了解一下！炉子都不用带，随随便便自己点火——呲啦一声，吃火锅直接开干！”🔥🍲

小王一听这话，哪能忍得住？站起身来了个武当派的手势：“你这算啥！我们武当，可是真正借力打力的巅峰！✨ 那肉片儿往锅里一掉，我一根筷子就能直接把它给弹回锅沿上！刀工？还能用脚趾写个‘服’字呢！”🥢🐂

这时候，旁边的卖糖葫芦的老葛很冷静，吃着自己的糖葫芦，喝了一口茶，悠悠地搭了句：“嘁～你们这两派吵啥呢？再有功夫，不还是得买我糖葫芦给肉片糖一糖蘸点甜味儿才香！”🍡😏

哎哟，这话一出，老张头和小王直接当场哑火了！但又觉得输了面子不爽，正要再怼回去，这时——伙计端茶过来了！🍵💨

结果呢！不知伙计是太激动，还是这茶壶里有暗劲儿，您猜怎么着？那茶壶一抖，茶水全！洒！了！💦🤦‍♂️

小王瞪圆眼睛就开骂：“哎！你个茶水工！没练过轻功啊！洒一桌！你这是武功不行呗？！”😡

那伙计搓搓手，低头一脸无奈：“哎，二位大侠别怪啊……小的我啊，这不怕了吗！👀 看您几个聊功夫，怕露馅儿，咱这**洒水派**估计……入不了流吧！”～ 😅🤷‍♂️

哎呀～全场居然噗的一声全乐了！哈哈哈哈！ 😂 不愧是江湖，这高手过招“吹牛为主，动手为辅”，嗓门最大，才是天下第一啊！🎤👊
好嘞！听好了，这可是我的绝活——

话说明月楼上，有一位大侠，名叫李不凡，轻功了得，刀法通神，那可真是人敬鬼怕。但他有个毛病，天生路痴，这件事在江湖上人尽皆知。

有一天，一个小镇闹山贼，于是乡亲们赶紧托人去请李不凡来帮忙。李大侠听了二话不说，一扬手，豪情万丈：“放心！小事一桩，先给我准备庆功酒，我半个时辰内就赶到！”

结果，半个时辰过去了，大侠没到；一个时辰过去了，还没到；直到天黑了，乡亲们一边喝着庆功酒，一边还没见人影。

第二天大中午，李不凡终于风尘仆仆地翻墙而入，气喘吁吁地喊：“山贼呢？！快带我去剿！”

乡亲们一头雾水：“李大侠，那山贼昨天就被隔壁镇另一位大侠打跑了，您怎么才到啊？”

李不凡脸一红，尬笑道：“这……我这不练轻功绕着走的吗？结果不小心跑了三圈，又走岔到你们隔壁镇去了！”

乡亲们忍不住扶额，而隔壁镇送信的伙计在一边弱弱地说：“嘿，我说的嘛，昨天怎么看到您飞檐走壁，跟那山贼擦肩而过，当时还以为您是故意放他们一马呢！”

这下，李不凡不好意思地挠了挠头，只能感慨：“哎，江湖路远，真是路痴的万丈深渊啊！”

怎么样，是不是觉着，连大侠都有难解的“江湖忧”？别笑岔气了！
哈哈！好嘞，那我再来给这个笑话加点江湖味儿，还送表情符号一波，全场注意笑点！

---

话说呀，江湖响当当的大侠、轻功飞天的刀王——李不凡🌪️，今天又被乡亲们请下山啦！这位李大侠，刀起刀落如秋风扫落叶🍂，动作潇洒犹如仙人踏云，看上去简直无敌，但偏偏有一桩“盖世烦恼”——天生认路不过关😵‍💫，活生生就是活地图里的漏网之鱼🤣。

事情是这样的👇：

这一天，镇上闹山贼了⚔️！乡亲们鸡飞狗跳，村长慌忙跪求：“李大侠啊，高抬贵脚救救我们吧！山贼要劫咱米缸啦🍚！”

李不凡听了，豪气冲天，拍拍刀鞘大喊：“小事小事，刀在手，无敌是最寂寞💪！乡亲们，看着吧——我半个时辰内到，先备庆功酒🍶！”

然而啊，半个时辰过去了，人呢？？？一个时辰过去了，那人……还是没来👀。他们等到天都黑了，只好喝着庆功酒，边喝边等。

到了第二天的晌午🍵，李不凡才突然从院墙上翻进来，衣服还沾着草叶，脸皮厚得像猪头，气喘吁吁地喊：“山贼呢？快快带我剿了！🏃‍♂️”

乡亲们愣住了：“李大侠，情况报告一下，那山贼昨天就被隔壁镇的刀王剿跑了……您咋才到啊？”

李不凡一听，顿时脸色一红，头上“金汗如雨🌞”，挠头尴尬笑道：“哈哈哈哈哈……这……我练轻功飞天走墙嘛，结果绕了三圈，跑到隔壁镇去复健了🤦‍♂️。”

旁边隔壁镇的送信小哥忍不住接了句：“李大侠，我昨天还看您跟山贼擦肩而过呐！那一刻我脑补您是要放他们一码，真是‘侠肝义胆’，没想到您迷路迷上天……😮‍💨”

这下，乡亲们是真扶额扶到天灵盖了！李大侠的脸，比城门还红，摇头感叹：“唉！这江湖路远，山高水长，可怜我这路痴，走不出江湖万丈深渊啊！🌄”

---

哈哈哈，各位，当大侠也是不容易，特别是走路靠命运，一出门就开启《导航失灵版江湖》！如何，看完是不是跟吃了辣椒一样——笑的吼辣辣🔥！

E:\Repos\aspnetcore-developer\aspnetcore-developer\src\09-AI-Agent\Agent-Framework\MultiturnConversation\bin\Debug\net10.0\MultiturnConversation.exe (process 15892) exited with code 0 (0x0).
To automatically close the console when debugging stops, enable Tools->Options->Debugging->Automatically close the console when debugging stops.
Press any key to close this window . . .
```
要点：

- `AgentThread` 负责上下文关联。
- `RunAsync` 返回完整结果。
- `RunStreamingAsync` 提供逐步生成的流式体验。

---

## 三、从示例可以学到什么

1. 上下文线程 Thread 非常关键, 框架通过 Thread 封装了上下文管理，减轻了开发者负担。
2. Agent 角色设定简单直接, 只需一段说明文字，就能让 Agent 带上特定风格。
3. 流式输出易于集成，使用 RunStreamingAsync 即可获取实时生成内容。
4. 适用场景广泛：聊天助手、客服问答、教学互动、内容生成、工具型界面等。

---

## 四、结语

agent-framework 将“对话、记忆、角色”封装得轻量，便于快速构建多轮对话应用。示例代码简洁易读，适合入门与扩展。建议直接运行官方示例加深理解。 
