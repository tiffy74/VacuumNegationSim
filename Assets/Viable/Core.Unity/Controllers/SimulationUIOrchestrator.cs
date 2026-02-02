using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Viable.Contracts;
using Viable.Core.Unity.Configuration;
using Viable.Core.Unity.UI;

namespace Viable.Core.Unity.Controllers
{
    /// <summary>
    /// Central orchestrator for UI ? Engine integration.
    /// Holds the single source of truth (WorkingScenarioConfig) and coordinates:
    /// - Preset ? WorkingConfig ? UI Refresh
    /// - UI edits ? WorkingConfig updates
    /// - Apply & Restart ? SimulationController.RestartWithScenario(...)
    /// 
    /// STRICT DESIGN:
    /// - All UI controllers write to THIS orchestrator's WorkingConfig
    /// - Preset changes go through LoadPreset()
    /// - Apply triggers RestartSimulation()
    /// - RefreshUI uses guard flag to avoid event loops
    /// </summary>
    public class SimulationUIOrchestrator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimulationController simulationController;
        [SerializeField] private ScenarioPreset defaultPreset; // Internal demo preset
        
        [Header("UI Controllers (Auto-wired in Awake)")]
        [SerializeField] private TopBarUI topBarUI;
        [SerializeField] private MechanismsSection mechanismsSection;
        [SerializeField] private CoreParametersSection coreParametersSection;
        [SerializeField] private TopologyDetailsSection topologyDetailsSection;
        [SerializeField] private InflowDetailsSection inflowDetailsSection;
        [SerializeField] private DiffusionDetailsSection diffusionDetailsSection;
        [SerializeField] private ViabilityDetailsSection viabilityDetailsSection;

        /// <summary>
        /// THE SINGLE SOURCE OF TRUTH: Working configuration edited by UI
        /// </summary>
        public WorkingScenarioConfig WorkingConfig { get; private set; }

        /// <summary>
        /// Currently selected preset (can be null if using custom config)
        /// </summary>
        public ScenarioPreset CurrentPreset { get; private set; }

        /// <summary>
        /// Guard flag to prevent event loops during UI refresh
        /// </summary>
        private bool _isRefreshing = false;

        void Awake()
        {
            // Auto-wire UI controllers if not set in Inspector
            if (topBarUI == null) topBarUI = FindFirstObjectByType<TopBarUI>();
            if (mechanismsSection == null) mechanismsSection = FindFirstObjectByType<MechanismsSection>();
            if (coreParametersSection == null) coreParametersSection = FindFirstObjectByType<CoreParametersSection>();
            if (topologyDetailsSection == null) topologyDetailsSection = FindFirstObjectByType<TopologyDetailsSection>();
            if (inflowDetailsSection == null) inflowDetailsSection = FindFirstObjectByType<InflowDetailsSection>();
            if (diffusionDetailsSection == null) diffusionDetailsSection = FindFirstObjectByType<DiffusionDetailsSection>();
            if (viabilityDetailsSection == null) viabilityDetailsSection = FindFirstObjectByType<ViabilityDetailsSection>();

            // Initialize with default preset or create empty config
            if (defaultPreset != null)
            {
                Debug.Log($"[SimulationUIOrchestrator] Initializing with default preset: {defaultPreset.PresetName}");
                LoadPreset(defaultPreset);
            }
            else
            {
                Debug.LogWarning("[SimulationUIOrchestrator] No default preset set - using empty config");
                WorkingConfig = new WorkingScenarioConfig();
            }
        }

        void Start()
        {
            // Give PresetSelectorUI a frame to initialize
            StartCoroutine(SyncPresetDropdownAfterInit());
        }

