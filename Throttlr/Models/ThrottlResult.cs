using Throttlr.Interfaces;

namespace Throttlr.Models
{
    /// <summary>
    /// The <see cref="ThrottlResult{T}"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThrottlResult<T> : IThrottlResult<T>
    {
        /// <summary>
        /// Gets the Item.
        /// </summary>
        public T Item => _item;

        /// <summary>
        /// Gets the <see cref="IThrottlrWindow"/>.
        /// </summary>
        public IThrottlrWindow Window => _window;

        /// <summary>
        /// Gets a value indicating whether the action is allowed.
        /// </summary>
        public bool IsAllowed => _isAllowed;

        /// <summary>
        /// Gets the Key.
        /// </summary>
        public string Key => _key;

        private readonly IThrottlrWindow _window;
        private readonly string _key;
        private readonly T _item;
        private readonly bool _isAllowed;

        /// <summary>
        /// Creates a new instance of <see cref="ThrottlResult{T}"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <param name="window"></param>
        /// <param name="actionData"></param>
        public ThrottlResult(T item, string key, IThrottlrWindow window, ActionData actionData)
            : this(item, key, window, actionData, null)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="ThrottlResult{T}"/>.
        /// </summary>
        /// <remarks>
        /// Applies the rule to the window.
        /// </remarks>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <param name="window"></param>
        public ThrottlResult(T item, string key, IThrottlrWindow window, ActionData actionData, IThrottlrRule? rule)
        {
            _item = item;
            _key = key;
            _window = window;
            _isAllowed = _window.IsAllowed(actionData, rule);
        }
    }
}
