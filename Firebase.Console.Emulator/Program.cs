using Firebase.Emulator.Configurations;
using Firebase.Emulator.Models;
using Firebase.Emulator.Services;
using Firebase.Emulator.Services.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Firebase.Emulator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            System.Console.Title = "Aplication Emulator from Firebase-tools";

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, configBuilder) =>
                {
                    configBuilder.Sources.Clear();

                    configBuilder
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    configBuilder.AddJsonFile("appsettings.User.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.AddAppConfiguration(configuration);

                    services.AddSingleton<IEmulatorService, EmulatorService>();
                    services.AddHttpClient<IAuthService, AuthService>();
                    services.AddScoped<IOrchestratorService, OrchestratorService>();
                    services.AddScoped<IDependencyValidator, DependencyValidator>();

                })
                .Build();

            using (var scope = host.Services.CreateScope())
            {
                var orchestrator = scope.ServiceProvider.GetRequiredService<IOrchestratorService>();

                await orchestrator.RunAsync();
            }
        }
    }
}