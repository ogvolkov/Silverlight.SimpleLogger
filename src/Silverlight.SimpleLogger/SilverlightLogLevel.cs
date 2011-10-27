namespace Silverlight.SimpleLogger
{
    /// <summary>
    /// Levels for <c>Silverlight</c> logging
    /// </summary>
    public static class SilverlightLogLevel
    {
        /// <summary>
        /// Everything is logged
        /// </summary>
        public const int All = 0;

        /// <summary>
        /// Only informational messages are logged
        /// </summary>
        public const int Info = 1000;

        /// <summary>
        /// Only errors are logged
        /// </summary>
        public const int Error = 4000;

        /// <summary>
        /// Nothing goes into log
        /// </summary>
        public const int Off = 10000;
    }
}
