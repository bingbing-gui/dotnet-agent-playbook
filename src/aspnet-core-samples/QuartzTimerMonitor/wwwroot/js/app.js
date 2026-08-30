const TIMER_NAME = "demo-timer";
const messageEl = document.getElementById("message");
const allTimersBody = document.getElementById("allTimersBody");
const healthStateEl = document.getElementById("health-state");
const healthDescEl = document.getElementById("health-desc");
const statusCircleEl = document.getElementById("status-circle");
const statusCircleIconEl = document.getElementById("status-circle-icon");

const fields = {
    running: document.getElementById("status-running"),
    executing: document.getElementById("status-executing"),
    interval: document.getElementById("status-interval")
};

let hubConnection = null;
const pushHistory = [];

document.getElementById("startBtn").addEventListener("click", startTimer);
document.getElementById("stopBtn").addEventListener("click", stopTimer);
document.getElementById("refreshBtn").addEventListener("click", refreshCurrentTimerStatus);

void refreshCurrentTimerStatus(false);
void connectSignalR();

async function connectSignalR() {
    if (hubConnection) {
        return;
    }

    hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/timerhub")
        .withAutomaticReconnect()
        .build();

    hubConnection.on("TimerRoundStarted", payload => {
        const startedAtUtc = payload?.startedAtUtc ?? null;
        prependHistory({
            event: "一轮",
            startedAtUtc,
            completedAtUtc: null,
            error: null
        });
        setMessage("已收到开始时间", false);
        void refreshCurrentTimerStatus(false);
    });

    hubConnection.on("TimerRoundCompleted", payload => {
        const completedAtUtc = payload?.completedAtUtc ?? null;
        const error = payload?.error ?? null;
        const openRow = pushHistory.find(item => item.startedAtUtc && !item.completedAtUtc);
        if (openRow) {
            openRow.completedAtUtc = completedAtUtc;
            openRow.error = error;
            renderHistoryTable();
        } else {
            prependHistory({
                event: "一轮",
                startedAtUtc: null,
                completedAtUtc,
                error
            });
        }
        if (error) {
            setMessage(`任务执行异常：${error}`, true);
        }

        void refreshCurrentTimerStatus(false);
    });

    hubConnection.onreconnecting(() => {
        setMessage("SignalR 重连中...", true);
    });

    hubConnection.onreconnected(() => {
        setMessage("SignalR 已重连", false);
    });

    try {
        await hubConnection.start();
        setMessage("SignalR 已连接", false);
    } catch {
        setMessage("SignalR 连接失败", true);
    }
}

function prependHistory(entry) {
    pushHistory.unshift(entry);
    renderHistoryTable();
}

function renderHistoryTable() {
    if (pushHistory.length === 0) {
        allTimersBody.innerHTML = `<tr><td colspan="4" class="empty">暂无推送记录</td></tr>`;
        return;
    }

    allTimersBody.innerHTML = pushHistory.map(item => `
        <tr>
            <td>${escapeHtml(item.event)}</td>
            <td>${formatDateTime(item.startedAtUtc)}</td>
            <td>${formatDateTime(item.completedAtUtc)}</td>
            <td>${item.error ? escapeHtml(item.error) : "-"}</td>
        </tr>`).join("");
}

async function startTimer() {
    const response = await fetch(`/api/timers/${encodeURIComponent(TIMER_NAME)}/start`, {
        method: "POST"
    });

    await handleTimerResponse(response, "已启动 demo-timer");
}

async function stopTimer() {
    const response = await fetch(`/api/timers/${encodeURIComponent(TIMER_NAME)}/stop`, {
        method: "POST"
    });

    await handleTimerResponse(response, "已停止 demo-timer");
}

async function refreshCurrentTimerStatus(showMessage = true) {
    const response = await fetch(`/api/timers/${encodeURIComponent(TIMER_NAME)}/status`);
    if (!response.ok) {
        setMessage("刷新状态失败", true);
        return;
    }

    const status = await response.json();
    renderCurrentStatus(status);
    if (showMessage) {
        setMessage("已刷新状态", false);
    }
}

async function handleTimerResponse(response, okMessage) {
    if (!response.ok) {
        const errorData = await safeJson(response);
        setMessage(errorData?.message ?? "请求失败", true);
        return;
    }

    const status = await response.json();
    renderCurrentStatus(status);
    setMessage(okMessage, false);
}

function renderCurrentStatus(status) {
    fields.running.textContent = boolText(status.isRunning);
    fields.executing.textContent = boolText(status.isExecuting);
    fields.interval.textContent = String(status.intervalSeconds);

    if (status.isExecuting) {
        healthStateEl.textContent = "执行中";
        healthDescEl.textContent = "任务正在运行";
        healthStateEl.classList.remove("status-title--small");
        statusCircleEl.style.background = "linear-gradient(135deg, #63a6ff 0%, #2f7cff 100%)";
        statusCircleIconEl.className = "bi bi-arrow-repeat";
    } else if (status.isRunning) {
        healthStateEl.textContent = "正常";
        healthDescEl.textContent = "所有定时任务运行正常";
        healthStateEl.classList.remove("status-title--small");
        statusCircleEl.style.background = "linear-gradient(135deg, #52e3be 0%, #2fcf94 100%)";
        statusCircleIconEl.className = "bi bi-check-circle-fill";
    } else {
        healthStateEl.textContent = "已停止";
        healthDescEl.textContent = "";
        healthStateEl.classList.add("status-title--small");
        statusCircleEl.style.background = "linear-gradient(135deg, #9aa7bd 0%, #6e7f9f 100%)";
        statusCircleIconEl.className = "bi bi-pause-circle-fill";
    }
}

function boolText(value) {
    return value ? "是" : "否";
}

function setMessage(message, isError) {
    messageEl.textContent = message;
    messageEl.style.color = isError ? "#b42318" : "#0a7f2e";
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}

function formatDateTime(value) {
    if (!value) {
        return "-";
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return date.toLocaleString("zh-CN", { hour12: false });
}

async function safeJson(response) {
    try {
        return await response.json();
    } catch {
        return null;
    }
}
