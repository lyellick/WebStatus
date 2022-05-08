using JSONStashAPI.CSharp;
using JSONStashAPI.CSharp.Models;
using JSONStashAPI.CSharp.Extensions;
using Newtonsoft.Json;
using WebStatus.Common.Models;
using System.Net;

namespace WebStatus.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private Dictionary<string, List<Status>> _statuses;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _statuses = new();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true) 
            {
                string host = _configuration["host"];
                string key = _configuration["key"];
                string stash = _configuration["stash"];
                string addressesToCheck = _configuration["addresses"].ToLower();

                string[] addresses = addressesToCheck.Split(",");

                JSONStash storage = new(host);

                StashResponse data = await storage.GetStashDataAsync(key, stash);

                if (data.TryParseData(out _statuses))
                {
                    foreach (string address in addresses)
                    {
                        using HttpClient client = new HttpClient();
                        HttpResponseMessage response = await client.GetAsync(address);
                        Status status = new() { IsUp = response.IsSuccessStatusCode, Checked = DateTimeOffset.UtcNow };

                        if (!_statuses.ContainsKey(address))
                            _statuses.Add(address, new());

                        _statuses[address].Add(status);
                    }
                }

                string json = JsonConvert.SerializeObject(_statuses);

                long size = data.Metadata.Quota.UsedBytes + (json.Length * sizeof(char));

                if (size > data.Metadata.Quota.MaxBytes)
                    break;
                
                await storage.UpdateStashDataAsync(key, stash, json);

                Thread.Sleep(15000);
            }
        }
    }
}