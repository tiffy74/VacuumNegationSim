// =============================================
// DEPRECATED: This file will be removed in Phase 7
// Use: Viable.Engine.Logic.SinkLogic instead
// Core union-find logic has been extracted to engine
// =============================================

using System;
using UnityEngine;
using Assets.Scripts.Domain;

// [DEPRECATED - Phase 4] This file will be removed in Phase 7
// New location: Assets/Viable/Engine/Logic/SinkLogic.cs
// DO NOT modify this file - changes go to new location

namespace Assets.Scripts.Events
{
    public static class SinkRegions
    {
        public static float LastDrained { get; private set; }
        public static float LastDrainedThisTick { get; private set; }
        public static int LastDrainedEdges { get; private set; }
        public static float LastRecoilThisTick { get; private set; }

        private static readonly int[] dx = { 0, 0, -1, 1 };
        private static readonly int[] dy = { -1, 1, 0, 0 };

        private static int FindRoot(int[] parent, int id)
        {
            if (id <= 0) return 0;
            while (parent[id] != id)
            {
                parent[id] = parent[parent[id]];
                id = parent[id];
            }
            return id;
        }

        public static void ComputePotential(StateGrid s, int radius, float scale)
        {
            // Clear potential
            Array.Clear(s.SinkPotential, 0, s.SinkPotential.Length);
            if (radius <= 0 || scale <= 0f) return;

            // We treat each sink cell's root mass as the "source strength"
            // and spread it out with a simple 1/(1+dist) kernel (Manhattan dist).
            for (int by = 0; by < s.H; by++)
            {
                for (int bx = 0; bx < s.W; bx++)
                {
                    int bIdx = s.Idx(bx, by);
                    if (!s.IsSink[bIdx]) continue;

                    int root = GetRootAtCell(bIdx, s.SinkId, s.SinkParent);
                    float mass = 1f;
                    if (root > 0 && root < s.SinkMass.Length)
                        mass = Mathf.Max(1f, s.SinkMass[root]);

                    // Spread influence within radius
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int y = by + dy;
                        if (y < 0 || y >= s.H) continue;

                        int rem = radius - Mathf.Abs(dy);
                        for (int dx = -rem; dx <= rem; dx++)
                        {
                            int x = bx + dx;
                            if (x < 0 || x >= s.W) continue;

                            int dist = Mathf.Abs(dx) + Mathf.Abs(dy);
                            float contrib = (mass / (1f + dist)) * scale;
                            s.SinkPotential[s.Idx(x, y)] += contrib;
                        }
                    }
                }
            }
        }


        private static int Union(int[] parent, float[] mass, int a, int b)
        {
            int ra = FindRoot(parent, a);
            int rb = FindRoot(parent, b);
            if (ra == 0) return rb;
            if (rb == 0) return ra;
            if (ra == rb) return ra;

            // Attach higher id to lower id for stability
            if (rb < ra)
            {
                int tmp = ra;
                ra = rb;
                rb = tmp;
            }

            parent[rb] = ra;
            mass[ra] += mass[rb];
            mass[rb] = 0f;
            return ra;
        }

        public static void ExpandSinkRegions(StateGrid s)
        {
            // Next-state copy
            bool[] next = (bool[])s.IsSink.Clone();

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);
                    if (!s.IsSink[i]) continue;

                    // For each neighbor of this sink:
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                            continue;

                        int ni = s.Idx(nx, ny);
                        if (s.IsSink[ni]) continue;

                        // Count sink neighbors around ni
                        int count = 0;
                        for (int dd = 0; dd < 4; dd++)
                        {
                            int nnx = nx + dx[dd];
                            int nny = ny + dy[dd];
                            if (nnx < 0 || nnx >= s.W || nny < 0 || nny >= s.H)
                                continue;

                            int nni = s.Idx(nnx, nny);
                            if (s.IsSink[nni]) count++;
                        }

