# LocalCodeActProvider 本地示例

这是一个中文控制台示例：模型编写 Python，通过 `execute_code` 运行代码，再使用
`call_tool` 查询模拟用户和商品数据、执行计算。示例数据不包含真实业务信息。

**安全提示：LocalCodeAct 不是安全沙箱。** Python 子进程在本机运行，可能访问本机
文件或网络。运行前请使用容器、虚拟机等隔离环境；程序会显示生成的代码，只有输入
`Y` 才会执行。超时限制和代码校验不能代替隔离。

## 运行准备

1. 安装 .NET 11 和 Python 3，确保 `python`（Windows）或 `python3`（其他平台）在
   PATH 中。也可以通过 `LOCAL_CODEACT_PYTHON` 指定 Python 可执行文件的完整路径。
2. 准备可调用的 Foundry 项目和模型部署，并使用 Azure CLI 登录或配置其他
   `DefaultAzureCredential` 支持的身份。
3. 在 PowerShell 中设置项目终结点和模型部署名称：

   ```powershell
   $env:FOUNDRY_PROJECT_ENDPOINT = "https://<资源名>.services.ai.azure.com/api/projects/<项目名>"
   $env:FOUNDRY_MODEL = "<模型部署名称>"
   # 如有需要：
   # $env:LOCAL_CODEACT_PYTHON = "C:\Python312\python.exe"
   ```

4. 在本项目目录运行：

   ```powershell
   dotnet run
   ```

程序会请求模型分析模拟数据，并在执行 Python 前展示代码。检查后输入 `Y` 批准；
输入其他内容或直接回车则退出。正常结果应包含商品总价 5100、平均价 2550，
以及管理员张伟和王芳。

示例不需要安装第三方 Python 包。生成的代码已在异步环境中运行，应直接使用
`await call_tool(...)`，不要调用 `asyncio.run(...)`；用 `print` 输出结果，仅给
`result` 赋值不会输出。若 Python 执行失败，终端会显示错误，模型可能请求重新生成代码。

项目使用预览版 Agent Framework 包；调用模型需要联网，可能产生费用。
