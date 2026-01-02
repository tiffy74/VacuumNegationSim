using UnityEngine;
using Assets.Scripts.Domain;

namespace Assets.Scripts.Events
{
    /// <summary>
    /// Pass 3: Configuration Space Propagation (Field Wave)
    /// 
    /// Manages the expansion of active configuration space at boundaries.
    /// Configuration space pervades everywhere but is only "active" (FieldPresent=true) where:
    /// 1. At boundaries between energy field and void
    /// 2. Adjacent to black holes (event horizon boundary)
    /// 3. Where energy can potentially propagate
    /// 
    /// Think of this as spacetime geometry becoming "observable" where it mediates interactions.
    /// </summary>
    public static class FieldWave
    {
        // ============================================================================
        // PRIVATE CONSTANTS
        // ============================================================================
        
        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };

        // ============================================================================
        // PUBLIC API: FIELD PROPAGATION
        // ============================================================================

        /// <summary>
        /// Propagates configuration space at boundaries and maintains event horizons.
        /// Configuration space expands probabilistically when neighboring cells can "pay"
        /// the activation cost from their local energy.
        /// </summary>
        public static void PropagateFieldWave(
            GridState s,
            int tick,
            float fieldAdvanceChance,
            float fieldAdvanceCost,
            float fieldAdvanceMinSource,
            bool requireViability = false,
            bool seedEnergyOnAdvance = false,
            float seedEnergy = 0.1f)
        {
            bool[] nextField = (bool[])s.FieldPresent.Clone();

            // Phase 1: Ensure configuration space persists around black holes (event horizons)
            MaintainBlackHoleEventHorizons(s, nextField, tick);

            // Phase 2: Expand configuration space at energy-void boundaries
            ExpandFieldAtBoundaries(
                s, nextField, tick,
                fieldAdvanceChance, fieldAdvanceCost, fieldAdvanceMinSource,
                requireViability, seedEnergyOnAdvance, seedEnergy);

            s.FieldPresent = nextField;
        }

        // ============================================================================
        // PRIVATE: BLACK HOLE EVENT HORIZONS
        // ============================================================================

