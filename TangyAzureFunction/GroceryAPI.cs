using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TangyAzureFunction.Data;
using TangyAzureFunction.Models;

namespace TangyAzureFunction;

public class GroceryAPI
{
    private readonly ILogger<GroceryAPI> _logger;
    private readonly ApplicationDbContext _dbContext;
    public GroceryAPI(ILogger<GroceryAPI> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext  = dbContext;
    }

    [Function("GetGrocery")]
    public IActionResult GetGrocery([HttpTrigger(AuthorizationLevel.Function, "get", Route ="GroceryList")] HttpRequest req)
    {
        _logger.LogInformation("Getting grocery list items");
        return new OkObjectResult(_dbContext.GroceryItems.ToList());
    }

    [Function("GetGroceryById")]
    public IActionResult GetGroceryById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GroceryList/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation("Getting grocery list item - " + id);
        return new OkObjectResult(_dbContext.GroceryItems.FirstOrDefault(x => x.Id == id));
    }

    [Function("CreateGrocery")]
    public async Task<IActionResult> CreateGrocery([HttpTrigger(AuthorizationLevel.Function, "post", Route = "GroceryList")] HttpRequest req)
    {
        _logger.LogInformation("Creating grocery list item.");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        GroceryItem_Upsert? data = JsonConvert.DeserializeObject<GroceryItem_Upsert>(requestBody);

        GroceryItem groceryItem = new GroceryItem()
        {
            Name = data!.Name
        };

        _dbContext.GroceryItems.Add(groceryItem);
        _dbContext.SaveChanges();
        return new OkObjectResult(groceryItem);
    }

    [Function("UpdateGrocery")]
    public async Task<IActionResult> UpdateGrocery([HttpTrigger(AuthorizationLevel.Function, "put", Route = "GroceryList/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation("Upadting grocery item " + id);

        GroceryItem? groceryItem = _dbContext.GroceryItems.FirstOrDefault(x => x.Id == id);
        if(groceryItem == null)
        {
            return new NotFoundObjectResult("Item not found");
        }
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        GroceryItem_Upsert? data = JsonConvert.DeserializeObject<GroceryItem_Upsert>(requestBody);
        if(!string.IsNullOrEmpty(data.Name))
        {
            groceryItem.Name = data.Name;
            _dbContext.SaveChanges();
        }       
        return new OkObjectResult(groceryItem);
    }

    [Function("DeleteGrocery")]
    public async Task<IActionResult> DeleteGrocery([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "GroceryList/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation("Deleting grocery item " + id);

        GroceryItem? groceryItem = _dbContext.GroceryItems.FirstOrDefault(x => x.Id == id);
        if (groceryItem == null)
        {
            return new NotFoundObjectResult("Item not found");
        }
        _dbContext.GroceryItems.Remove(groceryItem);
        return new OkResult();
    }
}