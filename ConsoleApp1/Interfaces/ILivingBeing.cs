using ConsoleApp1.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Interfaces
{
    public interface ILivingBeing
    {
        int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        bool IsAlive { get; set; }
        Gender Gender { get; set; }


        void AgeOneYear();
    }
}
