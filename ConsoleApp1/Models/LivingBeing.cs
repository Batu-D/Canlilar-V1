using ConsoleApp1.Enums;
using ConsoleApp1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public abstract class LivingBeing : ILivingBeing
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public int Age { get; set; }
        public bool IsAlive { get; set; }
        public Gender Gender { get; set; }
        protected LivingBeing(int id, string name, int age,Gender gender) 
        {
            Id = id;
            Name = name;
            Age = age;
            IsAlive = true;
            Gender = gender;
        }   

        public void AgeOneYear()
        {
            if (IsAlive)
                Age++;
        }
    }
}
