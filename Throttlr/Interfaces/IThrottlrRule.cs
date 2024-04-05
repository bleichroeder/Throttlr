using Throttlr.Models;

namespace Throttlr.Interfaces
{
    /// <summary>
    /// The <see cref="IThrottlrRule"/> interface.
    /// </summary>
    public interface IThrottlrRule
    {
        /// <summary>
        /// Gets the <see cref="IThrottlrRuleConfiguration"/>.
        /// </summary>
        ThrottlrRuleConfiguration RuleConfiguration { get; }

        /// <summary>
        /// Returns true if the rule applies to the target item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputConfiguration"></param>
        /// <param name="targetItem"></param>
        /// <returns></returns>
        bool Applies<T>(IThrottlrConfiguration<T> inputConfiguration, T targetItem);

        /// <summary>
        /// Returns true if the rule applies to the window.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        bool Applies(IThrottlrWindow window);
    }
}
