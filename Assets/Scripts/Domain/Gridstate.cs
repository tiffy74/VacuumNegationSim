using System;

namespace Assets.Scripts.Domain
{
    /// <summary>
    /// Grid State - Pure Simulation State Container
    /// 
    /// Encapsulates all per-cell and per-entity state for a 2D simulation grid.
    /// Uses Structure-of-Arrays (SoA) memory layout for cache efficiency.
    /// 
    /// Design Philosophy:
    /// - Pure Data: No behavior beyond indexing and memory management
    /// - Zero Unity Dependencies: Can be unit tested independently
    /// - Cache-Friendly: Flat arrays enable sequential access patterns
    /// - Preallocated: All arrays allocated once at construction
    /// - Mutable: Modified in-place by simulation passes for performance
    /// 
    /// Memory Layout:
    ///   Per-cell data stored in separate arrays (Structure of Arrays):
    ///   Nlocal:  [cell0, cell1, cell2, ...]  (contiguous memory)
    ///   Entropy: [cell0, cell1, cell2, ...]  (contiguous memory)
    ///   V:       [cell0, cell1, cell2, ...]  (contiguous memory)
    ///   
    ///   Benefits: Sequential access, SIMD-friendly, better CPU cache utilization
    ///   
    ///   Alternative (worse for performance - Array of Structures):
    ///   cells: [Cell{N, E, V, ...}, Cell{N, E, V, ...}, ...]  (scattered memory)
    /// 
    /// Lifetime: Created once at simulation start, reused throughout execution.
    ///           Never destroyed - just reset when restarting simulation.
    /// </summary>
    public sealed class GridState
    {
        // ============================================================================
        // PUBLIC READONLY FIELDS: GRID DIMENSIONS
        // ============================================================================

        /// <summary>
        /// Grid width (number of columns).
        /// Immutable after construction.
        /// </summary>
        public readonly int W;

        /// <summary>
        /// Grid height (number of rows).
        /// Immutable after construction.
        /// </summary>
        public readonly int H;

        /// <summary>
        /// Total cell count (W * H).
        /// Cached for convenience - all per-cell arrays have this length.
        /// </summary>
        public readonly int Len;

        // ============================================================================
        // PUBLIC FIELDS: CORE PER-CELL STATE
        // ============================================================================

        /// <summary>
        /// Local Energy per Cell
        /// Represents the energy "stored" at each grid location.
        /// Modified by: Pass2 (apply incoming), Pass1 (deduct outgoing), decay
        /// Physical Analogy: Energy density field ρ(x,y)
        /// Units: Arbitrary energy units
        /// Range: [0, NlocalMax]
        /// </summary>
        public float[] Nlocal;

        /// <summary>
        /// Entropy per Cell [0, 1]
        /// Represents information-theoretic complexity / disorder.
        /// Computed from: Configuration count + energy gradients
        /// Modified by: Pass2 (compute), Pass4 (diffuse)
        /// Physical Analogy: Thermodynamic entropy S
        /// Range: [0, 1] (normalized)
        /// </summary>
        public float[] Entropy;

        /// <summary>
        /// Viability per Cell
        /// Thermodynamic persistence criterion: V > 0 means structure persists.
        /// Formula: V = (inflow * entropy_boost - decay) / threshold
        /// Modified by: Pass2 (compute from incoming flow, energy, entropy)
        /// Physical Analogy: Gibbs free energy ΔG (negative = spontaneous)
        /// Range: Typically [-1, 1] but unbounded
        /// </summary>
        public float[] V;

        /// <summary>
        /// Active State per Cell (0 = inactive, 1 = active)
        /// Determines if cell participates in energy propagation.
        /// Condition: Active = (V > 0 AND Nlocal > MinBudgetToPropagate)
        /// Modified by: Pass2 (update based on viability)
        /// Uses byte (not bool) for potential multi-state expansion
        /// </summary>
        public byte[] Active;

        // ============================================================================
        // PUBLIC FIELDS: BUFFERS & TRANSIENT STATE
        // ============================================================================

        /// <summary>
        /// Incoming Energy Buffer (Pass1 → Pass2)
        /// Energy destined for each cell from neighbors during propagation.
        /// Lifecycle: Filled by Pass1, consumed by Pass2, cleared each tick
        /// Physical Analogy: Energy flux into control volume
        /// Units: Energy per tick
        /// </summary>
        public float[] Incoming;

