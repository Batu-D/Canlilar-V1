using ConsoleApp1.Interfaces;
using ConsoleApp1.Models;
using SocietySim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleApp1.Service
{
    public class SimulationManager
    {
        private readonly SimulationConfig _config;
        private readonly object _syncRoot = new();
        private readonly TimeSpan _interval;

        private SimulationEngine _engine;
        private List<ILivingBeing> _beings = new();
        private List<SimulationYearResult> _history = new();
        private Timer? _timer;
        private bool _isRunning;
        private int _startYear;
        private int _currentYear;

        private int _defaultHumanCount = 100;
        private int _defaultAnimalCount = 20;

        public event EventHandler<SimulationStateSnapshot>? StateChanged;
        public event EventHandler<SimulationYearUpdate>? YearAdvanced;

        public SimulationManager(SimulationConfig config)
            : this(config, TimeSpan.FromSeconds(2))
        {
        }

        public SimulationManager(SimulationConfig config, TimeSpan interval)
        {
            _config = config;
            _interval = interval;
            _engine = new SimulationEngine(_config);

            ResetInternal(_defaultHumanCount, _defaultAnimalCount, 2025);
        }

        public SimulationStateSnapshot GetSnapshot()
        {
            lock (_syncRoot)
            {
                return BuildSnapshot();
            }
        }

        public void Start()
        {
            lock (_syncRoot)
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;
                _timer = new Timer(OnTimerTick, null, TimeSpan.Zero, _interval);
                NotifyStateChanged();
            }
        }

        public void Pause()
        {
            lock (_syncRoot)
            {
                PauseInternal();
            }
        }

        public void Reset(int? humans = null, int? animals = null, int startYear = 2025)
        {
            lock (_syncRoot)
            {
                PauseInternal();
                ResetInternal(humans ?? _defaultHumanCount, animals ?? _defaultAnimalCount, startYear);
                NotifyStateChanged();
            }
        }

        public SimulationYearResult? Step()
        {
            lock (_syncRoot)
            {
                PauseInternal();
                return AdvanceOneYearInternal();
            }
        }

        private void OnTimerTick(object? state)
        {
            lock (_syncRoot)
            {
                if (!_isRunning)
                {
                    return;
                }

                AdvanceOneYearInternal();
            }
        }

        private SimulationYearResult? AdvanceOneYearInternal()
        {
            if (_history.Count == 0)
            {
                return null;
            }

            if (_beings.Count(b => b.IsAlive) == 0)
            {
                PauseInternal();
                return null;
            }

            _currentYear++;

            var result = _engine.AdvanceOneYear(_beings, _currentYear);
            _history.Add(result);

            if (result.TotalPopulation == 0)
            {
                PauseInternal();
            }

            YearAdvanced?.Invoke(this, new SimulationYearUpdate
            {
                YearResult = result,
                IsRunning = _isRunning,
                CurrentYear = _currentYear
            });

            return result;
        }

        private void PauseInternal()
        {
            _timer?.Dispose();
            _timer = null;

            if (_isRunning)
            {
                _isRunning = false;
                NotifyStateChanged();
            }
        }

        private void ResetInternal(int humans, int animals, int startYear)
        {
            _engine = new SimulationEngine(_config);
            _beings = _engine.CreateInitialPopulation(humans, animals);
            _history = new List<SimulationYearResult>();

            _startYear = startYear;
            _currentYear = startYear;

            var snapshot = _engine.CreateSnapshot(_beings, _startYear);
            _history.Add(snapshot);
        }

        private SimulationStateSnapshot BuildSnapshot()
        {
            return new SimulationStateSnapshot
            {
                StartYear = _startYear,
                CurrentYear = _currentYear,
                IsRunning = _isRunning,
                History = new List<SimulationYearResult>(_history)
            };
        }

        private void NotifyStateChanged()
        {
            var snapshot = BuildSnapshot();
            StateChanged?.Invoke(this, snapshot);
        }
    }
}
