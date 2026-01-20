using NUnit.Framework;
using Viable.Engine.Logic;
using Viable.Engine.State;

namespace Viable.Engine.Tests
{
    /// <summary>
    /// Tests for the SinkLogic union-find algorithm.
    /// Ensures proper sink creation, merging, and root finding.
    /// </summary>
    [TestFixture]
    public class SinkLogicTests
    {
        [Test]
        public void GetRootAtCell_WithNoSink_ReturnsZero()
        {
            // Arrange
            var state = new GridState(10, 10);

            // Act
            int root = SinkLogic.GetRootAtCell(5, state.SinkId, state.SinkParent);

            // Assert
            Assert.AreEqual(0, root, "Non-sink cell should return root 0");
        }

        [Test]
        public void AssignOrMergeAtCell_CreatesNewSink()
        {
            // Arrange
            var state = new GridState(10, 10);
            int cellIdx = 5;

            // Act
            int sinkId = SinkLogic.AssignOrMergeAtCell(
                cellIdx, state.W, state.H,
                state.IsSink, state.SinkId, state.SinkParent, state.SinkMass,
                ref state.NextSinkId
            );

            // Assert
            Assert.IsTrue(state.IsSink[cellIdx], "Cell should be marked as sink");
            Assert.AreEqual(1, sinkId, "First sink should have ID 1");
            Assert.AreEqual(2, state.NextSinkId, "NextSinkId should be incremented");
            Assert.AreEqual(sinkId, state.SinkId[cellIdx], "Cell should store sink ID");
        }

        [Test]
        public void AssignOrMergeAtCell_MergesAdjacentSinks()
        {
            // Arrange
            var state = new GridState(10, 10);
            
            // Create two separate sinks
            int sink1Idx = 5;  // Cell (5, 0)
            int sink2Idx = 6;  // Cell (6, 0) - adjacent to sink1

            // Act - Create first sink
            int sink1Root = SinkLogic.AssignOrMergeAtCell(
                sink1Idx, state.W, state.H,
                state.IsSink, state.SinkId, state.SinkParent, state.SinkMass,
                ref state.NextSinkId
            );

            // Create second sink (should merge with first)
            int sink2Root = SinkLogic.AssignOrMergeAtCell(
                sink2Idx, state.W, state.H,
                state.IsSink, state.SinkId, state.SinkParent, state.SinkMass,
                ref state.NextSinkId
            );

            // Assert
            Assert.AreEqual(sink1Root, sink2Root, "Adjacent sinks should merge to same root");
            Assert.IsTrue(state.IsSink[sink1Idx], "First cell should be sink");
            Assert.IsTrue(state.IsSink[sink2Idx], "Second cell should be sink");
        }

        [Test]
        public void AssignOrMergeAtCell_NonAdjacentSinks_RemainSeparate()
        {
            // Arrange
            var state = new GridState(10, 10);
            
            // Create two non-adjacent sinks
            int sink1Idx = 0;   // Cell (0, 0)
            int sink2Idx = 99;  // Cell (9, 9) - far away

            // Act
            int sink1Root = SinkLogic.AssignOrMergeAtCell(
                sink1Idx, state.W, state.H,
                state.IsSink, state.SinkId, state.SinkParent, state.SinkMass,
                ref state.NextSinkId
            );

            int sink2Root = SinkLogic.AssignOrMergeAtCell(
                sink2Idx, state.W, state.H,
                state.IsSink, state.SinkId, state.SinkParent, state.SinkMass,
                ref state.NextSinkId
            );

            // Assert
            Assert.AreNotEqual(sink1Root, sink2Root, "Non-adjacent sinks should have different roots");
        }

        [Test]
        public void GetRootAtCell_AfterMerge_ReturnsCorrectRoot()
        {
            // Arrange
            var state = new GridState(10, 10);
            int cell1 = 5;
            int cell2 = 6;

            // Create and merge two sinks
            SinkLogic.AssignOrMergeAtCell(
                cell1, state.W, state.H,
                state.IsSink, state.SinkId, state.SinkParent, state.SinkMass,
                ref state.NextSinkId
            );

            int mergedRoot = SinkLogic.AssignOrMergeAtCell(
                cell2, state.W, state.H,
                state.IsSink, state.SinkId, state.SinkParent, state.SinkMass,
                ref state.NextSinkId
            );

            // Act
            int root1 = SinkLogic.GetRootAtCell(cell1, state.SinkId, state.SinkParent);
            int root2 = SinkLogic.GetRootAtCell(cell2, state.SinkId, state.SinkParent);

            // Assert
            Assert.AreEqual(mergedRoot, root1, "First cell should point to merged root");
            Assert.AreEqual(mergedRoot, root2, "Second cell should point to merged root");
            Assert.AreEqual(root1, root2, "Both cells should share same root");
        }

        [Test]
        public void AssignOrMergeAtCell_MultipleAdjacentSinks_MergeIntoOne()
        {
            // Arrange
            var state = new GridState(10, 10);
            
            // Create a 2x2 block of sinks
            int[] sinkCells = { 0, 1, 10, 11 }; // Top-left 2x2 corner

            // Act - Create sinks one by one
            int finalRoot = 0;
            foreach (int cellIdx in sinkCells)
            {
                finalRoot = SinkLogic.AssignOrMergeAtCell(
                    cellIdx, state.W, state.H,
                    state.IsSink, state.SinkId, state.SinkParent, state.SinkMass,
                    ref state.NextSinkId
                );
            }

            // Assert - All should be merged into single root
            int root0 = SinkLogic.GetRootAtCell(0, state.SinkId, state.SinkParent);
            int root1 = SinkLogic.GetRootAtCell(1, state.SinkId, state.SinkParent);
            int root10 = SinkLogic.GetRootAtCell(10, state.SinkId, state.SinkParent);
            int root11 = SinkLogic.GetRootAtCell(11, state.SinkId, state.SinkParent);

            Assert.AreEqual(root0, root1, "All sinks should merge to same root");
            Assert.AreEqual(root0, root10, "All sinks should merge to same root");
            Assert.AreEqual(root0, root11, "All sinks should merge to same root");
        }
    }
}
