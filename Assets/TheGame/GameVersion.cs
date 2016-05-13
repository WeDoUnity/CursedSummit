using System.IO;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("0.1.*")]

namespace TheGame
{
    public static class GameVersion
    {
        #region Constants
        private const string buildLocation = "/Resources/buildID.build";
        #endregion

        #region Static properties
        public static int Major    { get; }
        public static int Minor    { get; }
        public static int Build    { get; }
        public static int Revision { get; }
        #endregion

        #region Constructors
        static GameVersion()
        {
            string[] version = File.ReadAllLines(Path.Combine(Application.dataPath, buildLocation))[0].Split('|')[1].Split('.');
            Major    = int.Parse(version[0]);
            Minor    = int.Parse(version[1]);
            Build    = int.Parse(version[2]);
            Revision = int.Parse(version[3]);
        }
        #endregion
    }
}
