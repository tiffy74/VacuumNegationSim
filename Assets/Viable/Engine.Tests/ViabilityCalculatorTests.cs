using NUnit.Framework;
using Viable.Engine.Computation;

namespace Viable.Engine.Tests
{
    /// <summary>
    /// Tests for the core viability calculation logic.
    /// Viability determines whether a cell can persist based on resource flow and complexity.
    /// </summary>
    [TestFixture]
    public class ViabilityCalculatorTests
    {
        [Test]
        public void Compute_WithPositiveInflow_ReturnsPositiveViability()
        {
            // Arrange
            float inflow = 1.0f;
            float resource = 50f;
            float complexity = 0.3f;
            float complexityGainA = 0.5f;
            float complexityGainK = 1.0f;
            float decayLoss = 0.003f;
            float threshold = 0.18f;

            // Act
            float viability = ViabilityCalculator.Compute(
                inflow, resource, complexity,
                complexityGainA, complexityGainK,
                decayLoss, threshold
            );

            // Assert
            Assert.IsTrue(viability > 0, "Cell should be viable with positive inflow");
        }

        [Test]
        public void Compute_WithZeroInflow_ReturnsNegativeViability()
        {
            // Arrange
            float inflow = 0f;
            float resource = 50f;
            float complexity = 0.3f;
            float complexityGainA = 0.5f;
            float complexityGainK = 1.0f;
            float decayLoss = 0.003f;
            float threshold = 0.18f;

            // Act
            float viability = ViabilityCalculator.Compute(
                inflow, resource, complexity,
                complexityGainA, complexityGainK,
                decayLoss, threshold
            );

            // Assert
            Assert.IsTrue(viability < 0, "Cell should not be viable with zero inflow (decay dominates)");
        }

        [Test]
        public void Compute_HigherComplexity_IncreasesViability()
        {
            // Arrange
            float inflow = 1.0f;
            float resource = 50f;
            float complexityGainA = 0.5f;
            float complexityGainK = 1.0f;
            float decayLoss = 0.003f;
            float threshold = 0.18f;

            // Act
            float lowComplexityViability = ViabilityCalculator.Compute(
                inflow, resource, 0.1f, complexityGainA, complexityGainK, decayLoss, threshold);

            float highComplexityViability = ViabilityCalculator.Compute(
                inflow, resource, 0.9f, complexityGainA, complexityGainK, decayLoss, threshold);

            // Assert
            Assert.IsTrue(highComplexityViability > lowComplexityViability,
                "Higher complexity should result in higher viability");
        }

        [Test]
        public void ComputeEffectiveThreshold_HighScarcity_IncreasesThreshold()
        {
            // Arrange
            float resourceGlobalMax = 1000f;
            float thresholdBase = 0.18f;
            float scarcityK = 0.3f;

            // Act
            float thresholdWithAbundance = ViabilityCalculator.ComputeEffectiveThreshold(
                resourceGlobal: 900f,  // 90% resources remaining
                resourceGlobalMax, thresholdBase, scarcityK
            );

            float thresholdWithScarcity = ViabilityCalculator.ComputeEffectiveThreshold(
                resourceGlobal: 100f,  // 10% resources remaining
                resourceGlobalMax, thresholdBase, scarcityK
            );

            // Assert
            Assert.IsTrue(thresholdWithScarcity > thresholdWithAbundance,
                "Threshold should increase when global resources are scarce");
            Assert.IsTrue(thresholdWithAbundance >= thresholdBase,
                "Threshold should never be below base");
        }

        [Test]
        public void Compute_ZeroThreshold_DoesNotCrash()
        {
            // Arrange - Edge case: prevent division by zero
            float inflow = 1.0f;
            float resource = 50f;
            float complexity = 0.3f;

            // Act
            float viability = ViabilityCalculator.Compute(
                inflow, resource, complexity,
                complexityGainA: 0.5f,
                complexityGainK: 1.0f,
                decayLoss: 0.003f,
                thresholdEffective: 0f  // Edge case!
            );

            // Assert
            Assert.IsFalse(float.IsNaN(viability), "Should not produce NaN");
            Assert.IsFalse(float.IsInfinity(viability), "Should not produce Infinity");
        }
    }
}
