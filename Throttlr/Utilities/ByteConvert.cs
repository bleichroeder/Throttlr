namespace Throttlr.Utilities
{
    /// <summary>
    /// Data size converters.
    /// </summary>
    public static class ByteConvert
    {
        // Some constants for converting bytes to other units.
        private const long BYTES_PER_KILOBYTE = 1024;
        private const long BYTES_PER_MEGABYTE = 1048576;
        private const long BYTES_PER_GIGABYTE = 1073741824;
        private const long BYTES_PER_TERABYTE = 1099511627776;

        /// <summary>
        /// Convert bytes to terabytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static long ToTerabytes(this long bytes) => bytes / BYTES_PER_TERABYTE;

        /// <summary>
        /// Convert bytes to gigabytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static long ToGigabytes(this long bytes) => bytes / BYTES_PER_GIGABYTE;

        /// <summary>
        /// Convert bytes to megabytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static long ToMegabytes(this long bytes) => bytes / BYTES_PER_MEGABYTE;

        /// <summary>
        /// Convert bytes to kilobytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static long ToKilobytes(this long bytes) => bytes / BYTES_PER_KILOBYTE;

        /// <summary>
        /// Convert terabytes to bytes.
        /// </summary>
        /// <param name="terabytes"></param>
        /// <returns></returns>
        public static long FromTerabytes(this long terabytes) => terabytes * BYTES_PER_TERABYTE;

        /// <summary>
        /// Convert gigabytes to bytes.
        /// </summary>
        /// <param name="gigabytes"></param>
        /// <returns></returns>
        public static long FromGigabytes(this long gigabytes) => gigabytes * BYTES_PER_GIGABYTE;

        /// <summary>
        /// Convert megabytes to bytes.
        /// </summary>
        /// <param name="megabytes"></param>
        /// <returns></returns>
        public static long FromMegabytes(this long megabytes) => megabytes * BYTES_PER_MEGABYTE;

        /// <summary>
        /// Convert kilobytes to bytes.
        /// </summary>
        /// <param name="kilobytes"></param>
        /// <returns></returns>
        public static long FromKilobytes(this long kilobytes) => kilobytes * BYTES_PER_KILOBYTE;
    }
}
