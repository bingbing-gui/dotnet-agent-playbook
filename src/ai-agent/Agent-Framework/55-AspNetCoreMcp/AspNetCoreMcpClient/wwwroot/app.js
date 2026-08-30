const elements = {
  endpoint: document.querySelector("#server-endpoint"),
  model: document.querySelector("#model-name"),
  serviceState: document.querySelector(".service-state"),
  serviceStateText: document.querySelector("#service-state-text"),
  weatherForm: document.querySelector("#weather-form"),
  city: document.querySelector("#city"),
  queryWeather: document.querySelector("#query-weather"),
  weatherOutput: document.querySelector("#weather-output"),
  weatherStatus: document.querySelector("#weather-status"),
  cityShortcuts: document.querySelectorAll(".city-chip"),
  chatForm: document.querySelector("#chat-form"),
  message: document.querySelector("#message"),
  sendMessage: document.querySelector("#send-message"),
  agentHint: document.querySelector("#agent-hint"),
  agentResponse: document.querySelector("#agent-response"),
  agentOutput: document.querySelector("#agent-output"),
  agentStatus: document.querySelector("#agent-status")
};

async function readResponse(response) {
  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? await response.json()
    : await response.text();

  if (!response.ok) {
    const message = typeof body === "object" && body?.detail
      ? body.detail
      : `请求失败（HTTP ${response.status}）`;
    throw new Error(message);
  }

  return body;
}

function updateWeatherStatus(message, state = "") {
  elements.weatherStatus.textContent = message;
  elements.weatherStatus.className = `status ${state}`.trim();
}

function extractToolText(result) {
  if (!Array.isArray(result.content)) {
    return "天气工具已完成调用，但未返回可显示的文本。";
  }

  const textParts = result.content
    .filter(item => item && typeof item.text === "string")
    .map(item => item.text.trim())
    .filter(Boolean);

  return textParts.join("\n") || "天气工具已完成调用，但未返回可显示的文本。";
}

async function loadConfiguration() {
  try {
    const response = await fetch("/api/mcp/configuration");
    const configuration = await readResponse(response);
    elements.endpoint.textContent = configuration.endpoint;
    elements.model.textContent = configuration.model;
    elements.serviceState.classList.add("ready");
    elements.serviceStateText.textContent = "客户端已就绪";

    if (configuration.foundryConfigured) {
      elements.agentHint.textContent = "Foundry 已配置，可使用自然语言问答";
      elements.message.disabled = false;
      elements.sendMessage.disabled = false;
    } else {
      elements.agentHint.textContent = "未配置 Foundry，当前仅保留聊天模式，请先完成 Foundry 配置";
      elements.message.disabled = true;
      elements.sendMessage.disabled = true;
    }
  } catch (error) {
    elements.endpoint.textContent = "配置读取失败";
    elements.model.textContent = "不可用";
    elements.serviceState.classList.add("error");
    elements.serviceStateText.textContent = "客户端配置异常";
    elements.weatherOutput.textContent = error.message;
    updateWeatherStatus("配置错误", "error");
  }
}

async function queryWeather(event) {
  event.preventDefault();
  const city = elements.city.value.trim();

  if (!city) {
    elements.city.focus();
    return;
  }

  elements.queryWeather.disabled = true;
  elements.weatherOutput.textContent = `正在通过 MCP Server 查询${city}天气…`;
  updateWeatherStatus("正在查询", "loading");

  try {
    const response = await fetch("/api/mcp/weather", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ city })
    });
    const result = await readResponse(response);
    elements.weatherOutput.textContent = extractToolText(result);
    updateWeatherStatus("查询成功", "success");
    elements.serviceState.classList.add("ready");
    elements.serviceState.classList.remove("error");
    elements.serviceStateText.textContent = "MCP Server 在线";
  } catch (error) {
    elements.weatherOutput.textContent = `查询失败：${error.message}`;
    updateWeatherStatus("查询失败", "error");
    elements.serviceState.classList.add("error");
    elements.serviceState.classList.remove("ready");
    elements.serviceStateText.textContent = "MCP Server 连接失败";
  } finally {
    elements.queryWeather.disabled = false;
  }
}

async function askAgent(event) {
  event.preventDefault();
  const message = elements.message.value.trim();

  if (!message) {
    elements.message.focus();
    return;
  }

  elements.sendMessage.disabled = true;
  elements.agentResponse.hidden = false;
  elements.agentStatus.textContent = "Agent 正在调用 MCP 工具…";
  elements.agentOutput.textContent = "正在生成个性化天气建议，请稍候。";

  try {
    const response = await fetch("/api/mcp/chat", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ message })
    });
    const result = await readResponse(response);
    elements.agentOutput.textContent = result.answer || "Agent 没有返回文本内容。";
    elements.agentStatus.textContent = "回答完成";
  } catch (error) {
    elements.agentOutput.textContent = `调用失败：${error.message}`;
    elements.agentStatus.textContent = "调用失败";
  } finally {
    elements.sendMessage.disabled = false;
  }
}

if (!elements.weatherForm.hidden) {
  for (const shortcut of elements.cityShortcuts) {
    shortcut.addEventListener("click", () => {
      elements.city.value = shortcut.textContent.trim();
      elements.weatherForm.requestSubmit();
    });
  }
}

if (!elements.weatherForm.hidden) {
  elements.weatherForm.addEventListener("submit", queryWeather);
}
elements.chatForm.addEventListener("submit", askAgent);

await loadConfiguration();