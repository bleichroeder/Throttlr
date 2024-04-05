using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Throttlr.Caching;
using Throttlr.Interfaces;
using Throttlr.Models;

namespace Throttlr.DependencyInjection
{

    /// <summary>
    /// The <see cref="ThrottlrFactory"/> extensions.
    /// </summary>
    public static class ThrottlrFactoryExtensions
    {
        /// <summary>
        /// Configures the <see cref="ThrottlrFactory"/> with the specified <see cref="ThrottlrConfigurationContext"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureThrottlr(this IServiceCollection services, Action<ThrottlrConfigurationContext> configure)
        {
            EnsureFactoryRegistration(services);

            IThrottlrFactory factory = GetThrottlrFactory(services);

            ThrottlrConfigurationContext context = new(factory);

            if (context.RulesLoaded is true)
                EnsureRuleCacheRegistration(services);

            configure(context);

            return services;
        }

        /// <summary>
        /// Registers a collection of <see cref="IThrottlrRule"/> with the <see cref="IThrottlrRuleCache"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="rulesFunction"></param>
        /// <returns></returns>
        public static IServiceCollection UseThrottlrRules(this IServiceCollection services, Func<IEnumerable<IThrottlrRule>> rulesFunction)
        {
            EnsureRuleCacheRegistration(services);

            IThrottlrRuleCache ruleCache = GetThrottlrRuleCache(services);

            ruleCache.AddOrUpdateRules(rulesFunction());

            return services;
        }

        /// <summary>
        /// Adds the <see cref="ThrottlrFactory"/> to the <see cref="IServiceCollection"/>.
        /// Uses the type name of <typeparamref name="T"/> as the name.
        /// Uses the default <see cref="MemoryCacheConfiguration"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="redisConnection"></param>
        /// <returns></returns>
        public static IServiceCollection AddThrottlr<T>(this IServiceCollection services,
                                                        IThrottlrConfiguration<T> configuration,
                                                        IConnectionMultiplexer redisConnection)
            => AddThrottlr(services, configuration, redisConnection, new MemoryCacheConfiguration(TimeSpan.FromHours(1)), typeof(T).Name);

        /// <summary>
        /// Adds a <see cref="IThrottlr{T}"/> to the <see cref="IServiceCollection"/>.
        /// Uses the type name of <typeparamref name="T"/> as the name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="redisConnection"></param>
        /// <param name="memoryCacheConfiguration"></param>
        /// <returns></returns>
        public static IServiceCollection AddThrottlr<T>(this IServiceCollection services,
                                                        IThrottlrConfiguration<T> configuration,
                                                        IConnectionMultiplexer redisConnection,
                                                        IMemoryCacheConfiguration memoryCacheConfiguration)
            => AddThrottlr(services, configuration, redisConnection, memoryCacheConfiguration, typeof(T).Name);

        /// <summary>
        /// Adds a <see cref="IThrottlr{T}"/> to the <see cref="IServiceCollection"/>.
        /// Uses the specified name as the name.
        /// Creates a default <see cref="MemoryCacheConfiguration"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="redisConnection"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IServiceCollection AddThrottlr<T>(this IServiceCollection services,
                                                        IThrottlrConfiguration<T> configuration,
                                                        IConnectionMultiplexer redisConnection,
                                                        string name)
            => AddThrottlr(services, configuration, redisConnection, new MemoryCacheConfiguration(TimeSpan.FromHours(1)), name);

        /// <summary>
        /// Adds a throttlr to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="redisConnection"></param>
        /// <param name="memoryCacheConfiguration"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IServiceCollection AddThrottlr<T>(this IServiceCollection services,
                                                        IThrottlrConfiguration<T> configuration,
                                                        IConnectionMultiplexer redisConnection,
                                                        IMemoryCacheConfiguration memoryCacheConfiguration,
                                                        string name)
        {
            EnsureFactoryRegistration(services);

            IThrottlrFactory factory = GetThrottlrFactory(services);

            factory.CreateThrottlr(configuration,
                                   redisConnection,
                                   memoryCacheConfiguration,
                                   name);

            return services;
        }

        /// <summary>
        /// Ensures the <see cref="IThrottlrFactory"/> is registered.
        /// </summary>
        /// <param name="services"></param>
        private static void EnsureFactoryRegistration(IServiceCollection services) => services.TryAddSingleton<IThrottlrFactory, ThrottlrFactory>();

        /// <summary>
        /// Ensures the <see cref="IThrottlrRuleCache"/> is registered.
        /// </summary>
        /// <param name="services"></param>
        private static void EnsureRuleCacheRegistration(IServiceCollection services) => services.TryAddSingleton<IThrottlrRuleCache, ThrottlrRuleCache>();

        /// <summary>
        /// Gets the <see cref="IThrottlrFactory"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IThrottlrFactory GetThrottlrFactory(IServiceCollection services) => services.BuildServiceProvider().GetRequiredService<IThrottlrFactory>();

        /// <summary>
        /// Gets the <see cref="IThrottlrRuleCache"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IThrottlrRuleCache GetThrottlrRuleCache(IServiceCollection services) => services.BuildServiceProvider().GetRequiredService<IThrottlrRuleCache>();
    }
}