using JSONStashAPI.CSharp;
using JSONStashAPI.CSharp.Models;
using JSONStashAPI.CSharp.Extensions;
using Newtonsoft.Json;
using WebStatus.Common.Models;

namespace WebStatus.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string host = _configuration["host"];
            string key = _configuration["key"];
            string stash = _configuration["stash"];
            string addresses = _configuration["addresses"];

            JSONStash storage = new(host);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    StashResponse response = await storage.GetStashDataAsync(key, stash);
                }
                catch (Exception ex)
                {

                }

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}