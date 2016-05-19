using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CursedSummit.Utils;
using Object = UnityEngine.Object;

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

    /// <summary>
    /// Path related usage string extension methods
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

    /// <summary>
    /// Holds method extensions for the System.Reflection.Assembly type
    /// </summary>
    public static class AssemblyExtensions
    {
        #region Methods
        /// <summary>
        /// Returns the Version information for this assembly
        /// </summary>
        /// <param name="assembly">Assembly to get the version for</param>
        /// <returns>Assembly informational version if any exists, else regular assembly version</returns>
        public static Version GetVersion(this Assembly assembly)
        {
            string info = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            return string.IsNullOrEmpty(info) ? assembly.GetName().Version : new Version(info);
        }
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
