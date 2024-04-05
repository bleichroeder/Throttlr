using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Reflection;
using Throttlr.Caching;
using Throttlr.Interfaces;

namespace Throttlr
{
    /// <summary>
    /// The <see cref="ThrottlrFactory"/>.
    /// </summary>
    public class ThrottlrFactory : IThrottlrFactory
    {
        private static readonly ConcurrentDictionary<string, object> _instances = new();

        private static readonly object _padLock = new();
        private static ThrottlrFactory? _defaultInstance = null;

        /// <summary>
        /// Gets the default instance of <see cref="ThrottlrFactory"/>.
        /// This instance acts as a Singleton for direct access outside DI.
        /// </summary>
        public static ThrottlrFactory Default
        {
            get
            {
                if (_defaultInstance is null)
                {
                    lock (_padLock)
                    {
                        _defaultInstance ??= new ThrottlrFactory();
                    }
                }

                return _defaultInstance;
            }
        }

        /// <summary>
        /// Creates a new <see cref="IThrottlr{T}"/>.
        /// Uses the type name of <typeparamref name="T"/> as the name.
        /// Creates a default <see cref="MemoryCacheConfiguration"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="redisDatabase"></param>
        /// <returns></returns>
        public IThrottlr<T> CreateThrottlr<T>(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisDatabase)
            => CreateThrottlr(configuration, redisDatabase, new MemoryCacheConfiguration(TimeSpan.FromHours(1)), typeof(T).Name);

        /// <summary>
        /// Creates a new <see cref="IThrottlr{T}"/>.
        /// Uses the type name of <typeparamref name="T"/> as the name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="redisDatabase"></param>
        /// <returns></returns>
        public IThrottlr<T> CreateThrottlr<T>(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisDatabase, IMemoryCacheConfiguration memoryCacheConfiguration)
            => CreateThrottlr(configuration, redisDatabase, memoryCacheConfiguration, typeof(T).Name);

        /// <summary>
        /// Creates a new <see cref="IThrottlr{T}"/>.
        /// Creates a default <see cref="MemoryCacheConfiguration"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="redisDatabase"></param>
        /// <returns></returns>
        public IThrottlr<T> CreateThrottlr<T>(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisDatabase, string name)
            => CreateThrottlr(configuration, redisDatabase, new MemoryCacheConfiguration(TimeSpan.FromHours(1)), name);

        /// <summary>
        /// Creates a new named <see cref="IThrottlr{T}"/>.
        /// Uses the specified name and <see cref="IMemoryCacheConfiguration"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="throttlrName"></param>
        /// <param name="configuration"></param>
        /// <param name="redisDatabase"></param>
        /// <returns></returns>
        public IThrottlr<T> CreateThrottlr<T>(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisDatabase, IMemoryCacheConfiguration memoryCacheConfiguration, string throttlrName, ILogger? logger = null)
        {
            if (logger is null)
            {
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter(Assembly.GetExecutingAssembly().GetName().Name, LogLevel.Debug)
                        .AddConsole();
                });

                logger ??= loggerFactory.CreateLogger<IThrottlr<T>>();
            }

            if (_instances.ContainsKey(throttlrName))
            {
                throw new ArgumentException($"A Throttlr instance with the name '{throttlrName}' already exists.\n" +
                                            $"Ensure that if multiple {nameof(IThrottlr<T>)} of the same type '{nameof(T)}' are created, they are given a custom name.");
            }

            _instances.TryAdd(throttlrName, new Throttlr<T>(configuration, redisDatabase, memoryCacheConfiguration, throttlrName, logger));

            return (IThrottlr<T>)_instances[throttlrName];
        }

        /// <summary>
        /// Gets an existing <see cref="IThrottlr{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public IThrottlr<T> GetThrottlr<T>(string? name = null)
        {
            string instanceKey = !string.IsNullOrEmpty(name)
                ? name
                : typeof(T).Name;

            return (IThrottlr<T>)_instances[instanceKey];
        }
    }
}
