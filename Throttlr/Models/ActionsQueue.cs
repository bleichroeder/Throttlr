using Throttlr.Interfaces;

namespace Throttlr.Models
{
    /// <summary>
    /// The <see cref="ActionsQueue{T}"/> class.
    /// </summary>
    /// <remarks>
    /// A simple wrapper around the <see cref="Queue{T}"/> class
    /// which provices specialized methods for interacting with a queue of <see cref="IActionData"/>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class ActionsQueue<T> : Queue<T> where T : IActionData
    {
        /// <summary>
        /// Compares the total number of bytes in the queue plus the additional bytes to the max bytes allowed.
        /// </summary>
        /// <param name="additionalBytes"></param>
        /// <param name="maxBytes"></param>
        /// <returns></returns>
        public bool AdditionalBytesAllowed(long additionalBytes, long maxBytes) =>
            TotalActionBytes + additionalBytes <= maxBytes;

        /// <summary>
        /// Gets the sum of all the <see cref="IActionData.Bytes"/> in the queue.
        /// </summary>
        public long TotalActionBytes => this.Sum(x => x.Bytes);

        /// <summary>
        /// Gets the total duration of all the <see cref="IActionData"/> in the queue.
        /// </summary>
        public TimeSpan TotalActionsDuration
        {
            get
            {
                // If there are no actions in the queue, return zero.
                if (Count == 0)
                {
                    return TimeSpan.Zero;
                }

                // Get the first and last action in the queue.
                DateTime firstAction = Peek().Timestamp;
                DateTime lastAction = this.Last().Timestamp;

                // Return the difference between the first and last action.
                return lastAction - firstAction;
            }
        }
    }
}
