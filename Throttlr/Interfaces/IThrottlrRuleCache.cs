namespace Throttlr.Interfaces
{
    /// <summary>
    /// The IRuleCache interface.
    /// </summary>
    public interface IThrottlrRuleCache
    {
        /// <summary>
        /// Tries to get a rule from the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="throttlrConfiguration"></param>
        /// <param name="targetItem"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        bool TryGetRule<T>(IThrottlrConfiguration<T> throttlrConfiguration, T targetItem, out IThrottlrRule? rule);

        /// <summary>
        /// Tries to get a rule from the cache.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        bool TryGetRule(IThrottlrWindow window, out IThrottlrRule? rule);

        /// <summary>
        /// Adds or updates a rule in the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        void AddOrUpdateRule(IThrottlrRule rule);

        /// <summary>
        /// Adds or updates a collection of rules in the cache.
        /// </summary>
        /// <param name="rules"></param>
        void AddOrUpdateRules(IEnumerable<IThrottlrRule> rules);

        /// <summary>
        /// Removes a rule from the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rule"></param>
        void RemoveRule(IThrottlrRule rule);
    }
}
