using System.Text.Json;
using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Utilities;

namespace Throttlr.Windows.Configuration
{
    public abstract class ThrottlrWindowConfigurationBase<T> : IThrottlrConfiguration<T>
    {
        /// <summary>
        /// Gets or sets the LimiterType.
        /// </summary>
        public abstract LimiterType LimiterType { get; }

        /// <summary>
        /// Gets the window type.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetWindowType();

        /// <summary>
        /// Gets or sets the ThrottlrName.
        /// </summary>
        public string Name { get; set; } = typeof(T).Name;

        /// <summary>
        /// Gets or sets the RedisNamespace
        /// </summary>
        public string Namespace { private get; set; } = typeof(T).Name;

        /// <summary>
        /// Gets or sets the KeyBuilder delegate.
        /// The key is used for accessing and storing windows in the cache.
        /// </summary>
        public Func<T, string> KeyBuilder { get; set; } = (item) => typeof(T).ToString();

        /// <summary>
        /// Gets or sets the MaximumRequests allowed within the TimeWindow.
        /// </summary>
        public long Maximum { get; set; } = 100;

        /// <summary>
        /// Gets or sets the TimeWindow.
        /// </summary>
        public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(60);

        /// <summary>
        /// On success delegate.
        /// The action to be performed when the request is not throttled.
        /// </summary>
        public Func<ThrottlResult<T>, Task<bool>> OnSuccess { get; set; } = (item) => Task.FromResult(true);

        /// <summary>
        /// On failure delegate.
        /// The action to be performed when the request is throttled.
        /// </summary>
        public Func<ThrottlResult<T>, Task<bool>> OnFailure { get; set; } = (item) => Task.FromResult(true);

        /// <summary>
        /// Builds and returns the Redis key using
        /// the keybuilder and specified namespace if it's not null or empty.
        /// </summary>
        /// <remarks>
        /// Placeholders:
        /// <br/>
        /// {name} - Replaced with <see cref="Name"/>.
        /// </remarks>
        /// <returns></returns>
        public string BuildKey(T item) => $"{(!string.IsNullOrEmpty(Namespace) ? Namespace + ':' : string.Empty)}{KeyBuilder(item)}"
                                                .Replace("{name}", Name);

        /// <summary>
        /// Gets or sets the JsonSerializerOptions.
        /// </summary>
        /// <value>
        /// Default: <see cref="Serialization.DefaultSerializerOptions"/>.
        /// </value>
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = Serialization.DefaultSerializerOptions;
    }
}
