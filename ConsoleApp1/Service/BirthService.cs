using ConsoleApp1.Enums;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Models;
using SocietySim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1.Service
{
    public class BirthService
    {
        private SimulationConfig _config;
        private Random _random;
        private int _nextId;

        public BirthService(SimulationConfig config, Random random, int startId)
        {
            _config = config;
            _random = random;
            _nextId = startId;
        }
        public List<ILivingBeing>ProcessBirths(List<ILivingBeing> beings)
        {
            var newborns = new List<ILivingBeing>();

            //İnsan Bebekleri
            var humans = beings.OfType<Human>().ToList();
            newborns.AddRange(ProcessHumanBirths(humans));

            //Animal Bebekleri
            var animals = beings.OfType<Animal>().ToList();
            newborns.AddRange(ProcessAnimalBirths(animals));

            return newborns;
        }
        public List<Human> ProcessHumanBirths(List<Human> humans)
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
                if (_random.NextDouble() < _config.BirthAnnualProbability)
                {
                    //Kaç bebek doğacak?
                    int babyCount = DetermineBabyCount();

                    for (int i = 0; i < babyCount; i++)
                    {
                        var baby = CreateHumanBaby(mother.Id, father.Id);
                        babies.Add(baby);
                    }
                }
            }
            return babies;
        }

        private int DetermineBabyCount()
        {
            double rand = _random.NextDouble();

            if (rand < _config.TripletProbability)
                return 3;
            else if (rand < _config.TwinProbability)
                return 2;
            else
                return 1;
        }
        private Human CreateHumanBaby(int motherId, int fatherId)
        {
            var gender = _random.NextDouble() < 0.5 ? Gender.Male : Gender.Female;
            var nameList = gender == Gender.Male ? _config.MaleNames : _config.FemaleNames;
            var name = nameList[_random.Next(nameList.Count)];

            var baby = new Human(_nextId++, name, 0, gender)
            {
                MotherId = motherId,
                FatherId = fatherId,
                MaritalStatus = MaritalStatus.Single
            };
            return baby;
        }
        private Animal CreateAnimalBaby(int motherId, int fatherId, string species)
        {
            var gender = _random.NextDouble() < 0.5 ? Gender.Male: Gender.Female;
            var nameList = gender == Gender.Male ? _config.MaleNames : _config.FemaleNames;
            var name = nameList[_random.Next(nameList.Count())];

            var baby = new Animal(_nextId++, name, 0, gender, species)
            {
                MotherId = motherId,
                FatherId = fatherId,
            };
            return baby;
        }
        public List<Animal> ProcessAnimalBirths(List<Animal> animals)
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

            foreach(var female in eligibleFemales)
            {
                //Aynı türden bir erkek bul
                var male = eligibleMales.FirstOrDefault(m => m.Species == female.Species);
                if (male == null) continue;

                //Doğum şansı kontrolü
                if(_random.NextDouble() < _config.BirthAnnualProbability)
                {
                    int babyCount = DetermineBabyCount();

                    for(int i= 0; i < babyCount; i++)
                    {
                        var baby = CreateAnimalBaby(female.Id, male.Id, female.Species);
                        babies.Add(baby);
                    }
                }
            }
            return babies;
        }
        private T CreateNewBorn<T>(int motherId, int fatherId) where T : ILivingBeing
        {
            Type tpye = typeof(T);

            var gender = _random.NextDouble() < 0.5 ? Gender.Male : Gender.Female;
        }
    }
}
