using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace CursedSummit
{
    /// <summary>
    /// Static class holding the game's Version info
    /// </summary>
    public static class GameVersion
    {
        #region Constants
        /// <summary>
        /// Name of the BuildID file
        /// </summary>
        private const string buildFile = "buildID.build";
        /// <summary>
        /// Separator of the build time and version number
        /// </summary>
        private static readonly string[] delim = { "UTC|v" };
        #endregion

        #region Static properties
        /// <summary>
        /// Major version number
        /// </summary>
        public static int Major { get; }
        /// <summary>
        /// Minor version number
        /// </summary>
        public static int Minor { get; }
        /// <summary>
        /// Build version number
        /// </summary>
        public static int Build { get; }
        /// <summary>
        /// Revision version number
        /// </summary>
        public static int Revision { get; }
        /// <summary>
        /// Full version string
        /// </summary>
        public static string VersionString { get; }
        /// <summary>
        /// Current game version
        /// </summary>
        public static Version Version { get; }
        /// <summary>
        /// UTC time of the Build
        /// </summary>
        public static DateTime BuildTime { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Loads the current game version from the BuildID file
        /// </summary>
        static GameVersion()
        {
            string path = Path.Combine(Application.dataPath, buildFile);
            string[] info;
            try
            {
                info = File.ReadAllLines(path)[0].Split(delim, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception e)
            {
                throw new FileNotFoundException("GameVersion could not properly load BuildID", path, e);
            }

            BuildTime = DateTime.SpecifyKind(DateTime.ParseExact(info[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), DateTimeKind.Utc);
            VersionString = info[1];
            Version = new Version(VersionString);

            Major    = Version.Major;
            Minor    = Version.Minor;
            Build    = Version.Build;
            Revision = Version.Revision;
        }
        #endregion
    }
}