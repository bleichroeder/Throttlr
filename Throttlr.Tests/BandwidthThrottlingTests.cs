using Throttlr.Models;
using Throttlr.Utilities;
using Throttlr.Windows.RateLimiting.Sliding;

namespace Throttlr.Tests
{
    public class BandwidthThrottlingTests
    {
        private const string THROTTLR_NAME = "BandwidthTest";

        [Fact]
        public void BandwidthLimiting_CheckIsAllowed_ShouldBeFalse()
        {
            long limitBytes = ByteConvert.FromMegabytes(100);
            long actionBytes = ByteConvert.FromMegabytes(101);

            // Create a new sliding window with a limit of 100MB and a 10 second expiry.
            SlidingBandwidthLimitingThrottlrWindow window = new(THROTTLR_NAME, limitBytes, TimeSpan.FromSeconds(10));

            // Check if we can allow an action of 101MB.
            bool allowed = window.IsAllowed(new ActionData(actionBytes));

            // We should not be allowed to perform an action of 101MB.
            Assert.False(allowed);
        }

        [Fact]
        public void BandwidthLimiting_CheckIsAllowed_ShouldBeTrue()
        {
            long limitBytes = ByteConvert.FromMegabytes(100);
            long actionBytes = ByteConvert.FromMegabytes(99);

            // Create a new sliding window with a limit of 100MB and a 10 second expiry.
            SlidingBandwidthLimitingThrottlrWindow window = new(THROTTLR_NAME, limitBytes, TimeSpan.FromSeconds(10));

            // Check if we can allow an action of 101MB.
            bool allowed = window.IsAllowed(new ActionData(actionBytes));

            // We should not be allowed to perform an action of 101MB.
            Assert.True(allowed);
        }

        [Fact]
        public void BandwidthLimiting_CheckIsAllowed_10Requests_ShouldBeTrue()
        {
            long limitBytes = 10000;
            long actionBytes = 900;
            int requests = 10;
            long expectedRemainingBytes = limitBytes - (900 * requests);

            // Create a new sliding window with a limit of 100MB and a 10 second expiry.
            SlidingBandwidthLimitingThrottlrWindow window = new(THROTTLR_NAME, limitBytes, TimeSpan.FromMinutes(10));

            // Make 10 requests of 9MB.
            bool allowed = false;

            for (int i = 0; i < requests; i++)
            {
                // Check if we can allow an action of 9MB.
                allowed = window.IsAllowed(new ActionData(actionBytes));
            }

            // Get the remaining bandwidth in the window.
            long remainingBytes = window.RateLimit_Remaining;

            // We should still have 10Mb remaining in our window, and should be allowed to perform an action of 9MB.
            Assert.True(remainingBytes == expectedRemainingBytes && allowed);
        }

        [Fact]
        public void BandwidthLimiting_CheckIsAllowed_13Requests_ShouldBeFalse()
        {
            long limitBytes = 10000;
            long actionBytes = 900;
            int requests = 13;

            // Create a new sliding window with a limit of 100MB and a 10 second expiry.
            SlidingBandwidthLimitingThrottlrWindow window = new(THROTTLR_NAME, limitBytes, TimeSpan.FromMinutes(10));

            // Make 10 requests of 9MB.
            bool allowed = false;

            for (int i = 0; i < requests; i++)
            {
                // Check if we can allow an action of 9MB.
                allowed = window.IsAllowed(new ActionData(actionBytes));
            }

            // Get the remaining bandwidth in the window.
            long remainingBytes = window.RateLimit_Remaining;

            // We should still have 10Mb remaining in our window, and should be allowed to perform an action of 9MB.
            Assert.True(remainingBytes == 0 && allowed is false);
        }
    }
}