# QuartzTimerMonitor

ASP.NET Core Web API + HTML/CSS/JS 单项目示例，演示 `Quartz.AspNetCore` 定时器的启动、停止与运行状态统计（简版固定 `demo-timer`）。

## 功能点

- 固定一个定时器：`demo-timer`
- 默认间隔 **10 秒**
- API 控制定时器：
  - `POST /api/timers/demo-timer/start`
  - `POST /api/timers/demo-timer/stop`
  - `GET /api/timers/demo-timer/status`
  - `GET /api/timers`
- 实时推送：SignalR Hub `GET /timerhub`（前端接收 `TimerRoundStarted`、`TimerRoundCompleted`）
- 统计项来自 API：
  - 是否运行中 / 是否正在执行当前轮
  - 当前配置间隔（秒）
  - Quartz 触发器状态（`triggerState`）、下次触发时间（`nextFireTimeUtc`）、上次触发时间（`previousFireTimeUtc`）
  - 调度器状态（`isSchedulerStarted`、`inStandbyMode`）

## 运行

```bash
dotnet restore
dotnet run
```

默认打开地址后访问首页 `/` 即可看到前端控制页（`wwwroot/index.html`）。

## 说明

- 当前示例使用内存态统计，不做持久化；应用重启后统计清零。
- Job 内使用短延迟模拟“每轮工作”，便于观察耗时和轮次变化。
- 代码已做“极简版”收敛：去掉 `TimerSchedulerService` 和 `DemoTimerState`，状态由 Quartz 运行时 + Job 结束指标直接组合。
