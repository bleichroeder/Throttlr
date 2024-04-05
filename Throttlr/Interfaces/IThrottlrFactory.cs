using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Throttlr.Caching;

namespace Throttlr.Interfaces
{
    /// <summary>
    /// The <see cref="IRabbitPublisherFactory"/> interface.
    /// </summary>
    public interface IThrottlrFactory
    {
        /// <summary>
        /// Creates a new <see cref="IThrottlr{T}"/>.
        /// Uses the type name of <typeparamref name="T"/> as the name.
        /// Creates a default <see cref="MemoryCacheConfiguration"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="redisDatabase"></param>
        /// <returns></returns>
        IThrottlr<T> CreateThrottlr<T>(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisDatabase);

        /// <summary>
        /// Creates a new <see cref="IThrottlr{T}"/>.
        /// Uses the type name of <typeparamref name="T"/> as the name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="redisDatabase"></param>
        /// <returns></returns>
        IThrottlr<T> CreateThrottlr<T>(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisDatabase, IMemoryCacheConfiguration memoryCacheConfiguration);

        /// <summary>
        /// Creates a new <see cref="IThrottlr{T}"/>.
        /// Creates a default <see cref="MemoryCacheConfiguration"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="redisDatabase"></param>
        /// <returns></returns>
        IThrottlr<T> CreateThrottlr<T>(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisDatabase, string name);

        /// <summary>
        /// Creates a new named <see cref="IThrottlr{T}"/>.
        /// Uses the specified name and <see cref="IMemoryCacheConfiguration"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="configuration"></param>
        /// <param name="redisDatabase"></param>
        /// <returns></returns>
        IThrottlr<T> CreateThrottlr<T>(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisDatabase, IMemoryCacheConfiguration memoryCacheConfiguration, string name, ILogger? logger = null);

        /// <summary>
        /// Gets the <see cref="IThrottlr{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        IThrottlr<T> GetThrottlr<T>(string? name = null);
    }
}
