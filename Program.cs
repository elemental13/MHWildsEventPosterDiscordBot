using Microsoft.Extensions.Hosting;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using Microsoft.Extensions.DependencyInjection;
using Services;

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands()
    .AddHostedService<WeeklyTriggerService>();
    
var host = builder.Build();

// Add handlers to handle the commands
host.UseGatewayEventHandlers();

await host.RunAsync(); 
