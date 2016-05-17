
namespace CursedSummit.Extensions
{
    using System.IO;
    using UnityEngine;

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

    /// <summary>
    /// Path related usage string extensions
    /// </summary>
    public static class StringExtensions
    {
        #region Methods
        /// <summary>
        /// Formats a string path by transforming all separator chars to '/' and removing trailing and ending
        /// instances of this char
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FormatPath(this string s) => s.Replace(Path.DirectorySeparatorChar, '/').Trim('/');
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

namespace System.IO
{
    using CursedSummit.Utils;

    /// <summary>
    /// Holds method extensions for the System.IO.FileInfo type
    /// </summary>
    public static class FileInfoExtensions
    {
        #region Methods
        /// <summary>
        /// Opens and reads a file to the end
        /// </summary>
        /// <param name="file">File to read</param>
        /// <returns>The whole text within the file</returns>
        public static string ReadFile(this FileInfo file)
        {
            using (StreamReader r = file.OpenText())
            {
                return r.ReadToEnd();
            }
        }

        /// <summary>
        /// Gets the local path to the current file from the current CSUtils workind directory
        /// </summary>
        /// <param name="file">File to get the path path for</param>
        /// <returns>Local path of this file</returns>
        public static string GetLocalPath(this FileInfo file) => file.FullName.Replace('\\', '/').Replace(CSUtils.CurrentDirectory, null).Trim('/');
        #endregion
    }
}
