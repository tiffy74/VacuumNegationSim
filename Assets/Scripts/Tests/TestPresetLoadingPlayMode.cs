using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Viable.Core.Unity;

namespace Viable.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for preset loading from Resources folder.
    /// Tests that all presets are accessible at runtime and have valid properties.
    /// Stage 12: Validates UI can load presets.
    /// </summary>
    public class TestPresetLoadingPlayMode
    {
        // Preset paths in Resources folder
        private const string PresetsPath = "Presets/Examples";

        [Test]
        public void AllPresets_CanLoadFromResources()
        {
            // Arrange
            string[] presetNames = {
                "01_BalancedPersistence",
                "02_ResourceStress",
                "03_RapidExpansion",
                "04_CompetingRegions",
                "05_StochasticDynamics"
            };

            // Act & Assert
            foreach (string presetName in presetNames)
            {
                var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/{presetName}");
                Assert.IsNotNull(preset, $"{presetName} should load from Resources");
            }
        }

        [Test]
        public void BalancedPersistence_LoadsWithCorrectProperties()
        {
            // Act
            var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/01_BalancedPersistence");

            // Assert
            Assert.IsNotNull(preset, "Preset should load");
            Assert.IsFalse(string.IsNullOrWhiteSpace(preset.PresetName), "Preset should have a name");
            Assert.IsTrue(preset.GridWidth > 0, "Grid width should be positive");
            Assert.IsTrue(preset.GridHeight > 0, "Grid height should be positive");
        }

        [Test]
        public void ResourceStress_LoadsWithCorrectProperties()
        {
            // Act
            var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/02_ResourceStress");

            // Assert
            Assert.IsNotNull(preset, "Preset should load");
            Assert.IsFalse(string.IsNullOrWhiteSpace(preset.PresetName), "Preset should have a name");
            Assert.IsTrue(preset.GridWidth > 0);
            Assert.IsTrue(preset.GridHeight > 0);
        }

        [Test]
        public void RapidExpansion_LoadsWithCorrectProperties()
        {
            // Act
            var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/03_RapidExpansion");

            // Assert
            Assert.IsNotNull(preset, "Preset should load");
            Assert.IsFalse(string.IsNullOrWhiteSpace(preset.PresetName), "Preset should have a name");
            Assert.IsTrue(preset.GridWidth > 0);
            Assert.IsTrue(preset.GridHeight > 0);
        }

        [Test]
        public void CompetingRegions_LoadsWithCorrectProperties()
        {
            // Act
            var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/04_CompetingRegions");

            // Assert
            Assert.IsNotNull(preset, "Preset should load");
            Assert.IsFalse(string.IsNullOrWhiteSpace(preset.PresetName), "Preset should have a name");
            Assert.IsTrue(preset.GridWidth > 0);
            Assert.IsTrue(preset.GridHeight > 0);
        }

        [Test]
        public void StochasticDynamics_LoadsWithCorrectProperties()
        {
            // Act
            var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/05_StochasticDynamics");

            // Assert
            Assert.IsNotNull(preset, "Preset should load");
            Assert.IsFalse(string.IsNullOrWhiteSpace(preset.PresetName), "Preset should have a name");
            Assert.IsTrue(preset.GridWidth > 0);
            Assert.IsTrue(preset.GridHeight > 0);
        }

        [Test]
        public void AllPresets_HaveValidDescriptions()
        {
            // Arrange
            string[] presetNames = {
                "01_BalancedPersistence",
                "02_ResourceStress",
                "03_RapidExpansion",
                "04_CompetingRegions",
                "05_StochasticDynamics"
            };

            // Act & Assert
            foreach (string presetName in presetNames)
            {
                var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/{presetName}");
                Assert.IsNotNull(preset, $"{presetName} should load");
                Assert.IsFalse(string.IsNullOrWhiteSpace(preset.Description), $"{presetName} should have a description");
                Assert.IsTrue(preset.Description.Length > 10, $"{presetName} description should be meaningful");
            }
        }

        [Test]
        public void AllPresets_HaveValidGridSizes()
        {
            // Arrange
            string[] presetNames = {
                "01_BalancedPersistence",
                "02_ResourceStress",
                "03_RapidExpansion",
                "04_CompetingRegions",
                "05_StochasticDynamics"
            };

            // Act & Assert
            foreach (string presetName in presetNames)
            {
                var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/{presetName}");
                Assert.IsNotNull(preset, $"{presetName} should load");
                Assert.IsTrue(preset.GridWidth >= 32 && preset.GridWidth <= 256, $"{presetName} grid width should be reasonable");
                Assert.IsTrue(preset.GridHeight >= 32 && preset.GridHeight <= 256, $"{presetName} grid height should be reasonable");
            }
        }

        [Test]
        public void AllPresets_HaveValidTicksPerSecond()
        {
            // Arrange
            string[] presetNames = {
                "01_BalancedPersistence",
                "02_ResourceStress",
                "03_RapidExpansion",
                "04_CompetingRegions",
                "05_StochasticDynamics"
            };

            // Act & Assert
            foreach (string presetName in presetNames)
            {
                var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/{presetName}");
                Assert.IsNotNull(preset, $"{presetName} should load");
                Assert.IsTrue(preset.TicksPerSecond > 0, $"{presetName} TicksPerSecond should be positive");
                Assert.IsTrue(preset.TicksPerSecond <= 100, $"{presetName} TicksPerSecond should be reasonable");
            }
        }

        [UnityTest]
        public IEnumerator PresetSelector_CanLoadAllPresetsAtRuntime()
        {
            // Arrange
            string[] presetNames = {
                "01_BalancedPersistence",
                "02_ResourceStress",
                "03_RapidExpansion",
                "04_CompetingRegions",
                "05_StochasticDynamics"
            };

            // Act & Assert
            foreach (string presetName in presetNames)
            {
                var preset = Resources.Load<ScenarioPreset>($"{PresetsPath}/{presetName}");
                Assert.IsNotNull(preset, $"PresetSelector should be able to load {presetName}");
                
                // Verify preset has required properties for UI
                Assert.IsFalse(string.IsNullOrWhiteSpace(preset.PresetName), $"{presetName} must have a name for UI");
                Assert.IsFalse(string.IsNullOrWhiteSpace(preset.Description), $"{presetName} must have a description for UI");
                
                yield return null;
            }
        }
    }
}
