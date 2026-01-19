using UnityEngine;
using Assets.Scripts.Domain;

namespace Assets.Scripts.Events
{
    public static class RegionExpansion
    {
        /// <summary>
        /// Manages active regions: substrate where state transitions are defined.
        /// Expands boundaries around sink regions and at the resource front.
        /// </summary>
        public static void ExpandActiveRegion(
            StateGrid s,
            int tick,
            float regionExpansionChance,
            float regionExpansionCost,
            float regionExpansionMinSource,
            bool requireViability = false,
            bool seedResourceOnExpansion = false,
            float seedResource = 0.1f)
        {
            bool[] nextRegion = (bool[])s.ActiveRegion.Clone();

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    // Sink regions themselves never have active substrate
                    if (s.IsSink[i])
                    {
                        nextRegion[i] = false;
                        continue;
                    }

                    // If cell already has active region, check if it should keep it
                    if (s.ActiveRegion[i])
                    {
                        // Keep region if:
                        // 1. It has resource, OR
                        // 2. It's adjacent to a sink (boundary), OR
                        // 3. It's adjacent to another active cell
                        bool shouldKeepRegion = s.ResourceLocal[i] > 0f;
                        
                        if (!shouldKeepRegion)
                        {
                            for (int d = 0; d < 4; d++)
                            {
                                int nx = x + dx[d];
                                int ny = y + dy[d];
                                if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H) continue;
                                int ni = s.Idx(nx, ny);
                                
                                // Keep region if adjacent to sink or another active cell
                                if (s.IsSink[ni] || s.ActiveRegion[ni])
                                {
                                    shouldKeepRegion = true;
                                    break;
                                }
                            }
                        }
                        
                        if (!shouldKeepRegion)
                            nextRegion[i] = false; // Allow region to decay if isolated
                        
                        continue; // Don't try to re-create region that already exists
                    }

                    // From here: cell doesn't have active region yet
                    // Regions form at boundaries (active-inactive interface)
                    bool adjacentToActive = false;
                    bool adjacentToInactive = false;

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                        {
                            adjacentToInactive = true;
                            continue;
                        }

                        int ni = s.Idx(nx, ny);
                        
                        if (s.ActiveRegion[ni])
                            adjacentToActive = true;
                        else if (!s.ActiveRegion[ni]) // Inactive or sink
                            adjacentToInactive = true;
                    }

                    // Only create active region at the boundary
                    if (!adjacentToActive || !adjacentToInactive)
                        continue;

                    // Now check if a neighboring active cell can pay to expand
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H) continue;

                        int ni = s.Idx(nx, ny);
                        if (!s.ActiveRegion[ni]) continue;

                        // Optional: require source to be viable
                        if (requireViability && !(s.V[ni] > 0f && s.Active[ni] == 1))
                            continue;

                        // Must have enough resource
                        if (s.ResourceLocal[ni] < regionExpansionMinSource)
                            continue;

                        // Probabilistic expansion
                        if (Random.value > regionExpansionChance)
                            continue;

                        // Pay resource cost
                        s.ResourceLocal[ni] = Mathf.Max(0f, s.ResourceLocal[ni] - regionExpansionCost);

                        // Create active region
                        nextRegion[i] = true;

                        if (s.RegionActivationTick[i] == -1)
                            s.RegionActivationTick[i] = tick;

                        // Optional: seed minimal resource
                        if (seedResourceOnExpansion)
                            s.ResourceLocal[i] = Mathf.Max(s.ResourceLocal[i], seedResource);

                        break;
                    }
                }
            }

            s.ActiveRegion = nextRegion;
        }
    }
}
