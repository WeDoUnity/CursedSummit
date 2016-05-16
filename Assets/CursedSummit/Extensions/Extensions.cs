using UnityEngine;

namespace CursedSummit.Extensions
{
    /// <summary>
    /// Holds method extensions for the UnityEngine.Object type
    /// </summary>
    public static class UnityObjectExtensions
    {
        #region Methods
        /// <summary>
        /// Calls Object.Destroy on this Object
        /// </summary>
        /// <param name="obj">Object to destroy</param>
        public static void DestroyThis(this Object obj) => Object.Destroy(obj);
        #endregion
    }
}

namespace System.Diagnostics
{
    /// <summary>
    /// Holds method extensions for the System.Diagnostics.Stopwatch type
    /// </summary>
    public static class StopwatchExtensions
    {
        #region Methods
        /// <summary>
        /// Resets the Stopwatch to zero and starts it again
        /// </summary>
        /// <param name="watch">Watch to restart</param>
        public static void Restart(this Stopwatch watch)
        {
            watch.Reset();
            watch.Start();
        }
        #endregion
    }
}
