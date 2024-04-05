using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Windows.RateLimiting.Sliding;

namespace Throttlr.Tests
{
    public class RulesTests
    {
        [Fact]
        public async Task Rules_LoadFromJson()
        {
            // Load the rules from a JSON file.
            await ThrottlrRuleCache.Default.LoadRules("Assets/Rules.json");

            // Create a window.
            SlidingRateLimitingThrottlrWindow window = new("Test", 5, TimeSpan.FromSeconds(10));

            // Try and get the rule for the window.
            ThrottlrRuleCache.Default.TryGetRule(window, out IThrottlrRule? rule);

            // We'll apply the rule to the window.
            for (int i = 0; i < 5; i++)
            {
                window.IsAllowed(new ActionData(), rule);
            }

            Assert.Equal(1, window.AllowedActionsCount);
        }
    }
}