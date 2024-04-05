namespace Throttlr.Demo
{
    internal partial class Program
    {
        /// <summary>
        /// A simple user order class.
        /// </summary>
        public class UserOrder
        {
            /// <summary>
            /// The user.
            /// </summary>
            public User User { get; set; } = new();

            /// <summary>
            /// The order.
            /// </summary>
            public Order Order { get; set; } = new();
        }
    }
}