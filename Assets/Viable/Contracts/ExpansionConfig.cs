using System;
using System.Collections.Generic;

namespace Viable.Contracts
{
    /// <summary>
    /// Configuration for expansion model dynamics.
    /// Scientists can customize these parameters to model different physical systems.
    /// </summary>
    [Serializable]
    public class ExpansionConfig
    {
        /// <summary>Base expansion model to use.</summary>
        public ExpansionModel Model = ExpansionModel.DefaultViabilityBoundaryPressure;

        /// <summary>Base formation probability per eligible cell per tick. Range: [0, 1].</summary>
        public double BaseFormationRate = 0.1;

        /// <summary>Minimum tick before sink formation is allowed.</summary>
        public int FormationDelayTicks = 10;

        /// <summary>Whether sinks can merge when adjacent.</summary>
        public bool AllowMerging = true;

        /// <summary>Whether sinks can shrink/evaporate.</summary>
        public bool AllowShrinkage = false;

        // Cosmological parameters
        public int InflationPeakTick = 5;
        public double InflationDecayRate = 0.1;
        public int BubbleRadius = 1;
        public int DarkEnergyOnsetTick = 50;
        public double AccelerationFactor = 0.01;
        public int CyclePeriod = 100;
        public double ContractionStrength = 0.3;
        public double BounceElasticity = 0.9;
        public double PhaseOffset = 0.0;
        public double SteadyStateCreationRate = 0.01;

        // Biological parameters
        public double LogisticGrowthRate = 0.5;
        public double CarryingCapacity = 0.3;
        public double NecroticCoreThreshold = 0.0;
        public double ActivatorRate = 1.0;
        public double InhibitorRate = 2.0;
        public int PatternWavelength = 10;
        public double BranchingProbability = 0.1;
        public double GradientSensitivity = 0.8;
        public int AvoidanceRadius = 3;

        // Fluid dynamics parameters
        public double ViscosityContrast = 10.0;
        public double CapillaryNumber = 0.1;
        public int FingerWidth = 3;
        public double StickingProbability = 1.0;
        public double DiffusionBias = 0.0;

        // Statistical physics parameters
        public double OccupationProbability = 0.5;
        public double Temperature = 2.0;
        public double ExternalField = 0.0;
        public double CouplingStrength = 1.0;
        public int CriticalSlope = 4;

        /// <summary>Custom parameters for research use.</summary>
        public Dictionary<string, double> AdvancedParams = new Dictionary<string, double>();

        /// <summary>Get effective formation rate at given tick based on model.</summary>
        public double GetFormationRate(int tick, int totalCells, int currentSinkCount)
        {
            if (tick < FormationDelayTicks)
                return 0.0;

            switch (Model)
            {
                case ExpansionModel.DefaultViabilityBoundaryPressure:
                case ExpansionModel.ViabilityPure:
                case ExpansionModel.ViabilityHysteresis:
                    return BaseFormationRate;

                case ExpansionModel.Inflationary:
                    return GetInflationaryRate(tick);

                case ExpansionModel.DarkEnergyAccelerating:
                    return GetDarkEnergyRate(tick);

                case ExpansionModel.Cyclic:
                    return GetCyclicRate(tick);

                case ExpansionModel.SteadyState:
                    return SteadyStateCreationRate;

                case ExpansionModel.LogisticGrowth:
                    return GetLogisticRate(currentSinkCount, totalCells);

                default:
                    return BaseFormationRate;
            }
        }

        /// <summary>Get current expansion phase based on model and tick.</summary>
        public ExpansionPhase GetCurrentPhase(int tick)
        {
            if (tick < FormationDelayTicks)
                return ExpansionPhase.Initialization;

            switch (Model)
            {
                case ExpansionModel.Inflationary:
                    if (tick < InflationPeakTick)
                        return ExpansionPhase.EarlyExpansion;
                    return ExpansionPhase.Deceleration;

                case ExpansionModel.Cyclic:
                    double phase = ((tick + PhaseOffset * CyclePeriod) % CyclePeriod) / CyclePeriod;
                    if (phase < 0.5)
                        return ExpansionPhase.MainExpansion;
                    return ExpansionPhase.Contraction;

                default:
                    return ExpansionPhase.MainExpansion;
            }
        }

        private double GetInflationaryRate(int tick)
        {
            int t = tick - FormationDelayTicks;
            if (t <= 0) return 0.0;

            if (t < InflationPeakTick)
                return BaseFormationRate * (1.0 - Math.Exp(-t * 0.5));
            else
            {
                int tDecay = t - InflationPeakTick;
                return BaseFormationRate * Math.Exp(-InflationDecayRate * tDecay);
            }
        }

        private double GetDarkEnergyRate(int tick)
        {
            if (tick < DarkEnergyOnsetTick)
                return BaseFormationRate * 0.2;
            else
            {
                int tAccel = tick - DarkEnergyOnsetTick;
                return BaseFormationRate * 0.2 * Math.Exp(AccelerationFactor * tAccel);
            }
        }

        private double GetCyclicRate(int tick)
        {
            int t = tick - FormationDelayTicks;
            if (t <= 0) return 0.0;

            double phase = 2.0 * Math.PI * (t + PhaseOffset * CyclePeriod) / CyclePeriod;
            double modulation = 0.5 * (1.0 + Math.Cos(phase));
            return BaseFormationRate * modulation;
        }

        private double GetLogisticRate(int currentSinkCount, int totalCells)
        {
            double density = (double)currentSinkCount / totalCells;
            if (density >= CarryingCapacity)
                return 0.0;
            return LogisticGrowthRate * density * (1.0 - density / CarryingCapacity) + 0.01;
        }

        public ExpansionConfig Clone()
        {
            var clone = (ExpansionConfig)MemberwiseClone();
            clone.AdvancedParams = new Dictionary<string, double>(AdvancedParams);
            return clone;
        }
    }
}
