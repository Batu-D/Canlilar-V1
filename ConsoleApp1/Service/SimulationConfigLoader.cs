using System.Text.Json;

namespace ConsoleApp1.Service
{
    public static class SimulationConfigLoader
    {
        public static SimulationConfig Load(string fileName)
        {
            var path = Path.IsPathRooted(fileName)
                ? fileName
                : Path.Combine(AppContext.BaseDirectory, fileName);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Config dosyası bulunamadı.", path);
            }

            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var config = JsonSerializer.Deserialize<SimulationConfig>(json, options)
                         ?? throw new InvalidOperationException("Config deserialize edilemedi (null döndü).");

            Console.WriteLine("Config dosyası yüklendi.\n");
            return config;
        }
    }
}
