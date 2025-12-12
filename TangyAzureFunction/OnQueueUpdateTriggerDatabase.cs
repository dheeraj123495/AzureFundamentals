using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TangyAzureFunction.Data;
using TangyAzureFunction.Models;

namespace TangyAzureFunction
{
    public class OnQueueUpdateTriggerDatabase
    {
        private readonly ILogger<OnQueueUpdateTriggerDatabase> _logger;
        private readonly ApplicationDbContext _dbContext;
        public OnQueueUpdateTriggerDatabase(ILogger<OnQueueUpdateTriggerDatabase> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [Function(nameof(OnQueueUpdateTriggerDatabase))]
        public void Run([QueueTrigger("SalesRequestInBound")] QueueMessage message)
        {
            string messageBody = message.Body.ToString();
            SalesRequest? salesRequest = JsonConvert.DeserializeObject<SalesRequest>(messageBody);

            if(salesRequest != null)
            {
                salesRequest.Status = "";
                _dbContext.SalesRequests.Add(salesRequest);
                _dbContext.SaveChanges();
            }
            else
            {
                _logger.LogWarning("Failed to deserialize the message body to salesrequest object.");
            }
        }
    }
}
