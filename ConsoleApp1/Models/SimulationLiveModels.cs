using System.Collections.Generic;

namespace ConsoleApp1.Models
{
    public class SimulationStateSnapshot
    {
        public int StartYear { get; init; }
        public int CurrentYear { get; init; }
        public bool IsRunning { get; init; }
        public List<SimulationYearResult> History { get; init; } = new();
    }

    public class SimulationYearUpdate
    {
        public SimulationYearResult YearResult { get; init; } = new();
        public bool IsRunning { get; init; }
        public int CurrentYear { get; init; }
    }
}
