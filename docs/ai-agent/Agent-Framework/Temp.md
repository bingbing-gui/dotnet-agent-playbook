# DeepSeek Harness 快速部署与初体验

DeepSeek 最近发布了自己的 Harness。简单来说，它提供了一套可以直接在本地运行的 Agent 工作环境，除了可以使用 DeepSeek 自家的模型之外，还支持自定义模型、插件以及不同级别的目录访问权限。

这篇文章主要带大家快速把 DeepSeek Harness 跑起来，并简单看一下它目前提供了哪些能力。

## 安装 Node.js

DeepSeek Harness 依赖 Node.js，所以安装之前先检查一下本机是否已经安装 Node.js。

打开命令行窗口(如果是 Windows 系统，推荐用 Windows Terminal)，执行：

```bash
node -v
```

如果能够正常输出版本号，说明 Node.js 已经安装，可以直接进入下一步。

如果提示找不到 `node` 命令，可以前往 Node.js 官网下载安装：

```text
https://nodejs.org/zh-cn/download
```

对于 Windows 用户来说，基本一路点击“下一步”即可完成安装。

安装完成以后，再次执行：

```bash
node -v
```
能够看到版本号，就说明 Node.js 已经准备好了。

## 安装 DeepSeek Harness

确认 Node.js 没问题以后，就可以开始安装 DeepSeek Harness。

打开命令行窗口，执行 DeepSeek Harness 官方提供的安装命令。

```bash
npx @deepseek-ai/dsh web 
```

安装完成以后启动 Harness。

默认情况下，可以在浏览器中打开：

```text
http://127.0.0.1:3080
```

如果页面能够正常打开，就说明 DeepSeek Harness 已经运行起来了。



## 初识 DeepSeek Harness

第一次打开 DeepSeek Harness，整体界面并不复杂，大致可以分成左右两个区域。

### 左侧工作区

左侧最重要的是工作区域。

这里的工作区，本质上就是你电脑上的一个本地目录。

比如我们选择`D:\Projects\demo`, 那么 Agent 后续能够读取或者修改的内容，基本都会围绕这个目录展开。

同一个工作目录下面还可以创建多个不同的对话，所以它并不是“一个目录只能对应一个聊天”，而更像是：
这样做的好处是，同一个项目可以针对不同任务分别创建会话，互相之间不会混在一起。

### 设置

进入设置以后，可以看到 DeepSeek Harness 提供了一些比较重要的配置能力。

1. **通用设置**

   表示 DeepSeek Harness 的一些通用配置，比如语言、主题等。

2. **模型**

   这个主要是给我开发第三方的模型使用的。DeepSeek Harness 目前提供了 DeepSeek 自家的模型，也可以通过自定义模型来接入其他模型。

3. **插件**

   DeepSeek Harness 一个比较明显的设计思路就是插件化。

   官方给出的口号可以概括成：

   > 一切皆插件。

   Harness 本身提供一个 Agent 运行框架，很多具体能力都可以通过插件继续扩展。

   比如后面完全可以围绕自己的开发流程增加：

   这样 Harness 就不只是一个聊天工具，而可以逐渐变成适合自己团队的 Agent 工作环境。

4. **Agent 预设**

   Harness 里面还有一个比较有意思的设计，就是 Agent Preset，也就是 Agent 预设。

   目前内置了几种不同模式。

   **标准模式**

   标准模式是默认使用的模式，也是功能最完整的一种。对于日常开发来说，直接使用标准模式基本就够了。

   **PTC 模式**

   PTC 模式在标准模式能力的基础上，又加入了 Code Mode SDK。

   模型可以通过 TypeScript 程序组合多步操作。

   **极简模式**

   极简模式提供的能力非常少。这种模式比较适合简单编码任务，也可以减少 Agent 可用工具太多带来的干扰。

   **创造模式**

   创造模式主要用来创建新的 Agent Preset。

   它除了拥有标准模式的能力之外，还提供了：

   如果后面想自己定制一套 Harness Agent，可以从这个模式开始。


## 右侧交互区域

右侧就是主要的 Agent 工作区域。

下半部分是输入框，我们可以在这里给 Agent 输入任务。

上半部分则用于显示 Agent 的执行过程和最终输出结果。

整体使用方式和现在常见的 Coding Agent 比较接近：


## 模型选择

在输入区域中可以切换模型。

默认可以看到`DeepSeek-V4-Flash`、`DeepSeek-V4-Pro`两个模型。

当然，Harness 还支持自定义模型，所以实际使用时并不限于默认提供的这几个模型。

### 目录访问权限

输入框左侧还有一个很重要的配置，就是目录访问权限。

目前可以看到三种模式：`Read Only`、`Workspace Write` 和 `Full Access`。

**Read Only**

只读模式。

Agent 可以读取工作区中的文件，但是不能修改文件。

如果只是希望 Agent 帮忙分析项目，而不希望它直接修改任何内容，使用这个模式比较合适。

**Workspace Write**

允许 Agent 修改当前 Workspace 中的内容。

这应该也是日常开发中最常用的一种模式。

**Full Access**

Full Access 是权限最高的模式。

开启之后，Agent 能够获得更大的操作范围，执行过程中需要用户手动确认的步骤也会明显减少。

## 总结

整体体验下来，DeepSeek Harness 给我的第一感觉还是不错的。

界面比较简单，上手成本不高，Workspace、模型、权限、Preset 和插件这些核心能力也都已经具备。

尤其是插件化这一点比较值得关注。

DeepSeek Harness 现在还处于比较早期的阶段，但至少从目前的设计方向来看，已经不只是“又一个 AI 编程聊天窗口”这么简单了。

后面如果插件生态能够继续丰富起来，还是很值得期待的。
