using UnityEngine;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Contract for UI sections that bind to WorkingScenarioConfig.
    /// Sections can show/hide themselves based on config context.
    /// </summary>
    public interface IConfigSection
    {
        /// <summary>
        /// Bind controls to the config (populate from config values).
        /// </summary>
        void Bind(Configuration.WorkingScenarioConfig config);

        /// <summary>
        /// Update visibility based on config context (e.g., hide irrelevant sections).
        /// </summary>
        void RefreshVisibility(Configuration.WorkingScenarioConfig config);

        /// <summary>
        /// Apply current UI values back to config.
        /// </summary>
        void ApplyEdits(Configuration.WorkingScenarioConfig config);
    }
}
