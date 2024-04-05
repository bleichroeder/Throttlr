using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Tests.Models;
using Throttlr.Windows.RateLimiting.Fixed;

namespace Throttlr.Tests
{
    public class FixedThrottlrWindowTests
    {
        [Fact]
        public void FixedWindow_AllowsRequestsUpToLimit()
        {
            var window = new FixedRateLimitingThrottlrWindow("Test", 5, TimeSpan.FromSeconds(10));

            for (int i = 0; i < 5; i++)
            {
                Assert.True(window.IsAllowed(new ActionData()));
            }

            Assert.Equal(5, window.AllowedActionsCount);
        }

        [Fact]
        public void FixedWindow_ThrottlesRequestsAfterLimit()
        {
            var window = new FixedRateLimitingThrottlrWindow("Test", 5, TimeSpan.FromSeconds(10));

            for (int i = 0; i < 5; i++)
            {
                window.IsAllowed(new ActionData());
            }

            Assert.False(window.IsAllowed(new ActionData()));
        }

        [Fact]
        public void FixedWindow_AllowsAdditionalRequestsAfterExpiry()
        {
            var window = new FixedRateLimitingThrottlrWindow("Test", 5, TimeSpan.FromSeconds(1));

            for (int i = 0; i < 5; i++)
            {
                window.IsAllowed(new ActionData());
            }

            Thread.Sleep(1100); // Wait for 1.1 seconds for requests to expire

            Assert.True(window.IsAllowed(new ActionData()));
        }

        [Fact]
        public void FixedWindow_SerializeDeserialize()
        {
            FixedRateLimitingThrottlrWindow window = new("Test", 5, TimeSpan.FromSeconds(10));
            string serialized = window.Serialize();

            FixedRateLimitingThrottlrWindow deserialized = IThrottlrWindow.Deserialize<FixedRateLimitingThrottlrWindow>(serialized);

            Assert.NotNull(deserialized);
            Assert.Equal(window.ThrottlrName, deserialized.ThrottlrName);
            Assert.Equal(window.MaxActions, deserialized.MaxActions);
            Assert.Equal(window.TimeWindow, deserialized.TimeWindow);
        }

        [Fact]
        public void ThrottlResult_CheckIsAllowed()
        {
            var window = new FixedRateLimitingThrottlrWindow("Test", 5, TimeSpan.FromSeconds(10));
            var item = new TestItem { PropA = "A", PropB = "B" };

            var result = new ThrottlResult<TestItem>(item, "Key", window, new ActionData());

            Assert.True(result.IsAllowed);
        }
    }
}