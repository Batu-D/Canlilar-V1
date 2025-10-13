using ConsoleApp1.Models;
using Microsoft.AspNetCore.SignalR;

namespace ConsoleApp1.Service
{
    public class SimulationBroadcaster
    {
        private readonly SimulationManager _manager;
        private readonly IHubContext<SimulationHub> _hubContext;

        public SimulationBroadcaster(SimulationManager manager, IHubContext<SimulationHub> hubContext)
        {
            _manager = manager;
            _hubContext = hubContext;

            _manager.StateChanged += OnStateChanged;
            _manager.YearAdvanced += OnYearAdvanced;
        }

        private void OnStateChanged(object? sender, SimulationStateSnapshot snapshot)
        {
            _ = _hubContext.Clients.All.SendAsync("ReceiveState", snapshot);
        }

        private void OnYearAdvanced(object? sender, SimulationYearUpdate update)
        {
            _ = _hubContext.Clients.All.SendAsync("ReceiveYear", update);
        }
    }
}
