using System;
using System.Collections.Generic;
using System.Linq;

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
        public int? MotherId { get; set; } = null;
        public int? FatherId { get; set; } = null;

        public override string ToString() =>
            $"#{Id} {Name} ({Gender}, {Age}) {MaritalStatus} {(IsAlive ? "" : "[DEAD]")}";
    }

    class Program
    {
        static readonly string[] MaleNames = { "Ahmet", "Mehmet", "Can", "Emre", "Burak", "Mert", "Kerem", "Ali", "Bora", "Onur" };
        static readonly string[] FemaleNames = { "Ayşe", "Elif", "Zeynep", "Naz", "Ece", "Melis", "Deniz", "Derya", "Sude", "İpek" };
        static int _nextId = 1;

        static void Main()
        {
            var rnd = new Random();
            var people = CreateInitialPopulation(200, rnd);

            int startYear = 2025;
            int i = 1;

            Console.Write($"Başlangıç Yılı: {startYear}\n");
            PrintSummary(people);
            Console.WriteLine("Similasyonu başlatmak için ENTER tuşuna basınız.");

            while (true)
            {
                int currentYear = startYear + i;
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    var (deaths, accidents, marriages, births, maleDeaths, femaleDeaths, birthDetails) = AdvanceOneYear(people, rnd);
                    PrintYearSummary(people, currentYear, deaths, accidents, marriages, births, maleDeaths, femaleDeaths, birthDetails);
                    i++;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Döngüden çıkılıyor...");
                    break;
                }
            }
        }

        static (int deathsThisYear, int accidentsThisYear, int marriagesThisYear, int birthsThisYear, int maleDeathsThisYear, int femaleDeathsThisYear, string birthDetails) AdvanceOneYear(List<Person> people, Random rnd)
        {
            int deathsThisYear = 0;
            int accidentsThisYear = 0;
            int marriagesThisYear = 0;
            int birthsThisYear = 0;
            int maleDeathsThisYear = 0;
            int femaleDeathsThisYear = 0;
            string birthDetails = "";

            // Yaşlandırma
            foreach (var p in people)
            {
                if (!p.IsAlive) continue;
                p.Age += 1;
            }

            // Evlilik Olayları
            marriagesThisYear = TryMatchMarriages(people, rnd);

            // Ölüm Olayları - FIX: Create a copy to avoid modification during iteration
            var alivePeople = people.Where(p => p.IsAlive).ToList();

            foreach (var p in alivePeople)
            {
                double deathProb = DeathProbabilityByAge(p.Age);
                if (rnd.NextDouble() < deathProb)
                {
                    KillPerson(p, people);
                    deathsThisYear++;

                    // FIX: Count deaths by gender correctly
                    if (p.Gender == Gender.Male)
                        maleDeathsThisYear++;
                    else
                        femaleDeathsThisYear++;
                }
            }

            // Kaza olayları - FIX: Use updated alive list and avoid double death counting
            alivePeople = people.Where(p => p.IsAlive).ToList();

            foreach (var p in alivePeople)
            {
                double accidentProb = 0.001;
                if (rnd.NextDouble() < accidentProb)
                {
                    accidentsThisYear++;
                    if (rnd.NextDouble() < 0.30)
                    {
                        KillPerson(p, people);
                        deathsThisYear++;

                        // Count accident deaths by gender
                        if (p.Gender == Gender.Male)
                            maleDeathsThisYear++;
                        else
                            femaleDeathsThisYear++;
                    }
                }
            }

            // Doğum Olayları
            var (births, details) = TryBirths(people, rnd);
            birthsThisYear = births;
            birthDetails = details;

            return (deathsThisYear, accidentsThisYear, marriagesThisYear, birthsThisYear, maleDeathsThisYear, femaleDeathsThisYear, birthDetails);
        }

        static void PrintYearSummary(List<Person> people, int year, int deathsThisYear, int accidentThisYear, int marriagesThisYear, int birthsThisYear, int maleDeaths, int femaleDeaths, string birthDetails)
        {
            // FIX: Count only alive people for gender statistics
            int aliveMales = people.Count(p => p.IsAlive && p.Gender == Gender.Male);
            int aliveFemales = people.Count(p => p.IsAlive && p.Gender == Gender.Female);
            int alive = aliveMales + aliveFemales;

            int married = people.Count(p => p.IsAlive && p.MaritalStatus == MaritalStatus.Married);
            int widowed = people.Count(p => p.IsAlive && p.MaritalStatus == MaritalStatus.Widowed);
            int single = alive - married - widowed;

            Console.WriteLine($@"
Yıl: {year}
------------------------------
Nüfus     : {alive}
Erkek     : {aliveMales}
Kadın     : {aliveFemales}
Evli      : {married}
Dul       : {widowed}
Bekar     : {single}

Ölüm      : {deathsThisYear} (E:{maleDeaths}, K:{femaleDeaths})
Kaza      : {accidentThisYear}
Evlilik   : {marriagesThisYear}
Doğum     : {birthsThisYear}{birthDetails}
");
        }

        static void KillPerson(Person p, List<Person> all)
        {
            if (!p.IsAlive) return; // Already dead

            p.IsAlive = false;

            if (p.SpouseId is int spouseId)
            {
                var spouse = all.Find(x => x.Id == spouseId);
                if (spouse != null && spouse.IsAlive)
                {
                    spouse.MaritalStatus = MaritalStatus.Widowed;
                    spouse.SpouseId = null;
                }
                p.SpouseId = null; // Clear the deceased person's spouse reference too
            }
        }

        static void PrintSummary(List<Person> people)
        {
            int alive = people.Count(p => p.IsAlive);
            int married = people.Count(p => p.IsAlive && p.MaritalStatus == MaritalStatus.Married);
            int widowed = people.Count(p => p.IsAlive && p.MaritalStatus == MaritalStatus.Widowed);
            int single = alive - married - widowed;

            Console.WriteLine($"Nüfus={alive}, Evli={married}, Dul={widowed}, Bekar={single}");
        }

        static List<Person> CreateInitialPopulation(int n, Random rnd)
        {
            var people = new List<Person>(n);
            int males = n / 2;
            int females = n - males;

            for (int i = 0; i < males; i++)
            {
                people.Add(new Person
                {
                    Id = _nextId++,
                    Name = MaleNames[rnd.Next(MaleNames.Length)],
                    Age = rnd.Next(18, 60),
                    Gender = Gender.Male
                });
            }

            for (int i = 0; i < females; i++)
            {
                people.Add(new Person
                {
                    Id = _nextId++,
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
            if (age < 18) return 0.00;
            if (age <= 28) return 0.20; // %20
            if (age <= 40) return 0.12; // %12
            if (age <= 60) return 0.06; // %6
            return 0.00;
        }

        static int TryMatchMarriages(List<Person> people, Random rnd)
        {
            var candidates = people.Where(p => p.IsAlive && p.MaritalStatus != MaritalStatus.Married && p.Age >= 18 && p.Age <= 65).ToList();

            var males = candidates.Where(p => p.Gender == Gender.Male).OrderBy(_ => rnd.Next()).ToList();
            var females = candidates.Where(p => p.Gender == Gender.Female).OrderBy(_ => rnd.Next()).ToList();

            var usedFemaleIds = new HashSet<int>();
            int marriages = 0;

            foreach (var m in males)
            {
                double prob = MarriageProbabilityAge(m.Age);
                if (rnd.NextDouble() >= prob) continue;

                var match = females.FirstOrDefault(f => !usedFemaleIds.Contains(f.Id) && Math.Abs(f.Age - m.Age) <= 25);
                if (match == null) continue;

                double femaleProb = MarriageProbabilityAge(match.Age);
                if (rnd.NextDouble() >= femaleProb) continue;

                m.MaritalStatus = MaritalStatus.Married;
                m.SpouseId = match.Id;

                match.MaritalStatus = MaritalStatus.Married;
                match.SpouseId = m.Id;

                usedFemaleIds.Add(match.Id);
                marriages++;
            }

            return marriages;
        }

        static Person CreateNewborn(Random rnd, int motherId, int fatherId)
        {
            var gender = (rnd.NextDouble() < 0.5) ? Gender.Male : Gender.Female;

            string name = (gender == Gender.Male)
                ? MaleNames[rnd.Next(MaleNames.Length)]
                : FemaleNames[rnd.Next(FemaleNames.Length)];

            return new Person
            {
                Id = _nextId++,
                Name = name,
                Age = 0,
                Gender = gender,
                MotherId = motherId,
                FatherId = fatherId,
                MaritalStatus = MaritalStatus.Single,
                IsAlive = true,
                SpouseId = null
            };
        }

        static (int totalBirths, string birthDetails) TryBirths(List<Person> people, Random rnd)
        {
            const double birthProb = 0.25; // Base probability for having children
            const int maxChildrenPerYear = 3; // Maximum children per couple per year

            var births = new List<Person>();
            int singleBirths = 0;
            int twinBirths = 0;
            int tripletBirths = 0;

            var mothers = people.Where(p =>
                p.IsAlive &&
                p.MaritalStatus == MaritalStatus.Married &&
                p.Gender == Gender.Female &&
                p.Age >= 20 && p.Age <= 45).ToList();

            var usedCouple = new HashSet<(int motherId, int fatherId)>();

            foreach (var mother in mothers)
            {
                if (mother.SpouseId is not int fatherId) continue;

                var father = people.FirstOrDefault(x => x.Id == fatherId);
                if (father == null || !father.IsAlive) continue;

                if (father.Age < 20 || father.Age > 60) continue;

                var coupleKey = (mother.Id, father.Id);
                if (usedCouple.Contains(coupleKey)) continue;

                // Check if this couple will have children this year
                if (rnd.NextDouble() < birthProb)
                {
                    // Determine number of children (1-3, with decreasing probability)
                    int numChildren = 1; // At least 1 child

                    // 20% chance for twins (2 children)
                    if (rnd.NextDouble() < 0.20)
                    {
                        numChildren = 2;

                        // 5% chance for triplets (3 children) - only if already having twins
                        if (rnd.NextDouble() < 0.05)
                        {
                            numChildren = 3;
                        }
                    }

                    // Count the birth type
                    switch (numChildren)
                    {
                        case 1:
                            singleBirths++;
                            break;
                        case 2:
                            twinBirths++;
                            break;
                        case 3:
                            tripletBirths++;
                            break;
                    }

                    // Create the children
                    for (int i = 0; i < numChildren; i++)
                    {
                        var baby = CreateNewborn(rnd, mother.Id, father.Id);
                        births.Add(baby);
                    }

                    usedCouple.Add(coupleKey);
                }
            }

            if (births.Count > 0)
            {
                people.AddRange(births);
            }

            // Create birth details string
            string details = "";
            if (twinBirths > 0 || tripletBirths > 0)
            {
                details = " (";
                if (twinBirths > 0)
                    details += $"İkiz: {twinBirths}";
                if (tripletBirths > 0)
                {
                    if (twinBirths > 0) details += ", ";
                    details += $"Üçüz: {tripletBirths}";
                }
                details += ")";
            }

            return (births.Count, details);
        }
    }
}