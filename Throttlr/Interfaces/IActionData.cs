namespace Throttlr.Interfaces
{
    /// <summary>
    /// The <see cref="IActionData"/> interface.
    /// </summary>
    public interface IActionData
    {
        /// <summary>
        /// Gets or sets the action data TimeStamp.
        /// </summary>
        DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the action data Bytes.
        /// </summary>
        long Bytes { get; set; }
    }
}
