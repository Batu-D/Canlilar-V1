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
    internal class BirthService
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
        private (Gender gender, string name) GenerateGenderAndName()
        {
            var gender = _random.NextDouble() < 0.5 ? Gender.Male : Gender.Female;
            var nameList = gender == Gender.Male ? _config.MaleNames : _config.FemaleNames;
            var name = nameList[_random.Next(nameList.Count)];
            return (gender, name);
        }
        private Human CreateHumanBaby(int motherId, int fatherId)
        {
            var (gender, name) = GenerateGenderAndName();

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
            var (gender, name) = GenerateGenderAndName();

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

            foreach (var female in eligibleFemales)
            {
                //Aynı türden bir erkek bul
                var male = eligibleMales.FirstOrDefault(m => m.Species == female.Species);
                if (male == null) continue;

                //Doğum şansı kontrolü
                if (_random.NextDouble() < _config.BirthAnnualProbability)
                {
                    int babyCount = DetermineBabyCount();

                    for (int i = 0; i < babyCount; i++)
                    {
                        var baby = CreateAnimalBaby(female.Id, male.Id, female.Species);
                        babies.Add(baby);
                    }
                }
            }
            return babies;
        }

        //Reflection ile generic bebek oluşturma
        /*private T CreateNewBorn<T>(int motherId, int fatherId) where T : ILivingBeing
        {
            Type tpye = typeof(T);

            //Random cinsiyet
            var gender = _random.NextDouble() < 0.5 ? Gender.Male : Gender.Female;

            //Random isim
            var nameList = gender == Gender.Male ? _config.MaleNames : _config.FemaleNames;
            var name = nameList[_random.Next(nameList.Count)];

            //Constructor parametreleri
            object[] parameters;

            if(type == typeof(Human))
            {
                parameters = new object[] { _nextId++, name, 0, gender};
            }
            else if (type == typeof(Animal))
            {
                //Animal için gerektli species bilgisini almak için
                //CreateAnimalBaby ayrı metod olarak kaldı
                parameters = new object[] { _nextId++, name, 0, gender, "Unknown" };
            }
            else
            {
                throw new NotSupportedException($"Tip {typoe.Name} desteklenmiyor");
            }

            var constructor = type.GetConstructor
                (new[] { typeof(int), typeof(string), typeof(int), typeof(Gender) });
            if (constructor == null)
                throw new InvalidOperationException($"{type.Name} için uygun contrustor bulunmadı ");

            var instance = (T)constructor.Invoke(parameters);

            //MotherId ve FatherId ata (reflection ile)
            var motherIdProp = type.GetProperty("MotherId");
            var fatherIdProp = type.GetProperty("FatherId");

            motherIdProp?.SetValue(instance, motherId);
            fatherIdProp?.SetValue(instance, fatherId);

            return instance;
        }*/
    }
}
