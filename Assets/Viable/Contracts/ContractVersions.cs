namespace Viable.Contracts
{
    /// <summary>
    /// Schema versioning constants for contract compatibility tracking.
    /// </summary>
    public static class ContractVersions
    {
        /// <summary>
        /// Current schema version for all contract types.
        /// Format: MAJOR.MINOR (semantic versioning)
        /// </summary>
        public const string SchemaVersion = "1.0";

        /// <summary>
        /// Engine version identifier (semantic versioning).
        /// </summary>
        public const string EngineVersion = "0.1.0-alpha";

        /// <summary>
        /// Engine name constant.
        /// </summary>
        public const string EngineName = "VIABLE Core Engine";
    }
}
