using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace SocietySim
{
    enum Gender { Male, Female }
    enum MaritalStatus { Single, Married, Widowed }

    class ProbabilityBand
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double Probability { get; set; } // 0..1
    }

    class SimulationConfig
    {
        public List<string> MaleNames { get; set; } = new()
        { "Ahmet", "Mehmet", "Can", "Emre", "Burak", "Mert", "Kerem", "Ali", "Bora", "Onur" };
        public List<string> FemaleNames { get; set; } = new()
        { "Ayşe", "Elif", "Zeynep", "Naz", "Ece", "Melis", "Deniz", "Derya", "Sude", "İpek" };
        public List<ProbabilityBand> DeathBands { get; set; } = new();
        public double AccidentAnnualProbability { get; set; } = 0.001;
        public double AccidentFatalityProbability { get; set; } = 0.30;

        public List<ProbabilityBand> MarriageAgeBands { get; set; } = new();
        public int MarriageCandidateMinAge { get; set; } = 18;
        public int MarriageCandidateMaxAge { get; set; } = 65;
        public int MarriageMaxAgeGap { get; set; } = 25;

        public int BirthMotherMinAge { get; set; } = 20;
        public int BirthMotherMaxAge { get; set; } = 50;
        public int BirthFatherMinAge { get; set; } = 20;
        public int BirthFatherMaxAge { get; set; } = 60;
        public double BirthAnnualProbability { get; set; } = 0.50;
        public double TwinProbability { get; set; } = 0.20;
        public double TripletProbability { get; set; } = 0.05;
    }

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
        static SimulationConfig _cfg = new SimulationConfig();
        static string RandomName(Gender g, Random rnd)
        {
            var list = g == Gender.Male ? _cfg.MaleNames : _cfg.FemaleNames;

            if (list == null || list.Count == 0)
                throw new InvalidOperationException("Appsettings.json içinde isim listesi tanımlı değil!");

            return list[rnd.Next(list.Count)];
        }

        static void LoadConfig(string path = "appsettings.json")
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Uyarı: {path} bulunamadı, varsayılan ayarlar kullanılacak.");
                    // Varsayılan ayarları yükle
                    LoadDefaultConfig();
                    return;
                }

                var json = File.ReadAllText(path);
                var cfg = JsonSerializer.Deserialize<SimulationConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

                if (cfg != null)
                {
                    _cfg = cfg;
                    Console.WriteLine("Ayarlar yüklendi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ayarlar yüklenemedi: " + ex.Message);
                LoadDefaultConfig();
            }
        }

        static void LoadDefaultConfig()
        {
            // Varsayılan yapılandırmayı yükle
            _cfg.DeathBands = new List<ProbabilityBand>
            {
                new() { MinAge = 0, MaxAge = 39, Probability = 0.001 },
                new() { MinAge = 40, MaxAge = 59, Probability = 0.005 },
                new() { MinAge = 60, MaxAge = 74, Probability = 0.02 },
                new() { MinAge = 75, MaxAge = 89, Probability = 0.06 },
                new() { MinAge = 90, MaxAge = 200, Probability = 0.12 }
            };

            _cfg.MarriageAgeBands = new List<ProbabilityBand>
            {
                new() { MinAge = 18, MaxAge = 28, Probability = 0.20 },
                new() { MinAge = 29, MaxAge = 40, Probability = 0.12 },
                new() { MinAge = 41, MaxAge = 60, Probability = 0.06 }
            };
        }

        static int _nextId = 1;

        static void Main()
        {
            LoadConfig();
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
                    var (deaths, accidents, marriages, births, maleDeaths, femaleDeaths, birthDetails, yearLog)
                        = AdvanceOneYear(people, rnd, currentYear);

                    PrintYearSummary(people, currentYear, deaths, accidents, marriages, births, maleDeaths, femaleDeaths, birthDetails, yearLog);
                    i++;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Döngüden çıkılıyor...");
                    break;
                }
            }
        }

        static (int deathsThisYear, int accidentsThisYear, int marriagesThisYear, int birthsThisYear, int maleDeathsThisYear, int femaleDeathsThisYear, string birthDetails, List<string> yearLog) AdvanceOneYear(List<Person> people, Random rnd, int currentYear)
        {
            int deathsThisYear = 0;
            int accidentsThisYear = 0;
            int marriagesThisYear = 0;
            int birthsThisYear = 0;
            int maleDeathsThisYear = 0;
            int femaleDeathsThisYear = 0;
            string birthDetails = "";
            var yearLog = new List<string>();

            // 1) Yaşlandır
            foreach (var p in people)
            {
                if (!p.IsAlive) continue;
                p.Age += 1;
            }

            // 2) Evlilik (önce eşleşmeleri al, logu burada yazacağız)
            var (marriages, pairs) = TryMatchMarriages(people, rnd);
            marriagesThisYear = marriages;

            foreach (var (maleId, femaleId) in pairs)
            {
                yearLog.Add($"{currentYear}: {TagById(people, maleId)} ile {TagById(people, femaleId)} evlendi.");
            }

            // 3) Doğum (evlilikten sonra)
            var (bCount, details, triples) = TryBirths(people, rnd);
            birthsThisYear = bCount;
            birthDetails = details;

            foreach (var (babyId, motherId, fatherId) in triples)
            {
                yearLog.Add($"{currentYear}: {TagById(people, babyId)} doğdu — ebeveynler: {TagById(people, motherId)} & {TagById(people, fatherId)}.");
            }

            var alivePeople = people.Where(p => p.IsAlive).ToList();

            // 4) Kaza
            foreach (var p in alivePeople.ToList()) // ToList() ile kopyalayarak güvenli iterasyon
            {
                if (!p.IsAlive) continue; // Önceki adımlarda ölmüş olabilir

                double accidentProb = _cfg.AccidentAnnualProbability;
                if (rnd.NextDouble() < accidentProb)
                {
                    accidentsThisYear++;
                    if (rnd.NextDouble() < _cfg.AccidentFatalityProbability)
                    {
                        yearLog.Add($"{currentYear}: {Tag(p)} ölümcül bir kaza geçirdi ve vefat etti.");
                        KillPerson(p, people);
                        deathsThisYear++;
                        if (p.Gender == Gender.Male) maleDeathsThisYear++; else femaleDeathsThisYear++;
                    }
                    else
                    {
                        yearLog.Add($"{currentYear}: {Tag(p)} kaza geçirdi ama hayatta.");
                    }
                }
            }

            // 5) Ölüm (yaşlılıktan)
            alivePeople = people.Where(p => p.IsAlive).ToList();
            foreach (var p in alivePeople)
            {
                double deathProb = DeathProbabilityByAge(p.Age);
                if (rnd.NextDouble() < deathProb)
                {
                    yearLog.Add($"{currentYear}: {Tag(p)} vefat etti (yaş {p.Age}).");
                    KillPerson(p, people);
                    deathsThisYear++;
                    if (p.Gender == Gender.Male) maleDeathsThisYear++; else femaleDeathsThisYear++;
                }
            }

            return (deathsThisYear, accidentsThisYear, marriagesThisYear, birthsThisYear, maleDeathsThisYear, femaleDeathsThisYear, birthDetails, yearLog);
        }

        static void PrintYearSummary(List<Person> people, int year, int deathsThisYear, int accidentThisYear, int marriagesThisYear, int birthsThisYear, int maleDeaths, int femaleDeaths, string birthDetails, List<string> yearLog)
        {
            int aliveMales = people.Count(p => p.IsAlive && p.Gender == Gender.Male);
            int aliveFemales = people.Count(p => p.IsAlive && p.Gender == Gender.Female);
            int alive = aliveMales + aliveFemales;
            int married = people.Count(p => p.IsAlive && p.MaritalStatus == MaritalStatus.Married);
            int widowed = people.Count(p => p.IsAlive && p.MaritalStatus == MaritalStatus.Widowed);
            int single = alive - married - widowed;

            /* DEBUG BİLGİSİ: Doğum için uygun çiftleri göster
            var eligibleMothers = people.Count(p =>
                p.IsAlive &&
                p.MaritalStatus == MaritalStatus.Married &&
                p.Gender == Gender.Female &&
                p.Age >= _cfg.BirthMotherMinAge &&
                p.Age <= _cfg.BirthMotherMaxAge);  */

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

            if (yearLog != null && yearLog.Count > 0)
            {
                Console.WriteLine("Olay Günlüğü:");
                foreach (var line in yearLog)
                    Console.WriteLine(" - " + line);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Bu yıl kayda değer bir olay yok.\n");
            }
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
                    Name = RandomName(Gender.Male, rnd),
                    Age = rnd.Next(18, 30),
                    Gender = Gender.Male
                });
            }

            for (int i = 0; i < females; i++)
            {
                people.Add(new Person
                {
                    Id = _nextId++,
                    Name = RandomName(Gender.Female, rnd),
                    Age = rnd.Next(18, 30),
                    Gender = Gender.Female
                });
            }

            return people;
        }

        static double DeathProbabilityByAge(int age)
        {
            var band = _cfg.DeathBands.FirstOrDefault(b => age >= b.MinAge && age <= b.MaxAge);
            return band?.Probability ?? 0.001;
        }

        static double MarriageProbabilityAge(int age)
        {
            var band = _cfg.MarriageAgeBands.FirstOrDefault(b => age >= b.MinAge && age <= b.MaxAge);
            return band?.Probability ?? 0.0;
        }

        static (int marriages, List<(int maleId, int femaleId)>) TryMatchMarriages(List<Person> people, Random rnd)
        {
            var candidates = people
                .Where(p => p.IsAlive
             && p.MaritalStatus != MaritalStatus.Married
             && p.Age >= _cfg.MarriageCandidateMinAge
             && p.Age <= _cfg.MarriageCandidateMaxAge)
                .ToList();

            var males = candidates.Where(p => p.Gender == Gender.Male).OrderBy(_ => rnd.Next()).ToList();
            var females = candidates.Where(p => p.Gender == Gender.Female).OrderBy(_ => rnd.Next()).ToList();

            var usedFemaleIds = new HashSet<int>();
            int marriages = 0;
            var pairs = new List<(int maleId, int femaleId)>();

            foreach (var m in males)
            {
                double prob = MarriageProbabilityAge(m.Age);
                if (rnd.NextDouble() >= prob) continue;

                var match = females.FirstOrDefault(f =>
                    !usedFemaleIds.Contains(f.Id) &&
                    Math.Abs(f.Age - m.Age) <= _cfg.MarriageMaxAgeGap);

                if (match == null) continue;

                double femaleProb = MarriageProbabilityAge(match.Age);
                if (rnd.NextDouble() >= femaleProb) continue;

                m.MaritalStatus = MaritalStatus.Married;
                m.SpouseId = match.Id;

                match.MaritalStatus = MaritalStatus.Married;
                match.SpouseId = m.Id;

                usedFemaleIds.Add(match.Id);
                marriages++;
                pairs.Add((m.Id, match.Id));
            }

            return (marriages, pairs);
        }

        static Person CreateNewborn(Random rnd, int motherId, int fatherId)
        {
            var gender = (rnd.NextDouble() < 0.5) ? Gender.Male : Gender.Female;

            string name = (gender == Gender.Male)
                ? _cfg.MaleNames[rnd.Next(_cfg.MaleNames.Count)]
                : _cfg.FemaleNames[rnd.Next(_cfg.FemaleNames.Count)];

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

        static (int totalBirths, string birthDetails, List<(int babyId, int motherId, int fatherId)>) TryBirths(List<Person> people, Random rnd)
        {
            var births = new List<Person>();
            int singleBirths = 0;
            int twinBirths = 0;
            int tripletBirths = 0;

            var birthTriples = new List<(int babyId, int motherId, int fatherId)>();

            var mothers = people.Where(p =>
                p.IsAlive &&
                p.MaritalStatus == MaritalStatus.Married &&
                p.Gender == Gender.Female &&
                p.Age >= _cfg.BirthMotherMinAge &&
                p.Age <= _cfg.BirthMotherMaxAge).ToList();

            //Console.WriteLine($"DEBUG: Doğum için uygun anne sayısı: {mothers.Count}");

            foreach (var mother in mothers)
            {
                if (mother.SpouseId is not int fatherId) continue;

                var father = people.FirstOrDefault(x => x.Id == fatherId);
                if (father == null || !father.IsAlive) continue;
                if (father.Age < _cfg.BirthFatherMinAge || father.Age > _cfg.BirthFatherMaxAge) continue;

                //Console.WriteLine($"DEBUG: Anne {mother.Name} ({mother.Age}) ve Baba {father.Name} ({father.Age}) doğum ihtimali kontrol ediliyor...");

                if (rnd.NextDouble() < _cfg.BirthAnnualProbability)
                {
                    int numChildren = 1;

                    // İkiz/üçüz hesaplama düzeltildi - bağımsız kontroller
                    double rand1 = rnd.NextDouble();
                    double rand2 = rnd.NextDouble();

                    if (rand2 < _cfg.TripletProbability)
                        numChildren = 3; // Üçüz
                    else if (rand1 < _cfg.TwinProbability)
                        numChildren = 2; // İkiz

                    //Console.WriteLine($"DEBUG: {numChildren} çocuk doğacak!");

                    switch (numChildren)
                    {
                        case 1: singleBirths++; break;
                        case 2: twinBirths++; break;
                        case 3: tripletBirths++; break;
                    }

                    for (int i = 0; i < numChildren; i++)
                    {
                        var baby = CreateNewborn(rnd, mother.Id, father.Id);
                        births.Add(baby);
                        birthTriples.Add((baby.Id, mother.Id, father.Id));
                        //Console.WriteLine($"DEBUG: {baby.Name} ({baby.Gender}) doğdu!");
                    }
                }
            }

            if (births.Count > 0)
                people.AddRange(births);

            string details = "";
            if (twinBirths > 0 || tripletBirths > 0)
            {
                details = " (";
                if (twinBirths > 0) details += $"İkiz: {twinBirths}";
                if (tripletBirths > 0)
                {
                    if (twinBirths > 0) details += ", ";
                    details += $"Üçüz: {tripletBirths}";
                }
                details += ")";
            }

            return (births.Count, details, birthTriples);
        }

        static string Tag(Person p)
            => $"#{p.Id} {p.Name} ({(p.Gender == Gender.Male ? "E" : "K")}, {p.Age})";

        static string TagById(List<Person> people, int id)
        {
            var p = people.FirstOrDefault(x => x.Id == id);
            return p != null ? Tag(p) : $"#{id} (?)";
        }
    }
}

//Classlar çıplak -- interface yok, kodu modüler hale getir, parçala, ana canlı classı yap üzerinden hayvan da üreticez, reflection bak!, reflectionla var sorgusu çekmek, o tiptekileri buna cast et