using Quartz;

namespace AspNetCore.Quartz.Models
{
    public class SyncFinishedOrderJob : IJob
    {
        private readonly ILogger<SyncFinishedOrderJob> _logger;
        private readonly IConfiguration _configuration;
        public SyncFinishedOrderJob(IConfiguration configuration,ILogger<SyncFinishedOrderJob> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("run .........");
            return Task.CompletedTask;
        }
    }
}
