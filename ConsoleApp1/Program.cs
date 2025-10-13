using ConsoleApp1.Models;
using ConsoleApp1.Service;
using SocietySim;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Contains("--server", StringComparer.OrdinalIgnoreCase))
            {
                await SimulationDashboardServer.RunAsync(args);
                return;
            }

            RunConsoleMode();
        }

        private static void RunConsoleMode()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.WriteLine("=== TOPLUM SİMÜLASYONU ===\n");

            var config = SimulationConfigLoader.Load("appsettings.json");
            var engine = new SimulationEngine(config);

            var yearlyHistory = new List<SimulationYearResult>();

            Console.Write("Kaç insan ile başlamak istersiniz? (varsayılan: 100): ");
            string humanInput = Console.ReadLine();
            int humanCount = string.IsNullOrWhiteSpace(humanInput) ? 100 : int.Parse(humanInput);

            Console.Write("Kaç hayvan ile başlamak istersiniz? (varsayılan: 20): ");
            string animalInput = Console.ReadLine();
            int animalCount = string.IsNullOrWhiteSpace(animalInput) ? 20 : int.Parse(animalInput);

            var beings = engine.CreateInitialPopulation(humanCount, animalCount);

            int startYear = 2025;
            int currentYear = startYear;

            Console.WriteLine($"\n--- Başlangıç Yılı: {startYear} ---");
            SimulationEngine.PrintSummary(beings);

            var initialSnapshot = engine.CreateSnapshot(beings, startYear);
            yearlyHistory.Add(initialSnapshot);

            Console.WriteLine("\n[ENTER] Bir yıl ilerlet | [ESC] Çıkış");

            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nSimülasyon sonlandırıldı. Hoşçakalın!");
                    break;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    currentYear++;

                    var result = engine.AdvanceOneYear(beings, currentYear);

                    yearlyHistory.Add(result);

                    SimulationEngine.PrintYearResult(result, beings);

                    if (result.TotalPopulation == 0)
                    {
                        Console.WriteLine("\n💀 TÜM NÜFUS YIKILDI! Simülasyon sona erdi.");
                        break;
                    }
                }
            }

            Console.WriteLine("\nSimülasyon toplam süre: " + (currentYear - startYear) + " yıl");

            PersistHistory(startYear, currentYear, yearlyHistory);
        }

        static void PersistHistory(int startYear, int endYear, List<SimulationYearResult> results)
        {
            try
            {
                if (results.Count == 0)
                {
                    return;
                }

                var export = new SimulationRunExport
                {
                    StartYear = startYear,
                    EndYear = endYear,
                    GeneratedAt = DateTime.UtcNow,
                    YearlyResults = results
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
                Directory.CreateDirectory(outputDir);

                var filePath = Path.Combine(outputDir, "simulation-history.json");
                File.WriteAllText(filePath, JsonSerializer.Serialize(export, options));

                Console.WriteLine($"\n📁 Simülasyon geçmişi '{filePath}' dosyasına kaydedildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n⚠️ Simülasyon geçmişi kaydedilirken hata oluştu: {ex.Message}");
            }
        }


    }
}