        private System.Collections.IEnumerator SyncPresetDropdownAfterInit()
        {
            // Wait one frame to ensure PresetSelectorUI.Start() has run
            yield return null;
            
            // Ensure preset dropdown shows the current preset
            if (topBarUI != null && CurrentPreset != null)
            {
                var presetSelector = topBarUI.GetComponent<PresetSelectorUI>();
                if (presetSelector == null)
                {
                    // Try finding it anywhere in scene
                    presetSelector = FindFirstObjectByType<PresetSelectorUI>();
                }
                
                if (presetSelector != null)
                {
                    presetSelector.SelectPreset(CurrentPreset);
                    Debug.Log($"[SimulationUIOrchestrator] Synced dropdown to default preset: {CurrentPreset.PresetName}");
                }
                else
                {
                    Debug.LogWarning("[SimulationUIOrchestrator] PresetSelectorUI not found - dropdown won't sync");
                }
            }
        }

        /// <summary>
        /// Load a preset and refresh all UI to match
        /// Called by: TopBar preset dropdown
        /// </summary>
        public void LoadPreset(ScenarioPreset preset)
        {
            if (preset == null)
            {
                Debug.LogWarning("[SimulationUIOrchestrator] Cannot load null preset");
                return;
            }

            CurrentPreset = preset;
            WorkingConfig = WorkingScenarioConfig.FromPreset(preset);
            
            RefreshUIFromWorkingConfig();
            
            Debug.Log($"[SimulationUIOrchestrator] Loaded preset: {preset.PresetName}");
        }

        /// <summary>
        /// Apply current WorkingConfig and restart simulation
        /// Called by: TopBar Apply button
        /// </summary>
        public void ApplyAndRestart()
        {
            if (simulationController == null)
            {
                Debug.LogError("[SimulationUIOrchestrator] SimulationController not set!");
                return;
            }

            // Build ScenarioDefinition from WorkingConfig
            var scenario = BuildScenarioDefinition();
            
            // Build RunRequest from WorkingConfig
            var request = BuildRunRequest();
            
            // Log for diagnostics
            Debug.Log($"[SimulationUIOrchestrator] Apply & Restart: " +
                     $"InflowMode={scenario.EngineConfig.InflowMode}, " +
                     $"BoundaryMode={scenario.EngineConfig.BoundaryMode}, " +
                     $"DiffusionMode={scenario.EngineConfig.DiffusionMode}, " +
                     $"ViabilityRule={scenario.EngineConfig.ViabilityRule}, " +
                     $"Seed={scenario.Seed}");

            // Restart simulation with new configuration
            simulationController.RestartWithScenario(scenario, request);
        }

