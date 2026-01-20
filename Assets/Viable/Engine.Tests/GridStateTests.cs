using NUnit.Framework;
using Viable.Engine.State;
using System;

namespace Viable.Engine.Tests
{
    /// <summary>
    /// Tests for the GridState data structure.
    /// Ensures proper initialization, indexing, and capacity management.
    /// </summary>
    [TestFixture]
    public class GridStateTests
    {
        [Test]
        public void Constructor_CreatesCorrectDimensions()
        {
            // Arrange & Act
            var state = new GridState(64, 48);

            // Assert
            Assert.AreEqual(64, state.W, "Width should match constructor parameter");
            Assert.AreEqual(48, state.H, "Height should match constructor parameter");
            Assert.AreEqual(64 * 48, state.Len, "Length should be Width * Height");
        }

        [Test]
        public void Constructor_InitializesAllArrays()
        {
            // Arrange & Act
            var state = new GridState(10, 10);

            // Assert
            Assert.IsNotNull(state.ResourceLocal);
            Assert.IsNotNull(state.ComplexityMetric);
            Assert.IsNotNull(state.V);
            Assert.IsNotNull(state.Active);
            Assert.IsNotNull(state.Incoming);
            Assert.IsNotNull(state.ComplexityNext);
            Assert.IsNotNull(state.IsInactive);
            Assert.IsNotNull(state.ActiveRegion);
            Assert.IsNotNull(state.RegionActivationTick);
            Assert.IsNotNull(state.ResourceFirstTick);
            Assert.IsNotNull(state.IsSink);
            Assert.IsNotNull(state.SinkCharge);
            Assert.IsNotNull(state.SinkId);
            Assert.IsNotNull(state.ZeroResourceTicks);
            Assert.IsNotNull(state.SinkPotential);
            Assert.IsNotNull(state.SinkParent);
            Assert.IsNotNull(state.SinkMass);

            Assert.AreEqual(100, state.ResourceLocal.Length);
            Assert.AreEqual(100, state.V.Length);
        }

        [Test]
        public void Idx_ConvertsCoordinatesCorrectly()
        {
            // Arrange
            var state = new GridState(10, 10);

            // Act & Assert
            Assert.AreEqual(0, state.Idx(0, 0), "Top-left should be index 0");
            Assert.AreEqual(9, state.Idx(9, 0), "Top-right should be index 9");
            Assert.AreEqual(90, state.Idx(0, 9), "Bottom-left should be index 90");
            Assert.AreEqual(99, state.Idx(9, 9), "Bottom-right should be index 99");
            Assert.AreEqual(55, state.Idx(5, 5), "Center should be (5 + 5*10) = 55");
        }

        [Test]
        public void Reset_ClearsAllState()
        {
            // Arrange
            var state = new GridState(10, 10);
            
            // Modify state
            state.ResourceLocal[5] = 100f;
            state.V[10] = 0.5f;
            state.Active[15] = 1;
            state.ActiveRegion[20] = true;
            state.IsSink[25] = true;
            state.RegionActivationTick[30] = 42;
            state.ResourceFirstTick[35] = 100;
            state.NextSinkId = 10;

            // Act
            state.Reset();

            // Assert
            Assert.AreEqual(0f, state.ResourceLocal[5]);
            Assert.AreEqual(0f, state.V[10]);
            Assert.AreEqual(0, state.Active[15]);
            Assert.IsFalse(state.ActiveRegion[20]);
            Assert.IsFalse(state.IsSink[25]);
            Assert.AreEqual(-1, state.RegionActivationTick[30]);
            Assert.AreEqual(-1, state.ResourceFirstTick[35]);
            Assert.AreEqual(1, state.NextSinkId);
        }

        [Test]
        public void EnsureSinkCapacity_ExpandsArraysWhenNeeded()
        {
            // Arrange
            var state = new GridState(10, 10, initialSinkCapacity: 16);
            int initialCapacity = state.SinkParent.Length;

            // Act
            state.EnsureSinkCapacity(100); // Request capacity for 100 sinks

            // Assert
            Assert.IsTrue(state.SinkParent.Length >= 100, "SinkParent array should expand");
            Assert.IsTrue(state.SinkMass.Length >= 100, "SinkMass array should expand");
            Assert.IsTrue(state.SinkParent.Length > initialCapacity, "Array should have grown");
        }

        [Test]
        public void CreateSinkEntity_ReturnsUniqueIds()
        {
            // Arrange
            var state = new GridState(10, 10);

            // Act
            int id1 = state.CreateSinkEntity(1.0f);
            int id2 = state.CreateSinkEntity(1.5f);
            int id3 = state.CreateSinkEntity(2.0f);

            // Assert
            Assert.AreNotEqual(id1, id2, "Sink IDs should be unique");
            Assert.AreNotEqual(id2, id3, "Sink IDs should be unique");
            Assert.AreEqual(1, id1, "First sink should have ID 1");
            Assert.AreEqual(2, id2, "Second sink should have ID 2");
            Assert.AreEqual(3, id3, "Third sink should have ID 3");
        }

        [Test]
        public void CreateSinkEntity_InitializesParentAndMass()
        {
            // Arrange
            var state = new GridState(10, 10);

            // Act
            int id = state.CreateSinkEntity(5.0f);

            // Assert
            Assert.AreEqual(id, state.SinkParent[id], "Sink should be its own parent (root)");
            Assert.AreEqual(5.0f, state.SinkMass[id], 0.001f, "Sink mass should match initialization");
        }

        [Test]
        public void Constructor_WithInvalidDimensions_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridState(0, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridState(10, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridState(-5, 10));
        }
    }
}
