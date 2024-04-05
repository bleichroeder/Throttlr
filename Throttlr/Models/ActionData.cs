using Throttlr.Interfaces;

namespace Throttlr.Models
{
    /// <summary>
    /// Creates a new instance of <see cref="ActionData"/>.
    /// </summary>
    /// <param name="bytes"></param>
    public class ActionData(long bytes = 0) : IActionData
    {
        /// <summary>
        /// Gets or sets the Timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the number of bytes sent.
        /// </summary>
        public long Bytes { get; set; } = bytes;
    }
}
