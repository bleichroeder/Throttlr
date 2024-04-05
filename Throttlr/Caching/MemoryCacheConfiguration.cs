using System.Text.Json;
using Throttlr.Utilities;

namespace Throttlr.Caching
{
    /// <summary>
    /// The <see cref="MemoryCacheConfiguration"/>.
    /// </summary>
    /// <remarks>
    /// Allows configuration of the <see cref="MemoryCache"/> using the <see cref="IMemoryCacheConfiguration"/> interface.
    /// </remarks>
    /// <remarks>
    /// Creates a new <see cref="MemoryCacheConfiguration"/>.
    /// </remarks>
    /// <remarks>
    /// Uses the specified <paramref name="cleanupInterval"/> and <paramref name="jsonSerializerOptions"/>.
    /// </remarks>
    /// <param name="cleanupInterval"></param>
    /// <param name="jsonSerializerOptions"></param>
    public class MemoryCacheConfiguration(TimeSpan cleanupInterval, JsonSerializerOptions jsonSerializerOptions) : IMemoryCacheConfiguration
    {
        /// <summary>
        /// Gets or sets the CleanupInterval.
        /// </summary>
        /// <value>Default: 1 Minute</value>
        public TimeSpan CleanupInterval { get; set; } = cleanupInterval;

        /// <summary>
        /// Gets or sets the JsonSerializerOptions.
        /// </summary>
        /// <value>Default: WriteIndented = true</value>
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = jsonSerializerOptions ?? Serialization.DefaultSerializerOptions;

        /// <summary>
        /// Creates a new <see cref="MemoryCacheConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// Uses the specified <paramref name="cleanupInterval"/> and the default <see cref="JsonSerializerOptions"/>.
        /// </remarks>
        /// <param name="cleanupInterval"></param>
        public MemoryCacheConfiguration(TimeSpan cleanupInterval)
            : this(cleanupInterval, Serialization.DefaultSerializerOptions)
        {

        }
    }
}
