using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;   
using TangyAzureFunction.Data;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        string connectingString = Environment.GetEnvironmentVariable("AzureSqlDatabase");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectingString));

    }).Build();





host.Run();