        /// <summary>
        /// Entropy Double-Buffer (Pass4 diffusion)
        /// Work array for entropy diffusion to enable simultaneous updates.
        /// Lifecycle: Written by Pass4, swapped with Entropy[], reused next tick
        /// Prevents read-modify-write conflicts during Laplacian computation
        /// </summary>
        public float[] EntropyNext;

        /// <summary>
        /// Vacuum State per Cell
        /// Marks cells as permanent/semi-permanent vacuum (no energy allowed).
        /// Modified by: Vacuum event logic (rare stochastic collapse)
        /// Could be promoted to: Permanent vacuum regions in future
        /// </summary>
        public bool[] IsVacuum;

        // ============================================================================
        // PUBLIC FIELDS: CONFIGURATION SPACE GEOMETRY
        // ============================================================================

        /// <summary>
        /// Field Presence per Cell (Configuration Space Active)
        /// True = configuration space is "activated" at this location.
        /// False = void (latent configuration space, no energy propagation)
        /// Modified by: FieldWave (propagation at boundaries)
        /// Physical Analogy: Active/curved spacetime (GR), quantum field vacuum state
        /// </summary>
        public bool[] FieldPresent;

        /// <summary>
        /// Field Arrival Tick per Cell
        /// Tick number when configuration space first activated at this location.
        /// -1 = field has not yet arrived
        /// Used for: Visualization (arrival ring), diagnostics, frontier detection
        /// Never decreases once set
        /// </summary>
        public int[] FieldFirstTick;

        /// <summary>
        /// Energy Arrival Tick per Cell
        /// Tick number when energy first arrived at this location.
        /// -1 = energy has never arrived
        /// Used for: Diagnostics, propagation speed analysis
        /// Never decreases once set
        /// </summary>
        public int[] EnergyFirstTick;

        // ============================================================================
        // PUBLIC FIELDS: BLACK HOLE STATE (PER-CELL)
        // ============================================================================

        /// <summary>
        /// Black Hole Presence per Cell
        /// True = this cell belongs to a black hole (collapsed configuration space).
        /// Modified by: Pass1 (formation at boundaries), BlackHoles.GrowBlackHoles()
        /// Physical Analogy: Inside event horizon (r < r_Schwarzschild)
        /// Invariant: IsBlackHole[i] = true → Nlocal[i] = 0, Entropy[i] = 1, Active[i] = 0
        /// </summary>
        public bool[] IsBlackHole;

        /// <summary>
        /// Black Hole Charge per Cell (Boundary Void Accumulation)
        /// Energy "leaked" into boundary voids accumulates as charge.
        /// When charge exceeds threshold → black hole forms.
        /// Modified by: Pass1 (accumulate at boundary voids), Pass1 (reset on formation)
        /// Physical Analogy: Energy density approaching critical collapse threshold
        /// Units: Energy (accumulated over multiple ticks)
        /// </summary>
        public float[] BlackHoleCharge;

        /// <summary>
        /// Black Hole Gravitational Potential per Cell (Optional)
        /// Pre-computed potential field from all black holes.
        /// Used by: Optional gravitational bias in energy propagation
        /// Modified by: BlackHoles.ComputePotential() (currently unused in main logic)
        /// Physical Analogy: Gravitational potential Φ(x,y)
        /// </summary>
        public float[] BlackHolePotential;

        // ============================================================================
        // PUBLIC FIELDS: BLACK HOLE ENTITY TRACKING (UNION-FIND)
        // ============================================================================

        /// <summary>
        /// Black Hole Entity ID per Cell
        /// Maps cell index to black hole entity ID (0 = no black hole).
        /// Multiple adjacent cells can share same ID (merged black hole).
        /// Modified by: BlackHoles.AssignOrMergeAtCell() (union-find assignment)
        /// Indexed by: Cell index [0, Len)
        /// Values: 0 = no BH, >0 = BH entity ID
        /// </summary>
        public int[] BlackHoleId;

        /// <summary>
        /// Black Hole Union-Find Parent Pointers
        /// Implements union-find data structure for BH entity merging.
        /// Parent[id] = id means id is a root (representative of merged group).
        /// Modified by: BlackHoles.Union() (path compression during merge)
        /// Indexed by: BH entity ID [1, NextBlackHoleId)
        /// NOT indexed by cell index!
        /// </summary>
        public int[] BlackHoleParent;

