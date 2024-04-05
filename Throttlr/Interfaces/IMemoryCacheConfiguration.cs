using System.Text.Json;

namespace Throttlr
{
    /// <summary>
    /// The <see cref="IMemoryCacheConfiguration"/> interface.
    /// </summary>
    public interface IMemoryCacheConfiguration
    {
        /// <summary>
        /// Gets the CleanupInterval.
        /// </summary>
        public TimeSpan CleanupInterval { get; }

        /// <summary>
        /// Gets the JsonSerializerOptions.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; }
    }
}
