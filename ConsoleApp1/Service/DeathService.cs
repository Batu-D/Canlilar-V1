using ConsoleApp1.Enums;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Models;
using SocietySim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Service
{
    internal class DeathService
    {
        private SimulationConfig _config;
        private Random _random;
        public DeathService(SimulationConfig config, Random random)
        {
            _config = config;
            _random = random;
        }
        public double GetDeathProbability(int age)
        {
            var band = _config.DeathBands
                .FirstOrDefault(b => age >= b.MinAge && age <= b.MaxAge);

            return band?.Probability ?? 0.0;
        }
        public List<ILivingBeing> ProcessNaturalDeaths(List<ILivingBeing> beings)
        {
            var deaths = new List<ILivingBeing>();

            foreach (var being in beings)
            {
                if (!being.IsAlive) continue;

                //Canlının kendi ölüm ihtimalini al
                var mortalBeing = being as IMortal; // cast etmek
                if (mortalBeing == null) continue; // IMortal Değilse atla

                double deathProb = mortalBeing.GetDeathProbability();

                if (_random.NextDouble() < deathProb)
                {
                    KillBeing(being, beings);
                    deaths.Add(being);
                }
            }
            return deaths;
        }
        public (List<(ILivingBeing victim, string accidentType)> deaths, int accidents)
    ProcessAccidents(List<ILivingBeing> beings)
        {
            var deaths = new List<(ILivingBeing victim, string accidentType)>();
            int accidentCount = 0;

            foreach (var being in beings)
            {
                if (!being.IsAlive) continue;

                // Kaza geçirdi mi?
                if (_random.NextDouble() < _config.AccidentAnnualProbability)
                {
                    accidentCount++;

                    // Kaza türünü seç
                    string accType = PickAccidentType();

                    // Ölümcül mü?
                    if (_random.NextDouble() < _config.AccidentFatalityProbability)
                    {
                        KillBeing(being, beings);
                        deaths.Add((being, accType));
                    }
                    // değilse: sadece kaza, yaşıyor (istersen burada da log tutabilirsin)
                }
            }

            return (deaths, accidentCount);
        }

        private string PickAccidentType()
        {
            var list = _config.AccidentTypes;
            if (list == null || list.Count == 0) return "kaza";
            return list[_random.Next(list.Count)];
        }

        public void KillBeing(ILivingBeing being, List<ILivingBeing> allBeings)
        {
            //1.Canlıyı öldür
            var mortalBeing = being as IMortal;
            mortalBeing?.Die();

            //2.Eğer Human ise ve evliyse, eşini dul yap
            if(being is Human human && human.SpouseId.HasValue)
            {
                //Eşi bul
                var spouse = allBeings
                    .OfType<Human>()
                    .FirstOrDefault(h => h.Id == human.SpouseId.Value);
                if (spouse != null && spouse.IsAlive)
                {
                    //Dul yap
                    spouse.MaritalStatus = MaritalStatus.Widowed;
                    spouse.SpouseId = null;
                }

                //Ölünün evlilik bilgisini temizle
                human.SpouseId = null;
            }
        }
    }
}
