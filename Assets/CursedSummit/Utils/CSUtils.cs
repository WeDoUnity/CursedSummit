using System.IO;
using CursedSummit.Extensions;
using UnityEngine;
using static UnityEngine.Mathf;

namespace CursedSummit.Utils
{
    /// <summary>
    /// General usage static utils properties and methods
    /// </summary>
    public static class CSUtils
    {
        #region Constants
        /// <summary>
        /// Cursed Summit game data folder name
        /// </summary>
        public const string CSDataFolderName = "CSData";
        /// <summary>
        /// Original current directory
        /// </summary>
        private static readonly string OriginalDirectory;
        /// <summary>
        /// Directory separator characters
        /// </summary>
        private static readonly char[] Separators = { '/' };
        #endregion

        #region Static properties
        /// <summary>
        /// Cursed Summit root folder URL
        /// </summary>
        public static string RootPath { get; }

        /// <summary>
        /// Cursed Summit game data folder URL
        /// </summary>
        public static string CSDataPath { get; }

        /// <summary>
        /// Current working directory of CSUtils
        /// </summary>
        public static string CurrentDirectory { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initiates the fields and properties of this utils class
        /// </summary>
        static CSUtils()
        {
            RootPath = Path.Combine(Application.dataPath, "../").FormatPath();
            CSDataPath = Path.Combine(RootPath, CSDataFolderName).FormatPath();
            OriginalDirectory = Directory.GetCurrentDirectory().FormatPath();
            CurrentDirectory = OriginalDirectory;
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Clamps a given Vector2 between a set of minimum and maximum values
        /// </summary>
        /// <param name="v">Vector to clamp</param>
        /// <param name="min">Minimum components vector</param>
        /// <param name="max">Maximum components vector</param>
        /// <returns>A new Vector2, corretly clamped</returns>
        public static Vector2 ClampVector2(Vector2 v, Vector2 min, Vector2 max) => new Vector2(Clamp(v.x, min.x, max.x), Clamp(v.y, min.y, max.y));

        /// <summary>
        /// Sets the current CSUtils working directory
        /// </summary>
        /// <param name="path">Path to set the workind directory to</param>
        public static void SetCurrentDirectory(string path) => CurrentDirectory = path.FormatPath();

        /// <summary>
        /// Resets the current CSUtils working directory to the original workind directory
        /// </summary>
        public static void ResetCurrentDirectory() => CurrentDirectory = OriginalDirectory;
        #endregion
    }
}
