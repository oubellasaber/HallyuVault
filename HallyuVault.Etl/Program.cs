using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateApplicationBuilder(args);

// DI
host.Services.AddHttpClient();


// Workers
host.Services.AddHostedService<Worker>();   // Worker : BackgroundService

// Logging
host.Logging.ClearProviders()
            .AddConsole();                  // or Serilog, Seq, etc.

// Build
await host.Build().RunAsync();
