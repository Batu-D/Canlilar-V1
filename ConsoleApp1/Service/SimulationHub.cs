using ConsoleApp1.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ConsoleApp1.Service
{
    public class SimulationHub : Hub
    {
        private readonly SimulationManager _manager;

        public SimulationHub(SimulationManager manager)
        {
            _manager = manager;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var snapshot = _manager.GetSnapshot();
            await Clients.Caller.SendAsync("ReceiveState", snapshot);
        }

        public Task StartSimulation()
        {
            _manager.Start();
            return Task.CompletedTask;
        }

        public Task PauseSimulation()
        {
            _manager.Pause();
            return Task.CompletedTask;
        }

        public Task StepSimulation()
        {
            _manager.Step();
            return Task.CompletedTask;
        }

        public Task ResetSimulation()
        {
            _manager.Reset();
            return Task.CompletedTask;
        }
    }
}
