using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TangyAzureFunction.Data;
using TangyAzureFunction.Models;

namespace TangyAzureFunction;

public class UpdateStatusToComplete
{
    private readonly ILogger _logger;
    private readonly ApplicationDbContext _dbContext;
    public UpdateStatusToComplete(ILoggerFactory loggerFactory, ApplicationDbContext dbContext)
    {
        _logger = loggerFactory.CreateLogger<UpdateStatusToComplete>();
        _dbContext = dbContext;
    }

    [Function("UpdateStatusToComplete")]
    public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        IEnumerable<SalesRequest> salesRequests = _dbContext.SalesRequests.Where(u => u.Status == "Image Processed").ToList();
        foreach (var item in salesRequests)
        {
            item.Status = "Completed";
        }
        _dbContext.SaveChanges();

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}