using Throttlr.Models;

namespace Throttlr.Interfaces
{
    /// <summary>
    /// The <see cref="IThrottlr{T}"/> interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IThrottlr<T>
    {
        /// <summary>
        /// Gets the <see cref="LimiterType"/> from the underlying <see cref="IThrottlrWindow"/>.
        /// </summary>
        LimiterType LimiterType { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="IThrottlr{T}"/> window.
        /// </summary>
        Type WindowType { get; }

        /// <summary>
        /// Asyncronously gets a <see cref="ThrottlResult{T}"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="actionData"></param>
        /// <returns></returns>
        Task<ThrottlResult<T>> CanIAsync(T item, ActionData actionData);

        /// <summary>
        /// Asyncronously gets a <see cref="ThrottlResult{T}"/>.
        /// Overrides the OnSuccess and OnFailure actions.
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="actionData"></param>
        /// <param name="onSuccessAction"></param>
        /// <param name="onFailureAction"></param>
        /// <returns></returns>
        Task<ThrottlResult<T>> CanIAsync(T targetItem,
                                         ActionData actionData,
                                         Func<ThrottlResult<T>, Task<bool>> onSuccessAction,
                                         Func<ThrottlResult<T>, Task<bool>> onFailureAction);

        /// <summary>
        /// Asyncronously gets a <see cref="IThrottlrWindow"/> from the cache.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<IThrottlrWindow> GetWindowAsync(T item);

        /// <summary>
        /// Asyncronously gets a <see cref="IThrottlrWindow"/> from the cache.
        /// </summary>
        /// <returns></returns>
        Task<IThrottlrWindow> GetWindowAsync(T item, IEnumerable<ActionData>? preSeed = null);

        /// <summary>
        /// Creates a new <see cref="IThrottlrWindow"/>.
        /// </summary>
        /// <returns><see cref="IThrottlrWindow"/></returns>
        IThrottlrWindow CreateWindow();

        /// <summary>
        /// Creates a new <see cref="IThrottlrWindow"/>.
        /// Seeds the window with the specified <see cref="DateTime"/>s if specified.
        /// </summary>
        /// <remarks>
        /// Allows for pre-seeding of the window with the specified <see cref="ActionData"/> collection.
        /// The provided <see cref="ActionData"/> are enqueued in the order they are provided.
        /// </remarks>
        /// <returns><see cref="IThrottlrWindow"/></returns>
        IThrottlrWindow CreateWindow(IEnumerable<ActionData>? preSeed = null);
    }
}
