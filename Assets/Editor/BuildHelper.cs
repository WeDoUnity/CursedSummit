using System;
using System.CodeDom;
using System.Globalization;
using System.IO;
using CursedSummit.Utils;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CursedSummit.Editor
{
    /// <summary>
    /// Static class containing helper methods for the Unity editor with build versioning
    /// </summary>
    [InitializeOnLoad]
    public static class Build
    {
        #region Constants
        /// <summary>
        /// Name of the BuildID file
        /// </summary>
        private const string FileName = "buildID.build";
        /// <summary>
        /// Game build folder path of the BuildID file
        /// </summary>
        private const string BuildFileName = "_Data/buildID.build";
        /// <summary>
        /// Separator of the build time and version number
        /// </summary>
        private static readonly string[] Delim = { "UTC|v" };
        /// <summary>
        /// File path of the BuildID
        /// </summary>
        private static readonly string FilePath = Path.Combine(Application.dataPath, FileName);
        /// <summary>
        /// Full folder path to the CSData folder
        /// </summary>
        private static readonly DirectoryInfo CSDataDir = new DirectoryInfo(Path.Combine(Application.dataPath, "/../" + CSUtils.CSDataFolderName));
        #endregion

        #region Static fields
        private static int major, minor, build, revision;   //Version number
        private static string date;                         //Last build date
        #endregion

        #region Static properties
        /// <summary>
        /// Version number of the game
        /// </summary>
        public static string Version
        {
            get { return string.Format("{0}.{1}.{2}.{3}", major, minor, build, revision); }
        }

        /// <summary>
        /// Current DateTime string, correctly formatted
        /// </summary>
        public static string CurrentDate
        {
            get { return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes BuildID components
        /// </summary>
        static Build()
        {
            if (File.Exists(FilePath))
            {
                string[] info = File.ReadAllLines(FilePath)[0].Split(Delim, StringSplitOptions.RemoveEmptyEntries);
                date = info[0];
                string[] version = info[1].Split('.');
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

        #region Static methods
        /// <summary>
        /// Runs after each game build process, and saves the new BuildID to the game folder
        /// </summary>
        /// <param name="target">Target platform of the build</param>
        /// <param name="pathToBuild">Full file path of the build executable</param>
        [PostProcessBuild]
        public static void OnBuild(BuildTarget target, string pathToBuild)
        {
            //Setup build file
            revision++;
            date = CurrentDate;
            string buildID = Version;
            Debug.Log(string.Format("Built version v{0}, at {1}UTC", buildID, date));
            string[] lines =  { date + "UTC|v" + buildID };

            //Write build file
            File.WriteAllLines(FilePath, lines);
            File.WriteAllLines(Path.ChangeExtension(pathToBuild, null) + BuildFileName, lines);

            //Copy CSData directory
            string destination = Path.Combine(new FileInfo(pathToBuild).DirectoryName, CSUtils.CSDataFolderName);
            CopyDirectory(CSDataDir, Directory.Exists(destination) ? new DirectoryInfo(destination) : Directory.CreateDirectory(destination));
        }

        /// <summary>
        /// Fully copies a directory and all it's files to a target directory
        /// </summary>
        /// <param name="source">Source directory</param>
        /// <param name="target">Target directory</param>
        private static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                CopyDirectory(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }

        /// <summary>
        /// Increments the Major version of the build number
        /// </summary>
        [MenuItem("Version/Increase Major")]
        public static void IncreaseMajor()
        {
            major++;
            LogVersion();
        }

        /// <summary>
        /// Increments the Minor version of the build number
        /// </summary>
        [MenuItem("Version/Increase Minor")]
        public static void IncreaseMinor()
        {
            minor++;
            LogVersion();
        }

        /// <summary>
        /// Increments the Build version of the build number
        /// </summary>
        [MenuItem("Version/Increase Build")]
        public static void IncreaseBuild()
        {
            build++;
            LogVersion();
        }

        /// <summary>
        /// Resets the Major version of the build number to zero
        /// </summary>
        [MenuItem("Version/Reset Major")]
        public static void ResetMajor()
        {
            major = 0;
            LogVersion();
        }

        /// <summary>
        /// Resets the Minor version of the build number to zero
        /// </summary>
        [MenuItem("Version/Reset Minor")]
        public static void ResetMinor()
        {
            minor = 0;
            LogVersion();
        }

        /// <summary>
        /// Resets the Build version of the build number to zero
        /// </summary>
        [MenuItem("Version/Reset Build")]
        public static void ResetBuild()
        {
            build = 0;
            LogVersion();
        }

        /// <summary>
        /// Saves the current BuildID to the Assets file
        /// </summary>
        [MenuItem("Version/Save Version")]
        public static void SaveVersion()
        {
            string buildID = Version;
            Debug.Log("Saving version v" + buildID);
            File.WriteAllLines(FilePath, new[] { date + "UTC|v" + buildID });
        }

        /// <summary>
        /// Logs the current BuildID and saves the current Date
        /// </summary>
        private static void LogVersion()
        {
            date = CurrentDate;
            Debug.Log("Current version: v" + Version);
        }
        #endregion
    }
}
