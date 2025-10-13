using ConsoleApp1.Enums;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Models;
using SocietySim;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1.Service
{
    internal class BirthService
    {
        private SimulationConfig _config;
        private Random _random;

        public BirthService(SimulationConfig config, Random random)
        {
            _config = config;
            _random = random;
        }

        // ✅ ref int nextId parametresi eklendi
        public List<ILivingBeing> ProcessBirths(List<ILivingBeing> beings, ref int nextId)
        {
            var newborns = new List<ILivingBeing>();

            //İnsan Bebekleri
            var humans = beings.OfType<Human>().ToList();
            newborns.AddRange(ProcessHumanBirths(humans, ref nextId));

            //Animal Bebekleri
            var animals = beings.OfType<Animal>().ToList();
            newborns.AddRange(ProcessAnimalBirths(animals, ref nextId));

            return newborns;
        }

        public List<Human> ProcessHumanBirths(List<Human> humans, ref int nextId)
        {
            var babies = new List<Human>();

            //Koşulları sağlayan anneleri bul
            var eligibleMothers = humans.Where(x => x.IsAlive
            && x.MaritalStatus == MaritalStatus.Married
            && x.Gender == Gender.Female
            && x.CanReproduce()
            ).ToList();

            foreach (var mother in eligibleMothers)
            {
                if (mother.SpouseId == null) continue;

                //Babayı bulma
                var father = humans.FirstOrDefault(h => h.Id == mother.SpouseId.Value);
                if (father == null || !father.IsAlive || !father.CanReproduce()) continue;

                //Doğum Şansı Kontrolü
                if (_random.NextDouble() < _config.BirthChance)
                {
                    //Kaç bebek doğacak?
                    int babyCount = DetermineBabyCount();

                    for (int i = 0; i < babyCount; i++)
                    {
                        var baby = CreateHumanBaby(mother.Id, father.Id, ref nextId);
                        babies.Add(baby);
                    }
                }
            }
            return babies;
        }

        public List<Animal> ProcessAnimalBirths(List<Animal> animals, ref int nextId)
        {
            var babies = new List<Animal>();

            //Uygun Dişi
            var eligibleFemales = animals.Where(a =>
            a.Gender == Gender.Female
            && a.IsAlive
            && a.CanReproduce()
            ).ToList();

            //Uygun Erkek
            var eligibleMales = animals.Where(a =>
            a.Gender == Gender.Male
            && a.IsAlive
            && a.CanReproduce()
            ).ToList();

            foreach (var female in eligibleFemales)
            {
                //Aynı türden bir erkek bul
                var male = eligibleMales.FirstOrDefault(m => m.Species == female.Species);
                if (male == null) continue;

                //Doğum şansı kontrolü
                if (_random.NextDouble() < _config.AnimalBreedChance)
                {
                    int babyCount = DetermineBabyCount();

                    for (int i = 0; i < babyCount; i++)
                    {
                        var baby = CreateAnimalBaby(female.Id, male.Id, female.Species, ref nextId);
                        babies.Add(baby);
                    }
                }
            }
            return babies;
        }

        private int DetermineBabyCount()
        {
            double rand = _random.NextDouble();

            // Varsayılan değerler (eğer config'de yoksa)
            double tripletProb = 0.05;
            double twinProb = 0.20;

            // Config'den al (eğer varsa)
            var configType = _config.GetType();
            var tripletProp = configType.GetProperty("TripletProbability");
            var twinProp = configType.GetProperty("TwinProbability");

            if (tripletProp != null)
                tripletProb = (double)(tripletProp.GetValue(_config) ?? 0.05);
            if (twinProp != null)
                twinProb = (double)(twinProp.GetValue(_config) ?? 0.20);

            if (rand < tripletProb)
                return 3;
            else if (rand < twinProb)
                return 2;
            else
                return 1;
        }

        private (Gender gender, string name) GenerateGenderAndName<T>(string? species = null)
            where T : ILivingBeing
        {
            var gender = _random.NextDouble() < 0.5 ? Gender.Male : Gender.Female;
            List<string> nameList;

            if (typeof(T) == typeof(Human))
            {
                // İnsan: cinsiyete göre listeden seç
                nameList = (gender == Gender.Male) ? _config.MaleNames : _config.FemaleNames;
            }
            else if (typeof(T) == typeof(Animal))
            {
                // Hayvan: tür bazlı varsa onu, yoksa genel hayvan isimleri
                nameList =
                    (_config.AnimalNamesBySpecies != null && species != null &&
                     _config.AnimalNamesBySpecies.TryGetValue(species, out var bySpecies) && bySpecies?.Count > 0)
                        ? bySpecies!
                        : _config.AnimalNames;
            }
            else
            {
                throw new NotSupportedException($"İsim üretimi {typeof(T).Name} için tanımlı değil.");
            }

            if (nameList == null || nameList.Count == 0)
                throw new InvalidOperationException($"{typeof(T).Name} için isim listesi boş!");

            var name = nameList[_random.Next(nameList.Count)];
            return (gender, name);
        }

        private Human CreateHumanBaby(int motherId, int fatherId, ref int nextId)
        {
            var (gender, name) = GenerateGenderAndName<Human>();

            var baby = new Human(nextId++, name, 0, gender)
            {
                MotherId = motherId,
                FatherId = fatherId,
                MaritalStatus = MaritalStatus.Single
            };
            return baby;
        }

        private Animal CreateAnimalBaby(int motherId, int fatherId, string species, ref int nextId)
        {
            var (gender, name) = GenerateGenderAndName<Animal>(species);

            var baby = new Animal(nextId++, name, 0, gender, species)
            {
                MotherId = motherId,
                FatherId = fatherId,
            };
            return baby;
        }
    }
}