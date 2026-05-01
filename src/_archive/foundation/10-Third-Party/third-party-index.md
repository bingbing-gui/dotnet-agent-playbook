# 📦 第三方库集成指南（ASP.NET Core）

本页列出本项目中使用到的优秀第三方 NuGet 包，并说明它们在 ASP.NET Core 项目中的用途、集成方式及参考资料。

---
## 📌 Sqids - 简洁可逆的短 ID 生成器

> 用于生成可逆、非连续、无敏感信息泄露的短 ID，适合替代数据库自增 ID 在 URL 中暴露。

### 📚 基本信息

- **NuGet 包名**：[Sqids](https://www.nuget.org/packages/Sqids)
- **GitHub 地址**：[https://github.com/sqids/sqids-dotnet](https://github.com/sqids/sqids-dotnet)
- **官方教程**：[https://sqids.org/dotnet](https://sqids.org/dotnet)
- **本地示例代码**：`src/10-Third-Party/Sqids/`
- **相关技术文章**：[《ASP.NET Core 中使用 Sqids 实现 url 短链接编码》](https://mp.weixin.qq.com/s/sqids-demo-link)

### 🧩 典型应用场景

- 用于替代数据库中递增的 ID，避免直接暴露真实主键
- 构建短链接系统或带 ID 的安全跳转链接
- 用户邀请码、资源隐藏编码等业务场景

### ⚙️ ASP.NET Core 集成方式

```csharp
// 注册 Sqids 编码器（以 int 类型为例）
builder.Services.AddSingleton<SqidsEncoder<int>>();

// 使用方式
var encoder = serviceProvider.GetRequiredService<SqidsEncoder<int>>();
string encoded = encoder.Encode(123);     // -> "jR"
int[] decoded = encoder.Decode("jR");     // -> [123]

```
