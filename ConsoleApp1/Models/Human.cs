using ConsoleApp1.Enums;
using ConsoleApp1.Interfaces;
using SocietySim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class Human : LivingBeing , IMortal, IReproducible
    {
        private static SimulationConfig _config;
        public static void SetConfig(SimulationConfig config)
        {
            _config = config;
        }
        /*public int MinReproductionAge =>
            Gender == Gender.Male
                ? _config.HumanMinReproductionAgeMale
                : _config.HumanMinReproductionAgeFemale;
        public double GetDeathProbability()
        {
            var band = _config.DeathBands
                .FirstOrDefault(b => Age >= b.MinAge && Age <= b.MaxAge);
            return band?.Probability ?? 0.001;
        }*/ 
        public MaritalStatus MaritalStatus { get; set; }
        public int? SpouseId { get; set; }
        public int? MotherId { get; set; }
        public int? FatherId { get; set; }
        
        public Human(int id, string name, int age, Gender gender) : base(id, name, gender, age)
        {
            MaritalStatus = MaritalStatus.Single;
            SpouseId = null;
        }
        public void Die()
        {
            IsAlive = false;
        }
    }
}
