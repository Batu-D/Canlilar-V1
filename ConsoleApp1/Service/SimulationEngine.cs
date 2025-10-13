using ConsoleApp1.Enums;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Models;
using SocietySim;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1.Service
{
    public class SimulationEngine
    {
        private SimulationConfig _config;
        private Random _random;

        private BirthService _birthService;
        private DeathService _deathService;
        private MarriageService _marriageService;

        private int _nextId = 1; // ID Sayacı

        public SimulationEngine(SimulationConfig config)
        {
            _config = config;
            _random = new Random();

            _birthService = new BirthService(_config, _random);
            _deathService = new DeathService(_config, _random);
            _marriageService = new MarriageService(_config, _random);
        }

        public List<ILivingBeing> CreateInitialPopulation(int humanCount, int animalCount)
        {
            var beings = new List<ILivingBeing>();

            // İnsanları oluştur
            int maleCount = humanCount / 2;
            int femaleCount = humanCount - maleCount;

            for (int i = 0; i < maleCount; i++)
            {
                var name = _config.MaleNames[_random.Next(_config.MaleNames.Count)];
                beings.Add(new Human(_nextId++, name, _random.Next(18, 30), Gender.Male));
            }

            for (int i = 0; i < femaleCount; i++)
            {
                var name = _config.FemaleNames[_random.Next(_config.FemaleNames.Count)];
                beings.Add(new Human(_nextId++, name, _random.Next(18, 30), Gender.Female));
            }

            // Hayvanları oluştur
            // Tür havuzu: config'teki AnimalNamesBySpecies varsa oradan; yoksa default
            var speciesPool = (_config.AnimalNamesBySpecies != null && _config.AnimalNamesBySpecies.Count > 0)
                ? _config.AnimalNamesBySpecies.Keys.ToList()
                : new List<string> { "Köpek", "Kedi", "Kuş" };

            for (int i = 0; i < animalCount; i++)
            {
                var gender = _random.NextDouble() < 0.5 ? Gender.Male : Gender.Female;

                // Tür seç
                var sp = speciesPool[_random.Next(speciesPool.Count)];

                // İsim: önce tür-bazlı, yoksa genel AnimalNames, o da yoksa fallback
                var name = GetAnimalName(sp);

                beings.Add(new Animal(_nextId++, name, _random.Next(1, 5), gender, sp));
            }

            return beings;
        }

        public SimulationYearResult AdvanceOneYear(List<ILivingBeing> beings, int currentYear)
        {
            var result = new SimulationYearResult { Year = currentYear };

            // 1️⃣ YAŞLANDIR
            foreach (var being in beings.Where(b => b.IsAlive))
            {
                being.AgeOneYear();
            }

            // 2️⃣ EVLİLİK (sadece Human için)
            var humans = beings.OfType<Human>().ToList();
            var marriedCouples = _marriageService.ProcessMarriage(humans);

            foreach (var (male, female) in marriedCouples)
            {
                result.EventLog.Add($"{currentYear}: #{male.Id} {male.Name} ve #{female.Id} {female.Name} evlendi!");
            }
            result.Marriages = marriedCouples.Count;

            // 3️⃣ DOĞUM
            var newborns = _birthService.ProcessBirths(beings, ref _nextId);
            beings.AddRange(newborns);  // ✅ Yeni bebekleri listeye ekle!

            foreach (var baby in newborns)
            {
                string type = baby is Human ? "İnsan" : "Hayvan";
                result.EventLog.Add($"{currentYear}: #{baby.Id} {baby.Name} ({type}) doğdu!");
            }
            result.Births = newborns.Count;

            // 4️⃣ KAZA
            var (accidentDeaths, accidentCount) = _deathService.ProcessAccidents(beings);

            foreach (var dead in accidentDeaths)
            {
                string type = dead is Human ? "İnsan" : "Hayvan";
                result.EventLog.Add($"{currentYear}: #{dead.Id} {dead.Name} ({type}) kazada öldü!");
            }
            result.Accidents = accidentCount;
            result.Deaths += accidentDeaths.Count;

            // 5️⃣ ÖLÜM (yaşlılık)
            var naturalDeaths = _deathService.ProcessNaturalDeaths(beings);

            foreach (var dead in naturalDeaths)
            {
                string type = dead is Human ? "İnsan" : "Hayvan";
                result.EventLog.Add($"{currentYear}: #{dead.Id} {dead.Name} ({type}) yaşlılıktan öldü (yaş {dead.Age}).");
            }
            result.Deaths += naturalDeaths.Count;

            // 6️⃣ İSTATİSTİKLERİ HESAPLA
            result.TotalPopulation = beings.Count(b => b.IsAlive);
            result.AliveHumans = beings.OfType<Human>().Count(h => h.IsAlive);
            result.AliveAnimals = beings.OfType<Animal>().Count(a => a.IsAlive);

            // BONUS: Reflection ile tip bazında sayma
            Console.WriteLine("\n--- Tip Bazında İstatistikler (Reflection ile) ---");
            var typeCounts = beings
                .Where(b => b.IsAlive)
                .GroupBy(b => b.GetType().Name)  // ✅ Reflection!
                .Select(g => new { Type = g.Key, Count = g.Count() });

            foreach (var tc in typeCounts)
            {
                Console.WriteLine($"  {tc.Type}: {tc.Count}");
            }

            return result;
        }

        // ✅ Eksik method eklendi
        private string GetAnimalName(string species)
        {
            // Önce türe özel isim listesine bak
            if (_config.AnimalNamesBySpecies != null &&
                _config.AnimalNamesBySpecies.ContainsKey(species) &&
                _config.AnimalNamesBySpecies[species].Count > 0)
            {
                var nameList = _config.AnimalNamesBySpecies[species];
                return nameList[_random.Next(nameList.Count)];
            }

            // Genel hayvan isimleri listesine bak
            if (_config.AnimalNames != null && _config.AnimalNames.Count > 0)
            {
                return _config.AnimalNames[_random.Next(_config.AnimalNames.Count)];
            }

            // Fallback: Varsayılan isim
            return $"{species}{_random.Next(1, 100)}";
        }
    }

    public class SimulationYearResult
    {
        public int Year { get; set; }
        public int TotalPopulation { get; set; }
        public int AliveHumans { get; set; }
        public int AliveAnimals { get; set; }

        public int Marriages { get; set; }
        public int Births { get; set; }
        public int Deaths { get; set; }
        public int Accidents { get; set; }

        public List<string> EventLog { get; set; } = new();
    }
}