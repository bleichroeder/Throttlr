using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Throttlr.Caching;
using Throttlr.Interfaces;
using Throttlr.Windows.Configuration;

namespace Throttlr.Models
{
    /// <summary>
    /// ThrottlrContext.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ThrottlrConfigurationContext"/> class.
    /// </remarks>
    /// <param name="factory"></param>
    public class ThrottlrConfigurationContext(IThrottlrFactory factory)
    {
        private readonly IThrottlrFactory _factory = factory;

        private IConnectionMultiplexer? _globalConnection;
        private IMemoryCacheConfiguration? _globalMemoryCacheConfiguration;

        /// <summary>
        /// True if rules have been loaded into the <see cref="ThrottlrRuleCache"/> via the <see cref="ThrottlrRuleCache.LoadRules"/> method.
        /// </summary>
        public bool RulesLoaded { get; internal set; }

        /// <summary>
        /// Sets the global Redis connection.
        /// </summary>
        /// <param name="globalConnection"></param>
        /// <returns></returns>
        public ThrottlrConfigurationContext UseGlobalRedisConnection(IConnectionMultiplexer globalConnection)
        {
            _globalConnection = globalConnection;
            return this;
        }

        /// <summary>
        /// Sets the global MemoryCacheConfiguration.
        /// </summary>
        /// <param name="globalMemoryCacheConfiguration"></param>
        /// <returns></returns>
        public ThrottlrConfigurationContext UseGlobalMemoryCacheConfiguration(IMemoryCacheConfiguration globalMemoryCacheConfiguration)
        {
            _globalMemoryCacheConfiguration = globalMemoryCacheConfiguration;
            return this;
        }

        /// <summary>
        /// Loads the specified rules into the <see cref="ThrottlrRuleCache"/>.
        /// </summary>
        /// <param name="throttlrRules"></param>
        /// <returns></returns>
        public ThrottlrConfigurationContext UseRules(IEnumerable<IThrottlrRule> throttlrRules)
        {
            ThrottlrRuleCache.Default.AddOrUpdateRules(throttlrRules);

            RulesLoaded = true;

            return this;
        }

        /// <summary>
        /// Deserializes the rules from the specified JSON file and loads them into the <see cref="ThrottlrRuleCache"/>.
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        public ThrottlrConfigurationContext UseRules(string jsonPath)
            => UseRules(new[] { jsonPath });

        /// <summary>
        /// Deserializes the rules from the specified JSON files and loads them into the <see cref="ThrottlrRuleCache"/>.
        /// </summary>
        /// <param name="jsonPaths"></param>
        /// <returns></returns>
        public ThrottlrConfigurationContext UseRules(IEnumerable<string> jsonPaths)
        {
            ThrottlrRuleCache.Default.LoadRules(jsonPaths).GetAwaiter().GetResult();

            RulesLoaded = true;

            return this;
        }

        /// <summary>
        /// Configures and creates a new Throttlr with a fixed window.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure"></param>
        /// <param name="redisConnection"></param>
        /// <param name="memoryCacheConfiguration"></param>
        /// <param name="throttlrName"></param>
        /// <returns></returns>
        public ThrottlrConfigurationContext AddFixedThrottlr<T>(Func<IWindowConfigurationContext<T>.IInitialization_WindowType, IWindowConfigurationContext<T>.IConfigure_Optional> configure,
                                                                IConnectionMultiplexer? redisConnection = null,
                                                                IMemoryCacheConfiguration? memoryCacheConfiguration = null)
        {
            IWindowConfigurationContext<T>.IConfigure_Optional configurationContext = configure(WindowConfigurationContextBase<T>.Start().WithWindowType(WindowType.Fixed));
            IThrottlrConfiguration<T> builtConfiguration = configurationContext.Build();
            AddThrottlr(builtConfiguration, redisConnection, memoryCacheConfiguration, builtConfiguration.Name);
            return this;
        }

        /// <summary>
        /// Configures and creates a new Throttlr with a sliding window.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure"></param>
        /// <param name="redisConnection"></param>
        /// <param name="memoryCacheConfiguration"></param>
        /// <param name="throttlrName"></param>
        /// <returns></returns>
        public ThrottlrConfigurationContext AddSlidingThrottlr<T>(Func<IWindowConfigurationContext<T>.IInitialization_WindowType, IWindowConfigurationContext<T>.IConfigure_Optional> configure,
                                                                  IConnectionMultiplexer? redisConnection = null,
                                                                  IMemoryCacheConfiguration? memoryCacheConfiguration = null)
        {
            IWindowConfigurationContext<T>.IConfigure_Optional configurationContext = configure(WindowConfigurationContextBase<T>.Start().WithWindowType(WindowType.Sliding));
            IThrottlrConfiguration<T> builtConfiguration = configurationContext.Build();
            AddThrottlr(builtConfiguration, redisConnection, memoryCacheConfiguration, builtConfiguration.Name);
            return this;
        }


        /// <summary>
        /// Creates a new Throttlr.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure"></param>
        /// <param name="configuration"></param>
        /// <param name="redisConnection"></param>
        /// <param name="memoryCacheConfiguration"></param>
        /// <param name="throttlrName"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void AddThrottlr<T>(IThrottlrConfiguration<T> configuration,
                                    IConnectionMultiplexer? redisConnection,
                                    IMemoryCacheConfiguration? memoryCacheConfiguration,
                                    string? throttlrName)
        {
            redisConnection ??= _globalConnection;

            if (redisConnection is null)
                throw new InvalidOperationException("Either a specific Redis connection or a global connection must be provided.");

            memoryCacheConfiguration ??= _globalMemoryCacheConfiguration ?? new MemoryCacheConfiguration(TimeSpan.FromHours(1));
            throttlrName ??= typeof(T).Name;

            _factory.CreateThrottlr(configuration,
                                    redisConnection,
                                    memoryCacheConfiguration,
                                    throttlrName,
                                    GetThrottlrLogger<T>());
        }

        /// <summary>
        /// Gets a logger for the Throttlr.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static ILogger GetThrottlrLogger<T>()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, LogLevel.Debug)
                    .AddConsole();
            });

            return loggerFactory.CreateLogger<IThrottlr<T>>();
        }
    }
}