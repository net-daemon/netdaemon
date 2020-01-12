using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace runner
{
    class RunnerService : BackgroundService
    {
        private readonly ILogger<RunnerService> _logger;
        //private readonly 

        public RunnerService(ILoggerFactory loggerFactory, ILogger<RunnerService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting stuff!");
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Doing stuff!");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Canceled Ending stuff!");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Some problem!");
            }

            _logger.LogInformation("Ending stuff!");
        }
    }
}
