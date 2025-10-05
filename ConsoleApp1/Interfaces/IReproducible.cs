using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Interfaces
{
    public interface IReproducible
    {
        bool CanReproduce();
        int MinReproductionAge { get; }
        int MaxReproductionAge { get; }
    }
}
