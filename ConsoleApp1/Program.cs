using System;
using System.Collections.Generic;

namespace SocietySim
{
    enum Gender { Male, Female }
    enum MaritalStatus { Single, Married, Widowed }


    class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public bool IsAlive { get; set; } = true;
        public int? SpouseId { get; set; } = null;


        public override string ToString() =>
            $"#{Id} {Name} ({Gender}, {Age}) {MaritalStatus} {(IsAlive ? "" : "[DEAD]")}";
    }

    class Program
    {
        static readonly string[] MaleNames = { "Ahmet", "Mehmet", "Can", "Emre", "Burak", "Mert", "Kerem", "Ali", "Bora", "Onur" };
        static readonly string[] FemaleNames = { "Ayşe", "Elif", "Zeynep", "Naz", "Ece", "Melis", "Deniz", "Derya", "Sude", "İpek" };
        static void Main()
        {
            var rnd = new Random();
            var people = CreateInitialPopulation(200, rnd);

            

            int males = people.FindAll(p => p.Gender == Gender.Male).Count;
            int females = people.Count - males;

            int startYear = 2025;
            int i = 1;

            int deaths = 0;
            int accidents = 0;

            Console.Write($"Başlangıç Yılı: {startYear}\n");

            PrintSummary(people);

            Console.WriteLine("Similasyonu başlatmak için ENTER tuşuna basınız.");

            while (true)
            {
                int currentYear = startYear + i;
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    (deaths, accidents) = AdvanceOneYear(people, rnd);
                    PrintYearSummary(people, currentYear, deaths, accidents);
                    i++;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Döngüden çıkılıyor...");
                    break;
                }
            }
        }

        static (int deathsThisYear, int accidentsThisYear) AdvanceOneYear(List<Person> people, Random rnd)
        {
            int deathThisYear = 0;
            int accidentsThisYear = 0;

            //Yaşlandırma
            foreach (var p in people)
            {
                if (!p.IsAlive) continue;
                p.Age += 1;
            }

            //Ölüm Olayları
            foreach (var p in people)
            {
                if (!p.IsAlive) continue;
                double deathProb = DeathProbabilityByAge(p.Age);
                if (rnd.NextDouble() < deathProb)
                {
                    p.IsAlive = false;

                    KillPerson(p, people);
                    deathThisYear++;
                }
            }

            //Kaza olayları
            foreach (var p in people)
            {
                if (!p.IsAlive) continue;

                double accidentProb = 0.001;
                if (rnd.NextDouble() < accidentProb)
                {
                    accidentsThisYear++;
                    if (rnd.NextDouble() < 0.30)
                    {
                        KillPerson(p, people);
                        deathThisYear++;
                    }
                }
            }
            return (deathThisYear, accidentsThisYear);
        }

        static void PrintYearSummary(List<Person> people,int year, int deathsThisYear, int accidentThisYear)
        {
            int alive = people.FindAll(p => p.IsAlive).Count;
            int married = people.FindAll ( p => p.IsAlive && p.MaritalStatus == MaritalStatus.Married).Count;
            int widowed = people.FindAll(p => p.IsAlive && p.MaritalStatus == MaritalStatus.Widowed).Count;
            int single = alive - married - widowed;

            Console.WriteLine($"Yıl {year}: Nüfus={alive},Evli= {married}, Dul= {widowed}, Bekar= {alive-married - widowed}, Ölüm= {deathsThisYear}, Kaza={accidentThisYear}");
        }

        static void KillPerson(Person p, List<Person> all)
        {
            if (!p.IsAlive) return;
            p.IsAlive = false;
            if(p.SpouseId is int spouseId)
            {
                var spouse = all.Find(x => x.Id == spouseId);
                if(spouse != null && spouse.IsAlive)
                {
                    spouse.MaritalStatus = MaritalStatus.Widowed;
                    spouse.SpouseId = null;
                }
            }
        }

        static void PrintSummary(List<Person> people)
        {
            int alive = people.FindAll(p => p.IsAlive).Count;
            int married = people.FindAll(p => p.IsAlive && p.MaritalStatus == MaritalStatus.Married).Count;
            int widowed = people.FindAll(p => p.IsAlive && p.MaritalStatus == MaritalStatus.Widowed).Count;
            int single = alive - married - widowed;

            Console.WriteLine($"Nüfus={alive},Evli= {married}, Dul= {widowed}, Bekar= {alive - married - widowed}");
        }

        static List<Person> CreateInitialPopulation(int n, Random rnd)
        {
            var people = new List<Person>(n);
            int males = n / 2;
            int females = n - males;

            int id = 1;
            for(int i = 0; i < males; i++)
            {
                people.Add(new Person
                {
                    Id = id++,
                    Name = MaleNames[rnd.Next(MaleNames.Length)],
                    Age = rnd.Next(18, 60),
                    Gender = Gender.Male
                });
            }
            for (int i = 0; i < females; i++)
            {
                people.Add(new Person
                {
                    Id = id++,
                    Name = FemaleNames[rnd.Next(FemaleNames.Length)],
                    Age = rnd.Next(18, 60),
                    Gender = Gender.Female
                });
            }

            return people;
        }

        static double DeathProbabilityByAge(int age)
        {
            if (age < 40) return 0.001;
            if (age < 60) return 0.005;
            if (age < 75) return 0.02;
            if (age < 85) return 0.06;
            return 0.12;
        }

        static double MarriageProbabilityAge(int age)
        {
            if (age < 20) return 0.00;
            if (age <= 28) return 0.10; // %10
            if (age <= 40) return 0.07; // %7
            if (age <= 60) return 0.03; // %3
            return 0.00;
        }

        static int TryMatchMarriages(List<Person> people, Random rnd)
        {
            var candidates = people.FindAll(p => p.IsAlive 
            && p.MaritalStatus == MaritalStatus.Single && p.Age >= 18 && p.Age <= 60);

            var males = candidates.FindAll(p => p.Gender == Gender.Male).OrderBy(_ => rnd.Next()).ToList();
            var females = candidates.FindAll(p => p.Gender == Gender.Female).OrderBy(_ => rnd.Next()).ToList();


            var usedFemaleIds = new HashSet<int>();

            int marriages = 0;

            foreach (var m in males)
            {
                double prob = MarriageProbabilityAge(m.Age);
                if (rnd.NextDouble() >= prob) continue;


                var match = females.FirstOrDefault(f => !usedFemaleIds.Contains(f.Id) && Math.Abs(f.Age - m.Age) <= 10);

                if ( match == null) continue;

                m.MaritalStatus = MaritalStatus.Married;
                m.SpouseId = match.Id;

                match.MaritalStatus = MaritalStatus.Married;
                match.SpouseId = m.Id;

                usedFemaleIds.Add(match.Id);
                marriages++;
            }

            return marriages;
        }
    }
}