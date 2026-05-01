import { spawn } from "child_process";

// Unicode 解码函数（必须有）
function decodeUnicode(str) {
    return JSON.parse(`"${str}"`);
}

// ⚠改成你自己的 MCP Server 项目路径
const MCP_PROJECT_PATH = "E:/Repos/aspnetcore-developer/aspnetcore-developer/src/09-AI-Agent/Agent-Framework/10-AgentAsMcpTools";

// 启动 dotnet MCP Server
const server = spawn(
    "dotnet",
    ["run", "--project", MCP_PROJECT_PATH],
    { stdio: ["pipe", "pipe", "pipe"] }
);

// ===== 监听 MCP STDOUT =====
server.stdout.on("data", (data) => {
    console.log("⬅ MCP STDOUT:");
    const text = data.toString().trim();
    let msg;

    try {
        msg = JSON.parse(text);
    } catch {
        console.log(text);
        return;
    }
    // 只对 tools/list 的 description 解码
    if (msg?.result?.tools) {
        msg.result.tools.forEach(tool => {
            if (typeof tool.description === "string") {
                tool.description = decodeUnicode(tool.description);
            }
        });
    }
    console.dir(msg, { depth: null });
});
// ===== STDERR =====
server.stderr.on("data", (data) => {
    console.error("⚠ MCP STDERR:");
    console.error(data.toString());
});

server.on("exit", (code) => {
    console.error(`❌ MCP Server exited with code ${code}`);
});

// ===== MCP 协议交互 =====
function send(msg) {
    server.stdin.write(JSON.stringify(msg) + "\n");
}
// tools/list
setTimeout(() => {
    console.log("➡ Sending tools/list");
    send({
        id: 1,
        jsonrpc: "2.0",
        method: "tools/list"
    });
}, 2000);

// 调用 Joker
setTimeout(() => {
    console.log("➡ Sending tools/call");
    send({
        id: 2,
        jsonrpc: "2.0",
        method: "tools/call",
        params: {
            name: "Joker",
            arguments: {
                query: "讲一个江湖笑话"
            }
        }
    });
}, 3000);