        /// <summary>
        /// Refresh all UI controls from WorkingConfig
        /// Uses guard flag to prevent event loops
        /// </summary>
        public void RefreshUIFromWorkingConfig()
        {
            if (_isRefreshing) return; // Prevent recursion

            _isRefreshing = true;

            try
            {
                // Refresh each UI section
                if (topBarUI != null)
                {
                    // TopBar handles preset dropdown internally
                }

                if (mechanismsSection != null)
                {
                    mechanismsSection.Refresh(WorkingConfig);
                }

                if (coreParametersSection != null)
                {
                    coreParametersSection.Refresh(WorkingConfig);
                }

                if (topologyDetailsSection != null)
                {
                    topologyDetailsSection.Refresh(WorkingConfig);
                }

                if (inflowDetailsSection != null)
                {
                    inflowDetailsSection.Refresh(WorkingConfig);
                }

                if (diffusionDetailsSection != null)
                {
                    diffusionDetailsSection.Refresh(WorkingConfig);
                }

                if (viabilityDetailsSection != null)
                {
                    viabilityDetailsSection.Refresh(WorkingConfig);
                }

                Debug.Log("[SimulationUIOrchestrator] UI refreshed from WorkingConfig");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        /// <summary>
        /// Build ScenarioDefinition from WorkingConfig
        /// </summary>
        private ScenarioDefinition BuildScenarioDefinition()
        {
            var engineConfig = new EngineConfig
            {
                // Mechanism modes - convert from WorkingConfig enums to Contracts enums
                InflowMode = MapInflowMode(WorkingConfig.Inflow),
                BoundaryMode = MapBoundaryMode(WorkingConfig.Boundary),
                DiffusionMode = MapDiffusionMode(WorkingConfig.Diffusion),
                ViabilityRule = MapViabilityRule(WorkingConfig.ViabilityRule),
                TopologyMode = MapTopologyMode(WorkingConfig.Topology),

                // Point sources (deep copy)
                PointSources = WorkingConfig.PointSources.Select(ps => new PointSourceConfig
                {
                    X = ps.X,
                    Y = ps.Y,
                    Strength = ps.Strength
                }).ToList(),

                // Hysteresis thresholds
                HysteresisOnThreshold = WorkingConfig.HysteresisOnThreshold,
                HysteresisOffThreshold = WorkingConfig.HysteresisOffThreshold,

                // Anisotropic diffusion
                AnisotropyDirectionX = MapAnisotropyDirectionX(WorkingConfig.AnisotropicDirection),
                AnisotropyDirectionY = MapAnisotropyDirectionY(WorkingConfig.AnisotropicDirection),
                AnisotropyBias = WorkingConfig.AnisotropicBias,

                // Mask parameters
                MaskShape = MapMaskShape(WorkingConfig.MaskType),
                MaskRadius = WorkingConfig.MaskRadiusOuter,
                MaskInnerRadius = WorkingConfig.MaskRadiusInner,
                CorridorWidth = WorkingConfig.MaskCorridorWidth,
                HoleProbability = WorkingConfig.MaskPercolationProbability
            };

            var parameters = new Dictionary<string, double>
            {
                ["resourceGlobalMax"] = WorkingConfig.ResourceGlobalMax,
                ["resourceRechargeRate"] = WorkingConfig.ResourceRechargeRate,
                ["decayLoss"] = WorkingConfig.DecayLoss,
                ["maintCost"] = WorkingConfig.MaintCost,
                ["activationCost"] = WorkingConfig.ActivationCost,
                ["expansionProbability"] = WorkingConfig.ExpansionProbability,
                ["inflowPerCell"] = WorkingConfig.InflowPerCell,
                ["diffusionRate"] = WorkingConfig.DiffusionRate
            };

            // Merge advanced parameters
            foreach (var kvp in WorkingConfig.AdvancedParams)
            {
                parameters[kvp.Key] = kvp.Value;
            }

            return new ScenarioDefinition
            {
                ScenarioId = WorkingConfig.ScenarioId,
                ScenarioName = WorkingConfig.ScenarioId,
                Description = $"UI-configured scenario (PhaseSet: {WorkingConfig.PhaseSetId})",
                GridWidth = WorkingConfig.GridWidth,
                GridHeight = WorkingConfig.GridHeight,
                Seed = WorkingConfig.Seed,
                EngineConfig = engineConfig,
                Parameters = parameters
            };
        }

        /// <summary>
        /// Build RunRequest from WorkingConfig
        /// </summary>
        private RunRequest BuildRunRequest()
        {
            return new RunRequest
            {
                Steps = 0, // Unlimited (UI-controlled stop)
                SampleEvery = 10,
                EmitEvents = false
            };
        }

        #region Enum Mapping Methods

        private Contracts.InflowMode MapInflowMode(Configuration.InflowMode mode)
        {
            switch (mode)
            {
                case Configuration.InflowMode.UniformField:
                    return Contracts.InflowMode.Uniform;
                case Configuration.InflowMode.PointSources:
                    return Contracts.InflowMode.PointSources;
                case Configuration.InflowMode.EdgeSources:
                    return Contracts.InflowMode.Boundary;
                default:
                    return Contracts.InflowMode.Uniform;
            }
        }

        private Contracts.BoundaryMode MapBoundaryMode(Configuration.BoundaryMode mode)
        {
            switch (mode)
            {
                case Configuration.BoundaryMode.Closed:
                    return Contracts.BoundaryMode.Reflecting;
                case Configuration.BoundaryMode.Open:
                    return Contracts.BoundaryMode.Absorbing;
                case Configuration.BoundaryMode.Wrap:
                    return Contracts.BoundaryMode.PeriodicWrap;
                default:
                    return Contracts.BoundaryMode.Absorbing;
            }
        }

        private Contracts.DiffusionMode MapDiffusionMode(Configuration.DiffusionMode mode)
        {
            switch (mode)
            {
                case Configuration.DiffusionMode.VonNeumann4:
                    return Contracts.DiffusionMode.VonNeumann4;
                case Configuration.DiffusionMode.Moore8:
                    return Contracts.DiffusionMode.Moore8;
                case Configuration.DiffusionMode.Anisotropic:
                    return Contracts.DiffusionMode.Anisotropic;
                default:
                    return Contracts.DiffusionMode.VonNeumann4;
            }
        }

        private Contracts.ViabilityRule MapViabilityRule(Configuration.ViabilityRuleMode mode)
        {
            switch (mode)
            {
                case Configuration.ViabilityRuleMode.Simple:
                    return Contracts.ViabilityRule.HardThreshold;
                case Configuration.ViabilityRuleMode.Hysteresis:
                    return Contracts.ViabilityRule.Hysteresis;
                default:
                    return Contracts.ViabilityRule.HardThreshold;
            }
        }

        private Contracts.TopologyMode MapTopologyMode(Configuration.TopologyMode mode)
        {
            switch (mode)
            {
                case Configuration.TopologyMode.FullDomain:
                    return Contracts.TopologyMode.RectGrid;
                case Configuration.TopologyMode.MaskedDomain:
                    return Contracts.TopologyMode.MaskedDomain;
                default:
                    return Contracts.TopologyMode.RectGrid;
            }
        }

        private Contracts.MaskShape MapMaskShape(Configuration.MaskShape shape)
        {
            switch (shape)
            {
                case Configuration.MaskShape.Rectangle:
                    return Contracts.MaskShape.Rectangle;
                case Configuration.MaskShape.Circle:
                    return Contracts.MaskShape.Circle;
                case Configuration.MaskShape.Ring:
                    return Contracts.MaskShape.Ring;
                case Configuration.MaskShape.Corridor:
                    return Contracts.MaskShape.Corridor;
                case Configuration.MaskShape.PercolationHoles:
                    return Contracts.MaskShape.PercolationHoles;
                default:
                    return Contracts.MaskShape.Rectangle;
            }
        }

        private double MapAnisotropyDirectionX(Configuration.DiffusionDirection dir)
        {
            switch (dir)
            {
                case Configuration.DiffusionDirection.North: return 0.0;
                case Configuration.DiffusionDirection.East: return 1.0;
                case Configuration.DiffusionDirection.South: return 0.0;
                case Configuration.DiffusionDirection.West: return -1.0;
                default: return 0.0;
            }
        }

        private double MapAnisotropyDirectionY(Configuration.DiffusionDirection dir)
        {
            switch (dir)
            {
                case Configuration.DiffusionDirection.North: return 1.0;
                case Configuration.DiffusionDirection.East: return 0.0;
                case Configuration.DiffusionDirection.South: return -1.0;
                case Configuration.DiffusionDirection.West: return 0.0;
                default: return 0.0;
            }
        }

        #endregion

        /// <summary>
        /// Get current scenario definition for diagnostics
        /// </summary>
        public ScenarioDefinition GetCurrentScenarioDefinition()
        {
            return BuildScenarioDefinition();
        }

        /// <summary>
        /// Check if we're currently refreshing (for UI controllers to check)
        /// </summary>
        public bool IsRefreshing()
        {
            return _isRefreshing;
        }
    }
}
