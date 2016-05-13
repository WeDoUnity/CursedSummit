using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TheGame.Editor
{
    [InitializeOnLoad]
    public class Build
    {
        #region Constants
        private const string file = "buildID.build";
        private const string buildFile = "_Data/buildID.build";
        #endregion

        #region Static fields
        private static int major;
        private static int minor;
        private static int build;
        private static int revision;
        private static string date;
        private static readonly string path;
        #endregion

        #region Static properties
        public static string Version
        {
            get { return string.Format("{0}.{1}.{2}.{3}", major, minor, build, revision); }
        }

        public static string CurrentDate
        {
            get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); }
        }
        #endregion

        #region Constructors
        static Build()
        {
            path = Path.Combine(Application.dataPath, file);
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path)[0].Split('|');
                string[] version = lines[1].Split('.');
                date = lines[0];
                major = int.Parse(version[0]);
                minor = int.Parse(version[1]);
                build = int.Parse(version[2]);
                revision = int.Parse(version[3]);
            }
            else
            {
                minor = 1;
                date = CurrentDate;
            }
        }
        #endregion

        #region Methods
        [PostProcessBuild]
        public static void OnBuild(BuildTarget target, string pathToBuild)
        {
            revision++;
            date = CurrentDate;
            string buildID = Version;
            Debug.Log(pathToBuild);
            Debug.Log(string.Format("Building version {0}, at {1}", buildID, date));
            string[] lines =  { date + "|" + buildID };
            File.WriteAllLines(path, lines);
            File.WriteAllLines(Path.ChangeExtension(pathToBuild, null) + buildFile, lines);
        }

        [MenuItem("Version/Increase Major")]
        public static void IncreaseMajor()
        {
            major++;
            Debug.Log("Current version: " + Version);
        }

        [MenuItem("Version/Increase Minor")]
        public static void IncreaseMinor()
        {
            minor++;
            Debug.Log("Current version: " + Version);
        }

        [MenuItem("Version/Increase Build")]
        public static void IncreaseBuild()
        {
            build++;
            Debug.Log("Current version: " + Version);
        }

        [MenuItem("Version/Reset Major")]
        public static void ResetMajor()
        {
            major = 0;
            Debug.Log("Current version: " + Version);
        }

        [MenuItem("Version/Reset Minor")]
        public static void ResetMinor()
        {
            minor = 0;
            Debug.Log("Current version: " + Version);
        }

        [MenuItem("Version/Reset Build")]
        public static void ResetBuild()
        {
            build = 0;
            Debug.Log("Current version: " + Version);
        }

        [MenuItem("Version/Save Version")]
        public static void SaveVersion()
        {
            string buildID = Version;
            Debug.Log("Saving version " + buildID);
            string[] lines = { date + "|" + buildID };
            File.WriteAllLines(path, lines);
        }
        #endregion
    }
}