        /// <summary>
        /// Maintains configuration space around black holes (event horizon boundaries).
        /// Cells adjacent to black holes must have active config space to mediate
        /// the interaction between collapsed geometry and surrounding field.
        /// </summary>
        private static void MaintainBlackHoleEventHorizons(GridState s, bool[] nextField, int tick)
        {
            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    // Black holes themselves never have FieldPresent
                    if (s.IsBlackHole[i])
                    {
                        nextField[i] = false;
                        continue;
                    }

                    // Cells with existing field: check if they should keep it
                    if (s.FieldPresent[i])
                    {
                        bool shouldKeepField = ShouldCellMaintainField(i, s);
                        
                        if (!shouldKeepField)
                            nextField[i] = false; // Allow field decay if isolated
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a cell should maintain its configuration space.
        /// Field persists if:
        /// 1. Cell has energy (active field)
        /// 2. Adjacent to a black hole (event horizon boundary)
        /// 3. Adjacent to another field cell (connected field structure)
        /// </summary>
        private static bool ShouldCellMaintainField(int idx, GridState s)
        {
            // Always keep field if cell has energy
            if (s.Nlocal[idx] > 0f)
                return true;

            // Check adjacency to black holes or other field cells
            for (int d = 0; d < 4; d++)
            {
                int x = idx % s.W;
                int y = idx / s.W;
                int nx = x + dx[d];
                int ny = y + dy[d];
                
                if (!IsInBounds(nx, ny, s.W, s.H)) continue;
                
                int ni = s.Idx(nx, ny);

                // Keep field if adjacent to BH (event horizon) or another field cell
                if (s.IsBlackHole[ni] || s.FieldPresent[ni])
                    return true;
            }

            return false; // Isolated field - allow decay
        }

        // ============================================================================
        // PRIVATE: BOUNDARY EXPANSION
        // ============================================================================

        /// <summary>
        /// Expands configuration space at boundaries where energy meets void.
        /// Field propagates when a neighboring cell with energy "pays" the activation cost.
        /// </summary>
        private static void ExpandFieldAtBoundaries(
            GridState s, bool[] nextField, int tick,
            float fieldAdvanceChance, float fieldAdvanceCost, float fieldAdvanceMinSource,
            bool requireViability, bool seedEnergyOnAdvance, float seedEnergy)
        {
            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    // Skip cells that already have field or are black holes
                    if (nextField[i]) continue;
                    if (s.IsBlackHole[i]) continue;

                    // Check if this is a valid boundary location
                    if (!IsBoundaryCell(i, x, y, s))
                        continue;

                    // Try to expand field from a viable neighbor
                    bool fieldExpanded = TryExpandFieldFromNeighbors(
                        i, x, y, s, tick,
                        fieldAdvanceChance, fieldAdvanceCost, fieldAdvanceMinSource,
                        requireViability, seedEnergyOnAdvance, seedEnergy);

                    if (fieldExpanded)
                    {
                        nextField[i] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a cell is at a boundary where field can expand.
        /// Boundary = adjacent to both existing field AND void (or non-field).
        /// </summary>
        private static bool IsBoundaryCell(int idx, int x, int y, GridState s)
        {
            bool adjacentToField = false;
            bool adjacentToVoidOrNonField = false;

            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                
                if (!IsInBounds(nx, ny, s.W, s.H))
                {
                    adjacentToVoidOrNonField = true;
                    continue;
                }

                int ni = s.Idx(nx, ny);

                if (s.FieldPresent[ni])
                    adjacentToField = true;
                else if (!s.IsBlackHole[ni]) // Void or non-field (but not BH)
                    adjacentToVoidOrNonField = true;
            }

            return adjacentToField && adjacentToVoidOrNonField;
        }

        /// <summary>
        /// Attempts to expand field from a neighboring cell.
        /// Neighbor must have sufficient energy and (optionally) be viable.
        /// Probabilistic expansion based on fieldAdvanceChance.
        /// </summary>
        private static bool TryExpandFieldFromNeighbors(
            int targetIdx, int targetX, int targetY, GridState s, int tick,
            float fieldAdvanceChance, float fieldAdvanceCost, float fieldAdvanceMinSource,
            bool requireViability, bool seedEnergyOnAdvance, float seedEnergy)
        {
            for (int d = 0; d < 4; d++)
            {
                int nx = targetX + dx[d];
                int ny = targetY + dy[d];
                
                if (!IsInBounds(nx, ny, s.W, s.H)) continue;

                int ni = s.Idx(nx, ny);
                if (!s.FieldPresent[ni]) continue;

                // Check if this neighbor can expand field
                if (!CanNeighborExpandField(ni, s, fieldAdvanceMinSource, requireViability))
                    continue;

                // Probabilistic expansion
                if (Random.value > fieldAdvanceChance)
                    continue;

                // Neighbor pays energy cost to activate config space
                ExpandFieldFromNeighbor(
                    targetIdx, ni, s, tick,
                    fieldAdvanceCost, seedEnergyOnAdvance, seedEnergy);

                return true; // Field successfully expanded
            }

            return false; // No viable neighbor found
        }

        /// <summary>
        /// Checks if a neighbor cell can expand configuration space.
        /// Requirements: sufficient energy AND (optionally) positive viability.
        /// </summary>
        private static bool CanNeighborExpandField(
            int neighborIdx, GridState s,
            float fieldAdvanceMinSource, bool requireViability)
        {
            // Must have enough energy to pay expansion cost
            if (s.Nlocal[neighborIdx] < fieldAdvanceMinSource)
                return false;

            // Optional: require source to be viable
            if (requireViability && !(s.V[neighborIdx] > 0f && s.Active[neighborIdx] == 1))
                return false;

            return true;
        }

        /// <summary>
        /// Expands field from a neighbor: deducts energy cost, marks arrival tick,
        /// optionally seeds initial energy at the new field location.
        /// </summary>
        private static void ExpandFieldFromNeighbor(
            int targetIdx, int neighborIdx, GridState s, int tick,
            float fieldAdvanceCost, bool seedEnergyOnAdvance, float seedEnergy)
        {
            // Neighbor pays energy cost to activate configuration space
            s.Nlocal[neighborIdx] = Mathf.Max(0f, s.Nlocal[neighborIdx] - fieldAdvanceCost);

            // Mark when field arrived at this cell
            if (s.FieldFirstTick[targetIdx] == -1)
                s.FieldFirstTick[targetIdx] = tick;

            // Optional: seed minimal energy to prevent field-energy desync
            if (seedEnergyOnAdvance)
                s.Nlocal[targetIdx] = Mathf.Max(s.Nlocal[targetIdx], seedEnergy);
        }

        // ============================================================================
        // PRIVATE: UTILITIES
        // ============================================================================

        /// <summary>
        /// Checks if coordinates are within grid bounds.
        /// </summary>
        private static bool IsInBounds(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }
}
