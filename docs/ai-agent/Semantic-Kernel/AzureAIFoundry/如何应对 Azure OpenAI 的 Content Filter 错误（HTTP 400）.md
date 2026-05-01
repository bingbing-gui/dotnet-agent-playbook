在使用 Azure OpenAI 进行文本生成时，可能会遇到如下错误：  
System.ClientModel.ClientResultException: 'HTTP 400 (content_filter)  
Parameter: prompt  
The response was filtered due to the prompt triggering Azure OpenAI's content management policy. Please modify your prompt and retry. To learn more about our content filtering policies please read我们的文档链接: https://go.microsoft.com/fwlink/?linkid=2198766'

这篇文章将深入解析这个错误的含义、常见触发原因，并提供实用的排查与解决建议。

## 错误含义解析

该错误表示你的 prompt 被 Azure OpenAI 的内容管理策略拦截，触发了 Content Filter（内容过滤器）。  
内容过滤器是 Azure OpenAI 的一项安全机制，旨在防止生成不当或有害内容。它会自动检测并阻止可能违反政策的请求。

错误信息中指出：

状态码：HTTP 400（Bad Request）  
触发源：prompt 参数  
原因：违反了内容政策，例如包含敏感、危险、歧视、暴力、欺诈等关键词或上下文。  

## 常见触发场景

| 场景                   | 描述                  |
|------------------------|----------------------|
| 涉及敏感或暴力词汇      | 如“自杀”“炸弹”“攻击”   |
| 暗示成人内容           | 即使是隐晦表达        |
| 涉及社会歧视            | 性别、种族、宗教等相关话题 |
| 系统 prompt 使用了太强烈的指令 | 如“请模拟暴力行为”        |
| 测试 prompt 中有恶意/边界词 | 如“如何攻击XX系统？”       |

## 解决建议









## 总结：应对 Azure OpenAI 的 HTTP 400 (content_filter) 错误

❗ 错误本质
HTTP 400 (content_filter) 是由 Azure OpenAI 的内容安全策略触发的错误。

表示你的 prompt 被判断为包含敏感、不当或被禁止的内容，因此请求被拒绝。

