using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ConsoleApp1.Service
{
    public static class SimulationDashboardServer
    {
        public static async Task RunAsync(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                          .SetIsOriginAllowed(_ => true);
                });
            });

            builder.Services.AddSignalR();
            builder.Services.AddSingleton(_ => SimulationConfigLoader.Load("appsettings.json"));
            builder.Services.AddSingleton<SimulationManager>();
            builder.Services.AddSingleton<SimulationBroadcaster>();

            var app = builder.Build();

            app.UseRouting();
            app.UseCors();

            app.MapHub<SimulationHub>("/simulationHub");
            app.MapGet("/", () => Results.Ok(new { message = "Simülasyon SignalR sunucusu çalışıyor." }));

            app.Services.GetRequiredService<SimulationBroadcaster>();

            Console.WriteLine("SignalR sunucusu http://localhost:5000 adresinde dinliyor (varsayılan).");
            Console.WriteLine("Simülasyonu kontrol etmek için dashboard arayüzünü kullanın.");

            await app.RunAsync();
        }
    }
}
