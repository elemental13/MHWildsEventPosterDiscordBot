using Microsoft.Extensions.Hosting;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using Microsoft.Extensions.DependencyInjection;
using Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands()
    .AddHostedService<WeeklyTriggerService>();

var host = builder.Build();

// Add commands using minimal APIs
// host.AddSlashCommand("test", "Testing!", () => "Yep I am alive!");

// Add commands from modules
host.AddModules(typeof(Program).Assembly);

// Add handlers to handle the commands
host.UseGatewayEventHandlers();

await host.RunAsync(); 
