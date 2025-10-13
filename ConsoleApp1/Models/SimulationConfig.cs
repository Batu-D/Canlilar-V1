using System.Collections.Generic;

namespace SocietySim
{
    public class SimulationConfig
    {
        // ========== İSİM LİSTELERİ ==========
        public List<string> MaleNames { get; set; } = new();
        public List<string> FemaleNames { get; set; } = new();
        public List<string> AnimalNames { get; set; } = new();
        public Dictionary<string, List<string>> AnimalNamesBySpecies { get; set; } = new();

        // ========== EVLİLİK AYARLARI ==========
        public int MinMarriageAge { get; set; } = 18;
        public int MaxMarriageAge { get; set; } = 40;
        public double MarriageChance { get; set; } = 0.3;

        // Evlilik için yaş aralıkları ve olasılıkları
        public List<MarriageAgeBand> MarriageAgeBands { get; set; } = new();
        public int MarriageCandidateMinAge { get; set; } = 18;
        public int MarriageCandidateMaxAge { get; set; } = 65;
        public int MarriageMaxAgeGap { get; set; } = 25;

        // ========== DOĞUM AYARLARI (İNSAN) ==========
        public int MinChildBirthAge { get; set; } = 18;
        public int MaxChildBirthAge { get; set; } = 45;
        public double BirthChance { get; set; } = 0.4;
        public double BirthAnnualProbability { get; set; } = 0.50;

        // Anne ve baba yaş sınırları
        public int BirthMotherMinAge { get; set; } = 18;
        public int BirthMotherMaxAge { get; set; } = 50;
        public int BirthFatherMinAge { get; set; } = 18;
        public int BirthFatherMaxAge { get; set; } = 70;

        // Çoğul doğum olasılıkları
        public double TwinProbability { get; set; } = 0.20;
        public double TripletProbability { get; set; } = 0.05;

        // ========== DOĞUM AYARLARI (HAYVAN) ==========
        public int MinAnimalBreedAge { get; set; } = 2;
        public int MaxAnimalBreedAge { get; set; } = 10;
        public double AnimalBreedChance { get; set; } = 0.5;
        public double AnimalBreedAnnualProbability { get; set; } = 0.5;

        // ========== ÖLÜM AYARLARI ==========
        public int MaxHumanAge { get; set; } = 80;
        public int MaxAnimalAge { get; set; } = 15;

        // Yaş bandlarına göre doğal ölüm olasılıkları
        public List<DeathBand> DeathBands { get; set; } = new();

        // Kaza ayarları
        public double AccidentChance { get; set; } = 0.02;
        public double AccidentAnnualProbability { get; set; } = 0.01;
        public double AccidentFatalityProbability { get; set; } = 0.30;
        public List<string> AccidentTypes { get; set; } = new();
    }

    // ========== HELPER CLASSES ==========

    /// <summary>
    /// Yaş aralığına göre ölüm olasılığı
    /// </summary>
    public class DeathBand
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double Probability { get; set; }
    }

    /// <summary>
    /// Yaş aralığına göre evlilik olasılığı
    /// </summary>
    public class MarriageAgeBand
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double Probability { get; set; }
    }
}