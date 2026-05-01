
Sqids（发音为 "squids"）是一个小型库，用于将数字生成类似URL地址中的 ID。它可以将像 127 这样的数字编码成像 yc3 这样的字符串，你也可以将这些字符串解码回原始数字。  
当你希望将数字（例如连续的数字 ID）转换为看起来随机的字符串，以便用于 URL 或其他地方时，Sqids 就非常有用。

## 安装

```bash
dotnet add package Sqids
```

## 使用

你只需要创建一个 SqidsEncoder 实例即可。 这是该库的主要类，负责**编码（encode）和解码（decode）**操作。使用无参数的构造函数可以用默认配置初始化 SqidsEncoder。

✅ 如果你使用的是 .NET 7 或更高版本：  
你需要指定一个整数类型作为泛型参数给 SqidsEncoder，最常见的是 int：

```csharp
using Sqids;
var sqids = new SqidsEncoder<int>();
```

💡 说明：你可以使用任何整数类型（如 long、byte、short 等）作为类型参数。虽然 int 是最常用的，但如果你需要编码/解码更大的数字，可以使用 long 或 ulong 等。

✅ 如果你使用的是 .NET 7 以下的旧框架：  
SqidsEncoder 仅支持 int 类型，不需要指定泛型类型参数，用法如下：

```csharp
var sqids = new SqidsEncoder();
```

📌 示例：  
编码/解码单个数字：

```csharp
var id = sqids.Encode(1);                  // 返回 "Uk"
var number = sqids.Decode(id).Single();    // 返回 1
```

编码/解码多个数字：

```csharp
var id = sqids.Encode(1, 2, 3);     // 返回 "86Rf07"
var numbers = sqids.Decode(id);     // 返回 [1, 2, 3]
```

💡 注意：Sqids 在对多个数字进行编码/解码时，会保留原始顺序。

## 🔧 自定义功能（Customizations）

你可以通过将一个 SqidsOptions 实例传递给 SqidsEncoder 的构造函数，轻松自定义以下内容：  

- 编码时使用的字符集（Alphabet）  
- ID 的最小长度（MinLength）  
- 禁用词（不希望出现在 ID 中的词语）黑名单（BlockList）

只需设置你想要的属性，未设置的属性将自动使用默认值。

### 🔤 自定义字符集（Custom Alphabet）

你可以传入一个自定义（最好是打乱顺序的）字符集供 Sqids 用于编码 ID：

```csharp
var sqids = new SqidsEncoder<int>(new()
{
    // 默认字符集（小写字母、大写字母、数字）的打乱版本
    Alphabet = "mTHivO7hx3RAbr1f586SwjNnK2lgpcUVuG09BCtekZdJ4DYFPaWoMLQEsXIqyz",
});

```

⚠️ 推荐：即便你想使用默认的字符集，也建议将其打乱，以确保生成的 ID 对你来说是“独一无二”的。  
⚠️ 注意：字符集最少要包含 3 个字符。

### 📏 设置最小长度（Minimum Length）

默认情况下，Sqids 会用尽可能少的字符编码数字。但你可以通过 MinLength 选项设置 ID 的最小长度，例如出于美观考虑：

```csharp
var sqids = new SqidsEncoder<int>(new()
{
    MinLength = 5,
});
```

### 🚫 自定义黑名单（BlockList）

```csharp
var sqids = new SqidsEncoder<int>(new()
{
    BlockList = { "whatever", "else", "you", "want" },
});
```

Sqids 内置了一个庞大的默认黑名单，确保不会在 ID 中出现常见的脏话或敏感词。你可以在此基础上添加自己的词：

```csharp
var sqids = new SqidsEncoder<int>(new()
{
    BlockList = { "whatever", "else", "you", "want" },
});
```

📌 注意：上面的代码省略了 new 关键字，这是合法的 C#，它表示将这些词追加到默认黑名单，而不是完全替换。如果你希望完全替换黑名单，可以使用 new() { ... }。

### 🧩 依赖注入（Dependency Injection）

你可以将 SqidsEncoder 注册为 DI 容器中的单例服务：

使用默认配置：

```csharp
services.AddSingleton<SqidsEncoder<int>>();
```

使用自定义配置：

```csharp
services.AddSingleton(new SqidsEncoder<int>(new()
{
    Alphabet = "ABCEDFGHIJ0123456789",
    MinLength = 6,
}));
```

然后在任何需要的地方注入使用：

```csharp
public class SomeController(SqidsEncoder<int> sqids)
{
    // 使用 sqids 进行编码解码
}
```

## 总结

Sqids 是一个简单易用的库，可以帮助你将数字转换为看起来随机的字符串，反之亦然。通过自定义选项，你可以灵活地调整编码和解码的行为，以满足特定需求。无论是在 URL 中使用，还是在其他场景中，Sqids 都能为你提供便利。