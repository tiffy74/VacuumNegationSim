using System;
using System.Collections.Generic;
using System.Linq;
using Viable.Contracts;

namespace Viable.Engine.ExpansionModels
{
    /// <summary>
    /// Factory for creating and managing expansion model plugins.
    /// Supports registration of custom models by scientists.
    /// Thread-safe for runtime model switching.
    /// </summary>
    public static class ExpansionModelFactory
    {
        private static readonly Dictionary<ExpansionModel, Func<IExpansionModel>> _registry 
            = new Dictionary<ExpansionModel, Func<IExpansionModel>>();

        private static readonly object _lock = new object();

        /// <summary>
        /// Static constructor registers built-in models.
        /// </summary>
        static ExpansionModelFactory()
        {
            // Register built-in models
            // Viability models will be registered when their plugins are loaded
            RegisterBuiltInModels();
        }

        /// <summary>
        /// Register a model factory function.
        /// Call this to add custom expansion models.
        /// </summary>
        /// <param name="modelType">The enum value for this model</param>
        /// <param name="factory">Factory function that creates a new instance</param>
        public static void Register(ExpansionModel modelType, Func<IExpansionModel> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            lock (_lock)
            {
                _registry[modelType] = factory;
            }
        }

        /// <summary>
        /// Register a model type directly (uses Activator.CreateInstance).
        /// Requires parameterless constructor.
        /// </summary>
        public static void Register<T>(ExpansionModel modelType) where T : IExpansionModel, new()
        {
            Register(modelType, () => new T());
        }

        /// <summary>
        /// Create an instance of the specified expansion model.
        /// </summary>
        /// <param name="modelType">The model type to create</param>
        /// <param name="config">Optional configuration (applied after creation)</param>
        /// <returns>Configured model instance</returns>
        public static IExpansionModel Create(ExpansionModel modelType, ExpansionConfig config = null)
        {
            Func<IExpansionModel> factory;

            lock (_lock)
            {
                if (!_registry.TryGetValue(modelType, out factory))
                {
                    throw new ArgumentException(
                        $"No expansion model registered for type '{modelType}'. " +
                        $"Available models: {string.Join(", ", _registry.Keys)}",
                        nameof(modelType));
                }
            }

            var model = factory();

            if (config != null)
            {
                model.Configure(config);
            }
            else
            {
                // Use model's default config
                model.Configure(model.GetDefaultConfig());
            }

            return model;
        }

        /// <summary>
        /// Create a model from an ExpansionConfig (uses Config.Model to determine type).
        /// </summary>
        public static IExpansionModel CreateFromConfig(ExpansionConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return Create(config.Model, config);
        }

        /// <summary>
        /// Check if a model type is registered.
        /// </summary>
        public static bool IsRegistered(ExpansionModel modelType)
        {
            lock (_lock)
            {
                return _registry.ContainsKey(modelType);
            }
        }

        /// <summary>
        /// Get all registered model types.
        /// </summary>
        public static IEnumerable<ExpansionModel> GetRegisteredModels()
        {
            lock (_lock)
            {
                return _registry.Keys.ToList();
            }
        }

        /// <summary>
        /// Get model metadata for UI display.
        /// Creates temporary instances to read metadata.
        /// </summary>
        public static IEnumerable<ModelMetadata> GetModelMetadata()
        {
            var result = new List<ModelMetadata>();

            lock (_lock)
            {
                foreach (var kvp in _registry)
                {
                    try
                    {
                        var instance = kvp.Value();
                        result.Add(new ModelMetadata
                        {
                            ModelType = kvp.Key,
                            DisplayName = instance.DisplayName,
                            Description = instance.Description,
                            Category = instance.Category
                        });
                    }
                    catch
                    {
                        // Skip models that fail to instantiate
                    }
                }
            }

            // Sort with default model first, then by category and name
            return result
                .OrderBy(m => m.ModelType == ExpansionModel.DefaultViabilityBoundaryPressure ? 0 : 1) // Default first
                .ThenBy(m => m.Category)
                .ThenBy(m => m.DisplayName);
        }

        /// <summary>
        /// Get models grouped by category for UI dropdown.
        /// </summary>
        public static Dictionary<string, List<ModelMetadata>> GetModelsByCategory()
        {
            return GetModelMetadata()
                .GroupBy(m => m.Category)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Unregister a model (for testing or hot-reload).
        /// </summary>
        public static bool Unregister(ExpansionModel modelType)
        {
            lock (_lock)
            {
                return _registry.Remove(modelType);
            }
        }

        /// <summary>
        /// Clear all registrations (for testing).
        /// </summary>
        public static void ClearAll()
        {
            lock (_lock)
            {
                _registry.Clear();
            }
        }

        /// <summary>
        /// Register built-in models.
        /// Called from static constructor.
        /// </summary>
        private static void RegisterBuiltInModels()
        {
            // ===== Viability-Constraint Models =====
            Register<Viability.ViabilityBoundaryPressureModel>(ExpansionModel.DefaultViabilityBoundaryPressure);
            
            // ===== Register additional models using reflection to avoid compile-time dependency =====
            TryRegisterModel("Viable.Engine.ExpansionModels.Cosmological.InflationaryModel", ExpansionModel.Inflationary);
            TryRegisterModel("Viable.Engine.ExpansionModels.Cosmological.CyclicModel", ExpansionModel.Cyclic);
            TryRegisterModel("Viable.Engine.ExpansionModels.Biological.LogisticGrowthModel", ExpansionModel.LogisticGrowth);
            TryRegisterModel("Viable.Engine.ExpansionModels.FluidDynamics.DLAModel", ExpansionModel.DLA);
            TryRegisterModel("Viable.Engine.ExpansionModels.StatisticalPhysics.PercolationModel", ExpansionModel.Percolation);
        }

        /// <summary>
        /// Try to register a model by type name using reflection.
        /// Silently fails if the type doesn't exist (e.g., not yet compiled by Unity).
        /// </summary>
        private static void TryRegisterModel(string typeName, ExpansionModel modelType)
        {
            try
            {
                var type = Type.GetType(typeName + ", Viable.Engine");
                if (type != null)
                {
                    Register(modelType, () => (IExpansionModel)Activator.CreateInstance(type));
                }
            }
            catch
            {
                // Silently ignore - model will be registered when Unity recompiles
            }
        }
    }

    /// <summary>
    /// Metadata about an expansion model for UI display.
    /// </summary>
    public class ModelMetadata
    {
        public ExpansionModel ModelType { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
}
