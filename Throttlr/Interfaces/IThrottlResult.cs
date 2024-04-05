namespace Throttlr.Interfaces
{
    /// <summary>
    /// The <see cref="IThrottlResult{T}"/> interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IThrottlResult<T>
    {
        /// <summary>
        /// Gets the Item.
        /// </summary>
        T Item { get; }

        /// <summary>
        /// Gets the SlidingWindow.
        /// </summary>
        IThrottlrWindow Window { get; }

        /// <summary>
        /// Gets a value indicating whether the action IsAllowed.
        /// </summary>
        bool IsAllowed { get; }

        /// <summary>
        /// Gets the Key.
        /// </summary>
        string Key { get; }
    }
}
