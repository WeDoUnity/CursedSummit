using System.IO;
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
        public const string DataFolderName = "CSData";
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
        #endregion

        #region Constructors
        /// <summary>
        /// Initiates the fields and properties of this utils class
        /// </summary>
        static CSUtils()
        {
            RootPath = Path.Combine(Application.dataPath, "/../");
            CSDataPath = Path.Combine(RootPath, DataFolderName);
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
        #endregion
    }
}
