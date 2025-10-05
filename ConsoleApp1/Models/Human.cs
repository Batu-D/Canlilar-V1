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
        public MaritalStatus MaritalStatus { get; set; }
        public int? SpouseId { get; set; }
        public int? MotherId { get; set; }
        public int? FatherId { get; set; }

        public Human(int id, string name, int age, Gender gender) : base(id, name, age, gender)
        {
            MaritalStatus = MaritalStatus.Single;
            SpouseId = null;
        }
        public void Die()
        {
            IsAlive = false;
        }
        public double GetDeathProbability()
        {
            if (Age < 40) return 0.001;
            if (Age < 60) return 0.005;
            if (Age < 75) return 0.02;
            if (Age < 90) return 0.06;
            return 0.12;
        }
        public bool CanReproduce()
        {
            return IsAlive 
                && MaritalStatus == MaritalStatus.Married 
                && Age >= MinReproductionAge
                && Age <= MaxReproductionAge;
        }
        public int MinReproductionAge => Gender == Gender.Male ? 20 : 20;
        public int MaxReproductionAge => Gender == Gender.Male ? 60 : 50;
    }
}
