using System;
using System.Collections.Generic;
using ConsoleApp1.Service;

namespace ConsoleApp1.Models
{
    public class SimulationRunExport
    {
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<SimulationYearResult> YearlyResults { get; set; } = new();
    }
}
