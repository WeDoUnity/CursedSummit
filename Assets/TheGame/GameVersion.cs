using System.IO;
using UnityEngine;

namespace TheGame
{
    public static class GameVersion
    {
        #region Constants
        private const string buildFile = "buildID.build";
        #endregion

        #region Static properties
        public static int Major { get; }
        public static int Minor { get; }
        public static int Build { get; }
        public static int Revision { get; }
        public static string Version { get; }
        #endregion

        #region Constructors
        static GameVersion()
        {
            Version = File.ReadAllLines(Path.Combine(Application.dataPath, buildFile))[0].Split('|')[1];
            string[] versionNumbers = Version.Split('.');
            Major    = int.Parse(versionNumbers[0]);
            Minor    = int.Parse(versionNumbers[1]);
            Build    = int.Parse(versionNumbers[2]);
            Revision = int.Parse(versionNumbers[3]);
        }
        #endregion
    }
}