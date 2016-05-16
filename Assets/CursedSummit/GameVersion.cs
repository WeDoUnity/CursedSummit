using System;
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
        #endregion

        #region Constructors
        /// <summary>
        /// Loads the current game version from the BuildID file
        /// </summary>
        static GameVersion()
        {
            string path = Path.Combine(Application.dataPath, buildFile);
            try
            {
                VersionString = File.ReadAllLines(path)[0].Split('|')[1];
            }
            catch (Exception e)
            {
                throw new FileNotFoundException("GameVersion could not properly load BuildID", path, e);
            }
            VersionString = File.ReadAllLines(path)[0].Split('|')[1];
            Version = new Version(VersionString);
            string[] versionNumbers = VersionString.Split('.');
            Major    = int.Parse(versionNumbers[0]);
            Minor    = int.Parse(versionNumbers[1]);
            Build    = int.Parse(versionNumbers[2]);
            Revision = int.Parse(versionNumbers[3]);
        }
        #endregion
    }
}