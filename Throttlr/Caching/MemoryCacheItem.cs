using Throttlr.Interfaces;

namespace Throttlr.Caching
{
    /// <summary>
    /// The <see cref="MemoryCacheItem{T}"/>.
    /// </summary>
    /// <remarks>
    /// Used as a storage container for entries in the <see cref="IThrottlr{T}"/> InMemoryCache.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Creates a new <see cref="MemoryCacheItem{T}"/>.
    /// </remarks>
    /// <param name="item"></param>
    /// <param name="expiration"></param>
    public class MemoryCacheItem<T>(IThrottlrWindow item, DateTime expiration) : IMemoryCacheItem<T> where T : IThrottlrWindow
    {

        /// <summary>
        /// Gets the Item.
        /// </summary>
        public IThrottlrWindow Item { get; } = item;

        /// <summary>
        /// Gets the Expiration <see cref="DateTime"/> of the <see cref="MemoryCacheItem{T}"/>.
        /// </summary>
        public DateTime Expiration { get; } = expiration;

        /// <summary>
        /// Gets the IsExpired value.
        /// </summary>
        /// <remarks>
        /// A <see cref="MemoryCacheItem{T}"/> is considered expired if the <see cref="Expiration"/> <see cref="DateTime"/> is less than the current <see cref="DateTime"/>.
        /// </remarks>
        public bool IsExpired => DateTime.UtcNow > Expiration;
    }
}
