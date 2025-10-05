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
    public class Animal: LivingBeing, IMortal, IReproducible
    {
        public string Species { get; set; }
        public int? MotherId { get; set; }
        public int? FatherId { get; set; }
        
        public Animal(int id, string name, int age, Gender gender, string species): base(id, name, age, gender)
        {
            Species = species;
        }

        public void Die()
        {
            IsAlive = false;
        }
        public double GetDeathProbability()
        {
            if (Age < 3) return 0.01;
            if (Age < 7) return 0.05;
            if (Age < 12) return 0.15;
            return 0.30;
        }
        public bool CanReproduce()
        {
            return IsAlive
                && Age >= MinReproductionAge
                && Age <= MaxReproductionAge;
        }
        public int MinReproductionAge => 2;
        public int MaxReproductionAge => 10;
    }
}