                        // Convert if adjacent to >=2 sinks
                        if (count >= 2)
                        {
                            next[ni] = true;
                        }
                    }
                }
            }

            s.IsSink = next;

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);

                    // If this is a sink cell without an ID (newly grown), assign/merge it
                    if (s.IsSink[i] && s.SinkId[i] == 0)
                    {
                        AssignOrMergeAtCell(i, s.W, s.H,
                            s.IsSink, s.SinkId, s.SinkParent, s.SinkMass,
                            ref s.NextSinkId);
                    }
                }
            }
        }

        public static int Find(int id, int[] parent)
        {
            if (id <= 0) return 0;
            while (parent[id] != id)
            {
                parent[id] = parent[parent[id]];
                id = parent[id];
            }
            return id;
        }

        public static int GetRootAtCell(int idx, int[] sinkId, int[] sinkParent)
        {
            int id = (idx >= 0 && idx < sinkId.Length) ? sinkId[idx] : 0;
            return Find(id, sinkParent);
        }

        public static int AssignOrMergeAtCell(int idx, int width, int height,
            bool[] isSink, int[] sinkId, int[] sinkParent, float[] sinkMass, ref int nextSinkId)
        {
            int EnsureIdForCell(int idx, int[] sinkId, int[] sinkParent, float[] sinkMass, ref int nextSinkId)
            {
                if (sinkId[idx] > 0)
                {
                    return Find(sinkId[idx], sinkParent);
                }

                int newId = nextSinkId;
                nextSinkId++;
                sinkParent[newId] = newId;
                sinkMass[newId] = sinkMass[newId]; // no-op but explicit
                sinkId[idx] = newId;
                return newId;
            }

            int x = idx % width;
            int y = idx / width;

            // If already sink, ensure it has an id and merge with adjacent sinks
            if (isSink[idx])
            {
                int root = EnsureIdForCell(idx, sinkId, sinkParent, sinkMass, ref nextSinkId);

                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    int nIdx = ny * width + nx;
                    if (!isSink[nIdx]) continue;

                    int nRoot = Find(sinkId[nIdx], sinkParent);
                    if (nRoot == 0) continue;
                    root = Union(sinkParent, sinkMass, root, nRoot);
                }

                sinkId[idx] = root;
                return root;
            }

            // Not a sink yet: create and merge with adjacent sinks if any
            int chosenRoot = 0;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d];
                int ny = y + dy[d];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                int nIdx = ny * width + nx;
                if (!isSink[nIdx]) continue;

                int nRoot = Find(sinkId[nIdx], sinkParent);
                if (nRoot == 0) continue;

                if (chosenRoot == 0) chosenRoot = nRoot;
                else chosenRoot = Union(sinkParent, sinkMass, chosenRoot, nRoot);
            }

            if (chosenRoot == 0)
            {
                chosenRoot = nextSinkId++;
                sinkParent[chosenRoot] = chosenRoot;
            }

            isSink[idx] = true;
            sinkId[idx] = chosenRoot;
            return chosenRoot;
        }

        public static float SinkAbsorbResource(StateGrid s, float absorbFracPerTick = 0f, bool activeRegionOnly = false, float sinkRecoilFrac = 0f)
        {
            absorbFracPerTick = Mathf.Clamp01(absorbFracPerTick); 
            float drainedTotal = 0f;
            int drainedEdges = 0;
            float recoilTotal = 0f;

            for (int y = 0; y < s.H; y++)
            {
                for (int x = 0; x < s.W; x++)
                {
                    int i = s.Idx(x, y);
                    if (!s.IsSink[i]) continue;

                    s.ResourceLocal[i] = 0f;
                    s.ComplexityMetric[i] = 1f;
                    s.Active[i] = 0;

                    int root = FindRoot(s.SinkParent, s.SinkId[i]);

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                            continue;

                        int ni = s.Idx(nx, ny);
                        if (s.IsSink[ni]) continue;
                        if (activeRegionOnly && !s.ActiveRegion[ni]) continue;

                        float neighbourResource = s.ResourceLocal[ni];
                        if (neighbourResource <= 0f) continue;

                        float absorbed = neighbourResource * absorbFracPerTick;
                        s.ResourceLocal[ni] = neighbourResource - absorbed;
                        drainedTotal += absorbed;
                        drainedEdges++;
                        if (root > 0)
                            s.SinkMass[root] += absorbed;
                    }
                }
            }

            if (sinkRecoilFrac > 0f && drainedTotal > 0f)
            {
                float recoilShare = drainedTotal * sinkRecoilFrac / 4f;
                for (int y = 0; y < s.H; y++)
                {
                    for (int x = 0; x < s.W; x++)
                    {
                        int i = s.Idx(x, y);
                        if (!s.IsSink[i]) continue;

                        for (int d = 0; d < 4; d++)
                        {
                            int nx = x + dx[d];
                            int ny = y + dy[d];
                            if (nx < 0 || nx >= s.W || ny < 0 || ny >= s.H)
                                continue;

                            int ni = s.Idx(nx, ny);
                            if (s.IsSink[ni]) continue;
                            if (!s.ActiveRegion[ni]) continue;

                            s.Incoming[ni] += recoilShare;
                            recoilTotal += recoilShare;
                        }
                    }
                }
            }

            LastDrainedThisTick = drainedTotal;
            LastDrainedEdges = drainedEdges;
            LastDrained = drainedTotal;
            LastRecoilThisTick = recoilTotal;
            return drainedTotal;
        }
    }
}
