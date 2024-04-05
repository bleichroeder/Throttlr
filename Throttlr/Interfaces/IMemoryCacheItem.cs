using Throttlr.Interfaces;

namespace Throttlr
{
    /// <summary>
    /// The <see cref="IMemoryCacheItem{T}"/> interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMemoryCacheItem<T> where T : IThrottlrWindow
    {
        /// <summary>
        /// Gets the Item.
        /// </summary>
        IThrottlrWindow Item { get; }

        /// <summary>
        /// Gets the Expiration.
        /// </summary>
        DateTime Expiration { get; }

        /// <summary>
        /// Gets whether this item is expired.
        /// </summary>
        bool IsExpired { get; }
    }
}
