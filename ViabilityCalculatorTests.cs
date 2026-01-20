// Create: Assets/Viable/Engine.Tests/ViabilityCalculatorTests.cs
using NUnit.Framework;
using Viable.Engine.Computation;

[TestFixture]
public class ViabilityCalculatorTests
{
    [Test]
    public void TestViabilityCalculation_PositiveFlow()
    {
        // Arrange
        float inflow = 1.0f;
        float resource = 50f;
        float complexity = 0.3f;
        
        // Act
        float v = ViabilityCalculator.Compute(
            inflow, resource, complexity,
            complexityGainA: 0.5f,
            complexityGainK: 1.0f,
            decayLoss: 0.003f,
            thresholdEffective: 0.18f
        );
        
        // Assert
        Assert.IsTrue(v > 0, "Cell should be viable with positive inflow");
    }
    
    [Test]
    public void TestDeterminism_SameSeedProducesSameResult()
    {
        // Arrange
        var config = new SimulationConfiguration { /* ... */ };
        var state1 = new GridState(64, 64);
        var state2 = new GridState(64, 64);
        var context1 = new StepContext(config, 1e7f, 1.0f, seed: 42);
        var context2 = new StepContext(config, 1e7f, 1.0f, seed: 42);
        
        var runner1 = new SimulationRunner(state1, phases);
        var runner2 = new SimulationRunner(state2, phases);
        
        // Act
        runner1.StepN(context1, 100);
        runner2.StepN(context2, 100);
        
        // Assert
        for (int i = 0; i < state1.Len; i++)
        {
            Assert.AreEqual(state1.ResourceLocal[i], state2.ResourceLocal[i], 
                $"Resource at cell {i} should be identical");
        }
    }
}