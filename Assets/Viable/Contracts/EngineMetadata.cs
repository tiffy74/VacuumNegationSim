namespace Viable.Contracts
{
    /// <summary>
    /// Engine metadata for version tracking and compatibility verification.
    /// </summary>
    public sealed class EngineMetadata
    {
        /// <summary>
        /// Human-readable engine name.
        /// </summary>
        public string EngineName { get; set; } = ContractVersions.EngineName;

        /// <summary>
        /// Semantic version of the engine implementation (e.g., "0.1.0-alpha").
        /// </summary>
        public string EngineVersion { get; set; } = ContractVersions.EngineVersion;

        /// <summary>
        /// Schema version for contract compatibility (e.g., "1.0").
        /// </summary>
        public string SchemaVersion { get; set; } = ContractVersions.SchemaVersion;

        /// <summary>
        /// Timestamp of result generation (ISO 8601 format).
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// Creates engine metadata with current version constants.
        /// </summary>
        public static EngineMetadata Current()
        {
            return new EngineMetadata
            {
                EngineName = ContractVersions.EngineName,
                EngineVersion = ContractVersions.EngineVersion,
                SchemaVersion = ContractVersions.SchemaVersion,
                Timestamp = System.DateTime.UtcNow.ToString("o")
            };
        }
    }
}
