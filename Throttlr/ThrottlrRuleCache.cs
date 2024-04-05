using System.Collections.Concurrent;
using System.Text.Json;
using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Utilities;

namespace Throttlr
{
    /// <summary>
    /// Provides a cache for accessing available <see cref="IThrottlrRule"/>.
    /// </summary>
    public class ThrottlrRuleCache : IThrottlrRuleCache
    {
        private static readonly ConcurrentDictionary<string, IThrottlrRule> _rulesCache = new();

        private static readonly object _padLock = new();
        private static ThrottlrRuleCache? _defaultInstance = null;

        /// <summary>
        /// Gets the default instance of <see cref="ThrottlrRuleCache"/>.
        /// This instance acts as a Singleton for direct access outside DI.
        /// </summary>
        public static ThrottlrRuleCache Default
        {
            get
            {
                if (_defaultInstance is null)
                {
                    lock (_padLock)
                    {
                        _defaultInstance ??= new ThrottlrRuleCache();
                    }
                }

                return _defaultInstance;
            }
        }

        /// <summary>
        /// Tries to get a rule from the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="throttlrConfiguration"></param>
        /// <param name="targetItem"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool TryGetRule<T>(IThrottlrConfiguration<T> throttlrConfiguration, T targetItem, out IThrottlrRule? rule)
        {
            rule = null;

            foreach (var kvp in _rulesCache)
            {
                if (kvp.Value is ThrottlrRule typedRule && typedRule.Applies(throttlrConfiguration, targetItem))
                {
                    rule = typedRule;
                }
            }

            return rule is not null;
        }

        /// <summary>
        /// Tries to get a rule from the cache.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool TryGetRule(IThrottlrWindow window, out IThrottlrRule? rule)
        {
            rule = null;
            foreach (var kvp in _rulesCache)
            {
                if (kvp.Value is ThrottlrRule typedRule && typedRule.Applies(window))
                {
                    rule = typedRule;
                }
            }
            return rule is not null;
        }

        /// <summary>
        /// Adds or updates a rule to the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        public void AddOrUpdateRule(IThrottlrRule rule)
        {
            _rulesCache.AddOrUpdate(rule.RuleConfiguration.RuleName, rule, (key, existingVal) => rule);
        }

        /// <summary>
        /// Adds or updates a collection of rules to the cache.
        /// </summary>
        /// <param name="rules"></param>
        public void AddOrUpdateRules(IEnumerable<IThrottlrRule> rules)
        {
            foreach (var rule in rules)
            {
                _rulesCache.AddOrUpdate(rule.RuleConfiguration.RuleName, rule, (key, existingVal) => rule);
            }
        }

        /// <summary>
        /// Removes a rule from the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        public void RemoveRule(IThrottlrRule rule)
        {
            _rulesCache.TryRemove(rule.RuleConfiguration.RuleName, out _);
        }

        /// <summary>
        /// Loads rules from a JSON file.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        public async Task LoadRules(string jsonPath)
        {
            string json = await File.ReadAllTextAsync(jsonPath);

            var rules = JsonSerializer.Deserialize<IEnumerable<ThrottlrRule>>(json, Serialization.DefaultSerializerOptions);

            if (rules is not null)
            {
                AddOrUpdateRules(rules);
            }
        }

        /// <summary>
        /// Loads rules from a collection of JSON files.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="jsonPaths"></param>
        /// <returns></returns>
        public async Task LoadRules(IEnumerable<string> jsonPaths)
        {
            foreach (string path in jsonPaths)
                await LoadRules(path);
        }
    }
}
