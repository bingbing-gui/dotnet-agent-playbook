using QuartzTimerMonitor.Jobs;
using QuartzTimerMonitor;
using Quartz;
using Quartz.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddQuartz(options =>
{
    var demoTimerJobKey = new JobKey("demo-timer-job");
    options.AddJob<NamedTimerJob>(job => job
        .WithIdentity(demoTimerJobKey)
        .UsingJobData(NamedTimerJob.TimerNameDataKey, "demo-timer"));

    options.AddTrigger(trigger => trigger
        .ForJob(demoTimerJobKey)
        .WithIdentity("demo-timer-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));
});
builder.Services.AddQuartzServer(options =>
{
    options.WaitForJobsToComplete = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapHub<TimerHub>("/timerhub");

app.Run();