        /// <summary>
        /// Black Hole Mass per Entity
        /// Total accumulated mass for each merged black hole entity.
        /// Indexed by: Root BH entity ID (use Find() to get root from cell)
        /// Modified by: BlackHoles.Union() (sum masses), BlackHoleAttractEnergy() (accretion)
        /// Physical Analogy: Black hole mass M (determines Schwarzschild radius)
        /// Units: Accumulated energy
        /// </summary>
        public float[] BlackHoleMass;

        /// <summary>
        /// Next Available Black Hole Entity ID
        /// Monotonically increasing counter for assigning new BH IDs.
        /// Starts at 1 (0 reserved for "no black hole").
        /// Modified by: BlackHoles.AssignOrMergeAtCell(), CreateBlackHoleEntity()
        /// </summary>
        public int NextBlackHoleId = 1;

        // ============================================================================
        // PUBLIC FIELDS: DIAGNOSTICS & TRACKING
        // ============================================================================

        /// <summary>
        /// Zero Energy Tick Counter per Cell
        /// Counts consecutive ticks with zero energy.
        /// Could be used for: Vacuum promotion logic (sustained zero energy)
        /// Modified by: Pass2 (increment if Nlocal[i] ≤ 0, reset if > 0)
        /// </summary>
        public int[] ZeroEnergyTicks;

        // ============================================================================
        // CONSTRUCTOR
        // ============================================================================

        /// <summary>
        /// Initializes grid state with specified dimensions.
        /// Allocates all per-cell and per-entity arrays.
        /// </summary>
        /// <param name="width">Grid width (columns)</param>
        /// <param name="height">Grid height (rows)</param>
        /// <param name="initialBlackHoleCapacity">Initial size for BH entity arrays (grows as needed)</param>
        /// <exception cref="ArgumentOutOfRangeException">If width or height ≤ 0</exception>
        public GridState(int width, int height, int initialBlackHoleCapacity = 8192)
        {
            ValidateGridDimensions(width, height);
            
            W = width;
            H = height;
            Len = W * H;

            AllocatePerCellArrays();
            AllocateBlackHoleEntityArrays(initialBlackHoleCapacity);

            Reset();
        }

        // ============================================================================
        // PUBLIC METHODS: INDEXING
        // ============================================================================

        /// <summary>
        /// Converts 2D grid coordinates to 1D array index.
        /// Formula: index = y * width + x (row-major order)
        /// </summary>
        /// <param name="x">Column index [0, W)</param>
        /// <param name="y">Row index [0, H)</param>
        /// <returns>Flat array index [0, Len)</returns>
        public int Idx(int x, int y) => (y * W) + x;

        // ============================================================================
        // PUBLIC METHODS: STATE MANAGEMENT
        // ============================================================================

        /// <summary>
        /// Resets all state arrays to default values.
        /// Called at simulation start and when restarting.
        /// 
        /// Reset Behavior:
        /// - Per-cell arrays: Zeroed (except FieldFirstTick/EnergyFirstTick = -1)
        /// - BH entities: Cleared, NextBlackHoleId = 1
        /// - Does NOT deallocate arrays (reuses memory)
        /// </summary>
        public void Reset()
        {
            ClearPerCellArrays();
            ResetBlackHoleEntityTracking();
        }

        /// <summary>
        /// Ensures black hole entity arrays can hold at least 'requiredId'.
        /// Grows arrays by doubling capacity if needed.
        /// Called automatically when creating new BH entities.
        /// </summary>
        /// <param name="requiredId">Minimum required capacity</param>
        public void EnsureBlackHoleCapacity(int requiredId)
        {
            if (requiredId < BlackHoleParent.Length)
                return; // Sufficient capacity

            int newCapacity = CalculateNewCapacity(requiredId);
            ResizeBlackHoleEntityArrays(newCapacity);
        }

        /// <summary>
        /// Creates a new black hole entity with initial mass.
        /// Allocates a fresh entity ID and initializes union-find structure.
        /// 
        /// Note: Caller must still:
        /// - Set IsBlackHole[cellIndex] = true
        /// - Set BlackHoleId[cellIndex] = returnedId
        /// </summary>
        /// <param name="initialMass">Initial mass (default: 1.0)</param>
        /// <returns>New black hole entity ID</returns>
        public int CreateBlackHoleEntity(float initialMass = 1f)
        {
            int id = NextBlackHoleId++;
            EnsureBlackHoleCapacity(id + 1);

            InitializeBlackHoleEntity(id, initialMass);

            return id;
        }

