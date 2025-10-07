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
    internal class MarriageService
    {
        private SimulationConfig _config;
        private Random _random;

        public MarriageService(SimulationConfig config, Random random)
        {
            _config = config;
            _random = random;
        }
        private double GetMarriageProbability(int age)
        {
            var band = _config.MarriageAgeBands
                .FirstOrDefault(b => age >= b.MinAge && age <= b.MaxAge);

            return band?.Probability ?? 0.0;
        }
        public List<(Human male, Human female)> ProcessMarriage(List<Humant> humans)
        {
            var candidates = humans.Where(h => 
            h.IsAlive &&
            h.MaritalStatus == MaritalStatus.Single && 
            h.MaritalStatus == MaritalStatus.Widowed &&
            h.Age >= _config.MarriageCandidateMinAge &&
            h.Age <= _config.MarriageCandidateMaxAge
            ).ToList();

            var males = candidates
                .Where(h => h.Gender == Gender.Male)
                .OrderBy(_ => _random.Next())
                .ToList();
            var females = candidates
                .Where(h => h.Gender == Gender.Female)
                .ToList();

            var usedFemaleIds = new HashSet<int>();
            var marriedCouples = new List<(Human male, Human female)>();

            foreach (var male in males)
            {
                //Erkek için evlilik olasılığı
                double maleProb = GetMarriageProbability(male.Age);
                if (_random.NextDouble() >= maleProb) continue;


                //Uygun kadın bul
                var match = females.FirstOrDefault(f =>
                !usedFemaleIds.Contains(f.Id) &&
                Math.Abs(f.Age - male.Age) <= _config.MarriageMaxAgeGap
                );

                if (match == null) continue;

                //Kadın için evlilik olasılığı
                double femaleProb = GetMarriageProbability(match.Age);
                if (_random.NextDouble() >= femaleProb) continue;

                //Evlendir
                MarryCouple(male, match);
                usedFemaleIds.Add(match.Id);
                marriedCouples.Add((male, match));
            }
            return marriedCouples;
        }
        private void MarryCouple(Human male, Human female)
        {
            male.MaritalStatus = MaritalStatus.Married;
            male.SpouseId = female.Id;

            female.MaritalStatus = MaritalStatus.Married;
            female.SpouseId = male.Id;
        }



    }
}
