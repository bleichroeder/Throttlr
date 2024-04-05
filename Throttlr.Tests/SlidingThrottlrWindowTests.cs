using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Tests.Models;
using Throttlr.Windows.RateLimiting.Sliding;

namespace Throttlr.Tests
{
    public class SlidingThrottlrWindowTests
    {
        [Fact]
        public void SlidingWindow_AllowsRequestsUpToLimit()
        {
            var window = new SlidingRateLimitingThrottlrWindow("Test", 5, TimeSpan.FromSeconds(10));

            for (int i = 0; i < 5; i++)
            {
                Assert.True(window.IsAllowed(new ActionData()));
            }

            Assert.Equal(5, window.AllowedActionsCount);
        }

        [Fact]
        public void SlidingWindow_ThrottlesRequestsAfterLimit()
        {
            var window = new SlidingRateLimitingThrottlrWindow("Test", 5, TimeSpan.FromSeconds(10));

            for (int i = 0; i < 5; i++)
            {
                window.IsAllowed(new ActionData());
            }

            Assert.False(window.IsAllowed(new ActionData()));
        }

        [Fact]
        public void SlidingWindow_AllowsAdditionalRequestsAfterExpiry()
        {
            var window = new SlidingRateLimitingThrottlrWindow("Test", 5, TimeSpan.FromSeconds(1));

            for (int i = 0; i < 5; i++)
            {
                window.IsAllowed(new ActionData());
            }

            Thread.Sleep(1100); // Wait for 1.1 seconds for requests to expire

            Assert.True(window.IsAllowed(new ActionData()));
        }

        [Fact]
        public void SlidingWindow_SerializeDeserialize()
        {
            var window = new SlidingRateLimitingThrottlrWindow("Test", 5, TimeSpan.FromSeconds(10));
            string serialized = window.Serialize();

            SlidingRateLimitingThrottlrWindow deserialized = IThrottlrWindow.Deserialize<SlidingRateLimitingThrottlrWindow>(serialized);

            Assert.NotNull(deserialized);
            Assert.Equal(window.ThrottlrName, deserialized.ThrottlrName);
            Assert.Equal(window.MaxActions, deserialized.MaxActions);
            Assert.Equal(window.TimeWindow, deserialized.TimeWindow);
        }

        [Fact]
        public void ThrottlResult_CheckIsAllowed()
        {
            var window = new SlidingRateLimitingThrottlrWindow("Test", 5, TimeSpan.FromSeconds(10));
            var item = new TestItem { PropA = "A", PropB = "B" };

            var result = new ThrottlResult<TestItem>(item, "Key", window, new ActionData());

            Assert.True(result.IsAllowed);
        }
    }
}