        // ============================================================================
        // PRIVATE METHODS: INITIALIZATION
        // ============================================================================

        /// <summary>
        /// Validates grid dimensions are positive.
        /// </summary>
        private void ValidateGridDimensions(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive");
        }

        /// <summary>
        /// Allocates all per-cell state arrays.
        /// </summary>
        private void AllocatePerCellArrays()
        {
            // Core state
            Nlocal = new float[Len];
            Entropy = new float[Len];
            V = new float[Len];
            Active = new byte[Len];

            // Buffers
            Incoming = new float[Len];
            EntropyNext = new float[Len];
            IsVacuum = new bool[Len];

            // Geometry
            FieldPresent = new bool[Len];
            FieldFirstTick = new int[Len];
            EnergyFirstTick = new int[Len];

            // Black holes (per-cell)
            IsBlackHole = new bool[Len];
            BlackHoleCharge = new float[Len];
            BlackHoleId = new int[Len];
            BlackHolePotential = new float[Len];

            // Diagnostics
            ZeroEnergyTicks = new int[Len];
        }

        /// <summary>
        /// Allocates black hole entity tracking arrays (union-find).
        /// Indexed by BH entity ID, not cell index.
        /// </summary>
        private void AllocateBlackHoleEntityArrays(int initialCapacity)
        {
            if (initialCapacity < 16)
                initialCapacity = 16; // Minimum capacity

            BlackHoleParent = new int[initialCapacity];
            BlackHoleMass = new float[initialCapacity];
        }

        // ============================================================================
        // PRIVATE METHODS: RESET
        // ============================================================================

        /// <summary>
        /// Clears all per-cell arrays to default values.
        /// </summary>
        private void ClearPerCellArrays()
        {
            // Zero arrays
            Array.Clear(Nlocal, 0, Len);
            Array.Clear(Entropy, 0, Len);
            Array.Clear(V, 0, Len);
            Array.Clear(Active, 0, Len);
            Array.Clear(Incoming, 0, Len);
            Array.Clear(EntropyNext, 0, Len);
            Array.Clear(IsVacuum, 0, Len);
            Array.Clear(FieldPresent, 0, Len);
            Array.Clear(IsBlackHole, 0, Len);
            Array.Clear(BlackHoleCharge, 0, Len);
            Array.Clear(BlackHoleId, 0, Len);
            Array.Clear(ZeroEnergyTicks, 0, Len);
            Array.Clear(BlackHolePotential, 0, Len);

            // Special initialization for arrival ticks (-1 = not yet arrived)
            for (int i = 0; i < Len; i++)
            {
                FieldFirstTick[i] = -1;
                EnergyFirstTick[i] = -1;
            }
        }

        /// <summary>
        /// Resets black hole entity tracking to initial state.
        /// </summary>
        private void ResetBlackHoleEntityTracking()
        {
            NextBlackHoleId = 1; // IDs start at 1 (0 = no BH)
            Array.Clear(BlackHoleParent, 0, BlackHoleParent.Length);
            Array.Clear(BlackHoleMass, 0, BlackHoleMass.Length);
        }

        // ============================================================================
        // PRIVATE METHODS: BLACK HOLE ENTITY MANAGEMENT
        // ============================================================================

        /// <summary>
        /// Calculates new capacity for BH entity arrays (doubling strategy).
        /// </summary>
        private int CalculateNewCapacity(int requiredId)
        {
            int newCap = BlackHoleParent.Length;
            while (newCap <= requiredId)
                newCap *= 2;
            return newCap;
        }

        /// <summary>
        /// Resizes black hole entity arrays to new capacity.
        /// </summary>
        private void ResizeBlackHoleEntityArrays(int newCapacity)
        {
            Array.Resize(ref BlackHoleParent, newCapacity);
            Array.Resize(ref BlackHoleMass, newCapacity);
        }

        /// <summary>
        /// Initializes a new black hole entity in union-find structure.
        /// </summary>
        private void InitializeBlackHoleEntity(int id, float initialMass)
        {
            BlackHoleParent[id] = id; // Self-parent = root
            BlackHoleMass[id] = Math.Max(1f, initialMass); // Minimum mass = 1
        }
    }
}
